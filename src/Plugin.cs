using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Combat;
using Dungeon_Generation;
using Dungeon_Generation.Objects;
using Enemies;
using Events;
using HarmonyLib;
using Managers;
using UnityEngine;

namespace TinyRogues.TrainingDummy;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BasePlugin
{
    public const string Guid = "tanner.tinyrogues.trainingdummy";
    public const string Name = "Tiny Rogues Training Dummy";
    public const string Version = "1.0.4";
    public const string SupportedGameVersion = "0.2.8.6";

    private const float DefaultIdleResetSeconds = 3f;
    private const float DefaultDummyOffsetX = 4.5f;
    private const float DefaultDummyOffsetY = 0f;
    private const float RoomEdgePadding = 1.25f;

    internal static Plugin? Instance;

    internal static bool IsSpawningTrainingDummy
    {
        get;
        private set;
    }

    private ConfigEntry<bool>? _showOverlayConfig;
    private ConfigEntry<float>? _idleResetConfig;
    private ConfigEntry<float>? _dummyOffsetXConfig;
    private ConfigEntry<float>? _dummyOffsetYConfig;
    private ConfigEntry<float>? _textScaleConfig;
    private ConfigEntry<bool>? _debugLoggingConfig;

    private TemporaryEvents? _temporaryEvents;
    private TemporaryEvents.DamageTakenHandler? _damageHandler;
    private Il2CppSystem.Action? _afterCompletedRoomHandler;

    private global::Player.Player? _currentPlayer;
    private Enemy? _dummyEnemy;
    private EnemyManager? _dummyEnemyManager;

    private int _runPlayerInstanceId = -1;
    private int _currentBonfireId = -1;

    private bool _pendingSpawn;
    private Vector3 _pendingSpawnPosition;
    private float _pendingSpawnDeadline;
    private float _nextSpawnRetry;
    private bool _pendingPostCombatSpawn;
    private bool _pendingBenchmarkReset;

    private float _totalDamage;
    private float _peakDamage;
    private float _firstDamageTime = -1f;
    private float _lastDamageTime = -1f;
    private int _measurementEventCount;

    // Completed and previous benchmark result state.

    private bool _hasCompletedResult;
    private float _completedDps;

    private bool _hasPreviousResult;
    private float _previousDps;

    internal bool ShowOverlay =>
        _showOverlayConfig?.Value ?? true;

    internal float IdleResetSeconds =>
        Mathf.Max(
            0.5f,
            _idleResetConfig?.Value ??
            DefaultIdleResetSeconds
        );

    internal float DummyOffsetX =>
        _dummyOffsetXConfig?.Value ??
        DefaultDummyOffsetX;

    internal float DummyOffsetY =>
        _dummyOffsetYConfig?.Value ??
        DefaultDummyOffsetY;

    internal float TextScale =>
        Mathf.Clamp(
            _textScaleConfig?.Value ?? 1f,
            0.5f,
            2f
        );

    internal bool DebugLogging =>
        _debugLoggingConfig?.Value ?? false;

    internal GameObject? TrackedDummyObject
    {
        get
        {
            if (!HasLiveDummy())
                return null;

            return _dummyEnemy!.gameObject;
        }
    }

    internal bool MeasurementStarted =>
        _firstDamageTime >= 0f;

    internal bool HasCompletedResult =>
        _hasCompletedResult;

    internal float CompletedDps =>
        _completedDps;

    internal bool HasPreviousResult =>
        _hasPreviousResult;

    internal float PreviousDps =>
        _previousDps;

    internal float MeasurementTime
    {
        get
        {
            if (
                _firstDamageTime < 0f ||
                _measurementEventCount < 2 ||
                _lastDamageTime <=
                    _firstDamageTime
            )
            {
                return 0f;
            }

            float observedDuration =
                _lastDamageTime -
                _firstDamageTime;

            // first->last omits one event interval.
            // Extend by the observed mean interval so short
            // sessions are not artificially inflated.
            return
                observedDuration *
                _measurementEventCount /
                (_measurementEventCount - 1);
        }
    }

    internal float PeakDamage =>
        _peakDamage;

    internal bool DpsReady =>
        MeasurementStarted &&
        _measurementEventCount >= 2 &&
        MeasurementTime > 0f;

    internal float CurrentDps =>
        DpsReady
            ? _totalDamage / MeasurementTime
            : 0f;

    public override void Load()
    {
        Instance = this;

        BindConfig();

        new Harmony(Guid).PatchAll();

        AddComponent<DpsOverlay>();

        SubscribeToDamageEvents();

        Log.LogInfo(
            $"{Name} {Version} loaded"
        );
    }

    public override bool Unload()
    {
        UnsubscribeFromTemporaryEvents();
        CleanupDummy("plugin unload");

        if (Instance == this)
            Instance = null;

        return true;
    }

    private void BindConfig()
    {
        _showOverlayConfig = Config.Bind(
            "Display",
            "ShowOverlay",
            true,
            "Show DPS text above the training dummy."
        );

        _textScaleConfig = Config.Bind(
            "Display",
            "TextScale",
            1f,
            "Scale of the floating DPS text."
        );


        _idleResetConfig = Config.Bind(
            "Measurement",
            "IdleResetSeconds",
            DefaultIdleResetSeconds,
            "Seconds without damage before finalizing the result."
        );

        _dummyOffsetXConfig = Config.Bind(
            "Placement",
            "DummyOffsetX",
            DefaultDummyOffsetX,
            "Horizontal dummy offset from the player."
        );

        _dummyOffsetYConfig = Config.Bind(
            "Placement",
            "DummyOffsetY",
            DefaultDummyOffsetY,
            "Vertical dummy offset from the player."
        );

        _debugLoggingConfig = Config.Bind(
            "Debug",
            "DebugLogging",
            false,
            "Enable verbose diagnostics."
        );
    }

    internal void DebugLog(
        string message)
    {
        if (DebugLogging)
            Log.LogInfo(message);
    }

    internal void OnRunStarted()
    {
        ResetDps();

        DebugLog(
            "[RUN] Run started"
        );
    }

    internal void OnPlayerSpawned(
        global::Player.Player player)
    {
        if (
            player == null ||
            player.gameObject == null
        )
        {
            return;
        }

        _currentPlayer = player;

        int playerId =
            player.gameObject.GetInstanceID();

        // Floor 1 has no bonfire, so its dummy starts with the run player.
        if (playerId != _runPlayerInstanceId)
        {
            _runPlayerInstanceId = playerId;
            _currentBonfireId = -1;

            CleanupDummy("new run");

            Vector3 position =
                GetSafeDummyPosition(
                    player.gameObject
                        .transform.position
                );

            DebugLog(
                "[LIFECYCLE] Floor 1 player spawned"
            );

            QueueSpawn(
                position,
                "Floor 1 starting room"
            );
        }
    }

    internal void OnBonfireReady(
        BonfireHandler bonfire)
    {
        if (
            bonfire == null ||
            bonfire.gameObject == null
        )
        {
            return;
        }

        int id =
            bonfire.gameObject.GetInstanceID();

        if (
            id == _currentBonfireId &&
            HasLiveDummy()
        )
        {
            return;
        }

        DebugLog(
            $"[BONFIRE] Active bonfire " +
            $"id={id} " +
            $"position={bonfire.transform.position}"
        );

        CleanupDummy(
            "new bonfire room"
        );

        _currentBonfireId = id;

        Vector3 position =
            GetSafeDummyPosition(
                bonfire.transform.position
            );

        QueueSpawn(
            position,
            "bonfire room"
        );
    }

    private Vector3 GetSafeDummyPosition(
        Vector3 fallbackAnchor)
    {
        Vector3 anchor =
            fallbackAnchor;

        try
        {
            if (
                _currentPlayer != null &&
                _currentPlayer.gameObject != null
            )
            {
                anchor =
                    _currentPlayer.gameObject
                        .transform.position;
            }
        }
        catch
        {
        }

        Vector3 desired =
            anchor +
            new Vector3(
                DummyOffsetX,
                DummyOffsetY,
                0f
            );

        try
        {
            DungeonGenerator generator =
                DungeonGenerator
                    ._Instance_k__BackingField;

            if (generator != null)
            {
                Rect room =
                    generator.CurrentRoomRect;

                if (
                    room.width >
                        RoomEdgePadding * 2f &&
                    room.height >
                        RoomEdgePadding * 2f
                )
                {
                    desired.x =
                        Mathf.Clamp(
                            desired.x,
                            room.xMin +
                                RoomEdgePadding,
                            room.xMax -
                                RoomEdgePadding
                        );

                    desired.y =
                        Mathf.Clamp(
                            desired.y,
                            room.yMin +
                                RoomEdgePadding,
                            room.yMax -
                                RoomEdgePadding
                        );
                }

                DebugLog(
                    $"[DUMMY] Placement " +
                    $"anchor={anchor} " +
                    $"desired={desired} " +
                    $"room={room}"
                );
            }
        }
        catch (
            System.Exception ex)
        {
            DebugLog(
                $"[DUMMY] Room clamp unavailable: " +
                $"{ex.Message}"
            );
        }

        return desired;
    }

    private void QueueSpawn(
        Vector3 position,
        string reason,
        bool postCombat = false,
        bool benchmarkReset = false)
    {
        _pendingSpawn = true;
        _pendingPostCombatSpawn = postCombat;
        _pendingBenchmarkReset = benchmarkReset;
        _pendingSpawnPosition =
            position;

        _pendingSpawnDeadline =
            Time.unscaledTime + 5f;

        _nextSpawnRetry = 0f;

        DebugLog(
            $"[DUMMY] Spawn scheduled: " +
            $"{reason} at {position}"
        );

        TryPendingSpawn();
    }

    internal void Tick()
    {
        TickMeasurementFinalization();

        if (!_pendingSpawn)
            return;

        if (HasLiveDummy())
        {
            _pendingSpawn = false;
            _pendingPostCombatSpawn = false;
            _pendingBenchmarkReset = false;
            return;
        }

        float now =
            Time.unscaledTime;

        if (
            now >
            _pendingSpawnDeadline
        )
        {
            _pendingSpawn = false;
            _pendingPostCombatSpawn = false;
            _pendingBenchmarkReset = false;

            Log.LogWarning(
                "[DUMMY] Spawn retry window expired"
            );

            return;
        }

        if (
            now <
            _nextSpawnRetry
        )
        {
            return;
        }

        _nextSpawnRetry =
            now + 0.25f;

        TryPendingSpawn();
    }

    private void TryPendingSpawn()
    {
        if (
            !_pendingSpawn ||
            HasLiveDummy()
        )
        {
            return;
        }

        GameObject? spawnedObject =
            null;

        try
        {
            CheatManager cheatManager =
                CheatManager._instance;

            if (
                cheatManager == null ||
                cheatManager.targetDummy == null
            )
            {
                return;
            }

            GameObject prefab =
                cheatManager.targetDummy;

            bool cheatedBefore =
                UI.Console.CheatConsole
                    .HasCheated;

            _dummyEnemy = null;
            _dummyEnemyManager = null;

            IsSpawningTrainingDummy =
                true;

            try
            {
                // Use the game's target-dummy prefab directly; never invoke the cheat command.
                spawnedObject =
                    UnityEngine.Object
                        .Instantiate(prefab);
            }
            finally
            {
                IsSpawningTrainingDummy =
                    false;
            }

            bool cheatedAfter =
                UI.Console.CheatConsole
                    .HasCheated;

            DebugLog(
                $"[SAFETY] HasCheated " +
                $"before={cheatedBefore} " +
                $"after={cheatedAfter}"
            );

            if (
                cheatedAfter !=
                cheatedBefore
            )
            {
                Log.LogError(
                    "[SAFETY] Spawning the training dummy " +
                    "changed CheatConsole.HasCheated. " +
                    "The dummy has been removed."
                );

                if (_dummyEnemy != null)
                {
                    CleanupDummy(
                        "cheat-state changed"
                    );
                }
                else if (
                    spawnedObject != null
                )
                {
                    UnityEngine.Object.Destroy(
                        spawnedObject
                    );
                }

                _pendingSpawn = false;

                return;
            }

            if (!HasLiveDummy())
            {
                if (
                    spawnedObject != null
                )
                {
                    UnityEngine.Object.Destroy(
                        spawnedObject
                    );
                }

                Log.LogWarning(
                    "[DUMMY] Prefab instantiated " +
                    "without registering an Enemy"
                );

                return;
            }

            _dummyEnemy!
                .TurnOnCannotDie();

            _dummyEnemy
                .gameObject
                .transform.position =
                _pendingSpawnPosition;

            _pendingSpawn = false;

            ClearActiveMeasurement();

            DebugLog(
                $"[DUMMY] Ready at " +
                $"{_dummyEnemy.gameObject.transform.position}"
            );

            if (_pendingPostCombatSpawn)
            {
                DebugLog(
                    "[DUMMY] Post-combat dummy spawned"
                );
            }

            if (_pendingBenchmarkReset)
            {
                DebugLog(
                    "[BENCHMARK] Fresh dummy spawned"
                );
            }

            _pendingPostCombatSpawn = false;
            _pendingBenchmarkReset = false;
        }
        catch (
            System.Exception ex)
        {
            IsSpawningTrainingDummy =
                false;

            if (
                spawnedObject != null &&
                !HasLiveDummy()
            )
            {
                try
                {
                    UnityEngine.Object.Destroy(
                        spawnedObject
                    );
                }
                catch
                {
                }
            }

            Log.LogWarning(
                $"[DUMMY] Spawn attempt failed: " +
                $"{ex}"
            );
        }
    }




    internal void CaptureTrainingDummy(
        Enemy enemy,
        EnemyManager manager)
    {
        if (
            enemy == null ||
            manager == null
        )
        {
            return;
        }

        _dummyEnemy = enemy;
        _dummyEnemyManager =
            manager;

        DebugLog(
            $"[DUMMY] Captured exact instance " +
            $"{enemy.gameObject?.name}"
        );
    }

    internal void OnLeavingRoom()
    {
        DebugLog(
            "[ROOM] Leaving room"
        );

        _pendingSpawn = false;
        _pendingPostCombatSpawn = false;
        _pendingBenchmarkReset = false;

        if (MeasurementStarted)
        {
            FinalizeDpsResult();
        }

        CleanupDummy(
            "room transition"
        );

        _currentBonfireId = -1;
    }

    private bool HasLiveDummy()
    {
        try
        {
            return
                _dummyEnemy != null &&
                _dummyEnemy.gameObject != null &&
                _dummyEnemy.gameObject
                    .activeInHierarchy;
        }
        catch
        {
            return false;
        }
    }

    private void CleanupDummy(
        string reason)
    {
        _pendingSpawn = false;

        if (_dummyEnemy == null)
        {
            ClearActiveMeasurement();
            return;
        }

        try
        {
            DebugLog(
                $"[DUMMY] Cleaning up: {reason}"
            );

            if (
                _dummyEnemyManager != null &&
                _dummyEnemyManager
                    .aliveEnemies != null
            )
            {
                _dummyEnemyManager
                    .aliveEnemies
                    .Remove(_dummyEnemy);
            }

            if (
                _dummyEnemy.gameObject != null
            )
            {
                UnityEngine.Object.Destroy(
                    _dummyEnemy.gameObject
                );
            }
        }
        catch (
            System.Exception ex)
        {
            Log.LogWarning(
                $"[DUMMY] Cleanup warning: " +
                $"{ex.Message}"
            );
        }

        _dummyEnemy = null;
        _dummyEnemyManager = null;

        ClearActiveMeasurement();

        DebugLog(
            $"[DUMMY] Dummy cleaned: {reason}"
        );
    }

    private void UnsubscribeFromTemporaryEvents()
    {
        if (
            _temporaryEvents == null
        )
        {
            return;
        }

        try
        {
            if (_damageHandler != null)
            {
                _temporaryEvents
                    .remove_AfterDamageTaken(
                        _damageHandler
                    );
            }

            if (_afterCompletedRoomHandler != null)
            {
                _temporaryEvents
                    .remove_AfterCompletedRoom(
                        _afterCompletedRoomHandler
                    );
            }
        }
        catch
        {
        }

        _temporaryEvents = null;
        _damageHandler = null;
        _afterCompletedRoomHandler = null;
    }

    internal void SubscribeToDamageEvents()
    {
        TemporaryEvents current =
            EventMediator.TemporaryEvents;

        if (current == null)
            return;

        if (
            _temporaryEvents != null &&
            _temporaryEvents.Pointer ==
                current.Pointer
        )
        {
            return;
        }

        if (_temporaryEvents != null)
            UnsubscribeFromTemporaryEvents();

        _temporaryEvents = current;

        System.Action<
            float,
            Il2CppSystem.Collections.Generic.HashSet<string>,
            Actor,
            Actor,
            GameObject
        > callback =
            OnAfterDamageTaken;

        _damageHandler = callback;

        current.add_AfterDamageTaken(
            _damageHandler
        );

        System.Action afterCompletedRoomCallback =
            OnAfterCompletedRoom;

        _afterCompletedRoomHandler =
            afterCompletedRoomCallback;

        current.add_AfterCompletedRoom(
            _afterCompletedRoomHandler
        );
    }

    private void OnAfterCompletedRoom()
    {
        DebugLog(
            "[ROOM] AfterCompletedRoom received"
        );

        if (Instance != this)
        {
            DebugLog(
                "[DUMMY] Spawn skipped: plugin is not active"
            );
            return;
        }

        if (HasLiveDummy())
        {
            DebugLog(
                "[DUMMY] Spawn skipped: dummy already exists"
            );
            return;
        }

        if (_pendingSpawn)
        {
            DebugLog(
                "[DUMMY] Spawn skipped: spawn already pending"
            );
            return;
        }

        if (
            _currentPlayer == null ||
            _currentPlayer.gameObject == null
        )
        {
            DebugLog(
                "[DUMMY] Spawn skipped: player unavailable"
            );
            return;
        }

        EnemyManager manager =
            EnemyManager._instance;

        if (manager == null)
        {
            DebugLog(
                "[DUMMY] Spawn skipped: EnemyManager unavailable"
            );
            return;
        }

        if (!manager.roomIsCompleted)
        {
            DebugLog(
                "[DUMMY] Spawn skipped: room is not completed"
            );
            return;
        }

        if (manager.roomIsBeingCompleted)
        {
            DebugLog(
                "[DUMMY] Spawn skipped: room completion is still in progress"
            );
            return;
        }

        DungeonGenerator generator =
            DungeonGenerator
                ._Instance_k__BackingField;

        if (
            generator == null ||
            generator.CurrentRoomRect.width <= 0f ||
            generator.CurrentRoomRect.height <= 0f
        )
        {
            DebugLog(
                "[DUMMY] Spawn skipped: current room unavailable"
            );
            return;
        }

        Vector3 position =
            GetSafeDummyPosition(
                _currentPlayer.gameObject
                    .transform.position
            );

        QueueSpawn(
            position,
            "post-combat room",
            true
        );
    }

    private void OnAfterDamageTaken(
        float damageTaken,
        Il2CppSystem.Collections.Generic.HashSet<string> tags,
        Actor damageReceiver,
        Actor damageDealer,
        GameObject damageDealingObject)
    {
        if (_pendingBenchmarkReset)
        {
            return;
        }

        if (
            !HasLiveDummy() ||
            damageReceiver == null ||
            damageReceiver.gameObject == null
        )
        {
            return;
        }

        // Count only final damage received by this exact training dummy.
        if (
            damageReceiver.gameObject
                .GetInstanceID() !=
            _dummyEnemy!
                .gameObject
                .GetInstanceID()
        )
        {
            return;
        }

        float now =
            Time.time;

        TickMeasurementFinalization();

        if (
            _hasCompletedResult &&
            _firstDamageTime < 0f
        )
        {
            BeginFreshMeasurement();
        }

        if (
            _firstDamageTime < 0f
        )
        {
            _firstDamageTime =
                now;
        }

        _lastDamageTime = now;
        _totalDamage += damageTaken;
        _measurementEventCount++;

        if (
            damageTaken >
            _peakDamage
        )
        {
            _peakDamage =
                damageTaken;
        }
    }

    internal void TickMeasurementFinalization()
    {
        if (
            _firstDamageTime < 0f
        )
        {
            return;
        }

        float now =
            Time.time;

        if (
            _lastDamageTime >= 0f &&
            now - _lastDamageTime >=
                IdleResetSeconds
        )
        {
            FinalizeDpsResult();
            ResetDummyAfterBenchmark();
            return;
        }

    }

    private void ResetDummyAfterBenchmark()
    {
        if (
            !_hasCompletedResult ||
            _pendingBenchmarkReset ||
            _pendingSpawn ||
            !HasLiveDummy()
        )
        {
            return;
        }

        Vector3 position;

        try
        {
            position =
                _dummyEnemy!
                    .gameObject
                    .transform.position;
        }
        catch
        {
            return;
        }

        _pendingBenchmarkReset = true;

        DebugLog(
            "[BENCHMARK] Dummy reset requested"
        );

        CleanupDummy(
            "benchmark reset"
        );

        DebugLog(
            "[BENCHMARK] Old dummy destroyed"
        );

        QueueSpawn(
            position,
            "benchmark reset",
            benchmarkReset: true
        );
    }

    private void FinalizeDpsResult()
    {
        if (_firstDamageTime < 0f)
            return;

        float duration =
            Mathf.Max(
                MeasurementTime,
                0.25f
            );

        float dps =
            _totalDamage /
            duration;

        if (_hasCompletedResult)
        {
            _previousDps =
                _completedDps;

            _hasPreviousResult =
                true;
        }

        _completedDps =
            dps;

        _hasCompletedResult =
            true;

        DebugLog(
            $"[DPS] Finalized: " +
            $"dps={_completedDps:F2} " +
            $"damage={_totalDamage:F2} " +
            $"duration={duration:F3}"
        );

        _totalDamage = 0f;
        _peakDamage = 0f;
        _firstDamageTime = -1f;
        _lastDamageTime = -1f;
        _measurementEventCount = 0;


    }

    private void BeginFreshMeasurement()
    {
        if (_hasCompletedResult)
        {
            _previousDps =
                _completedDps;

            _hasPreviousResult =
                true;
        }

        _hasCompletedResult =
            false;

        _totalDamage = 0f;
        _peakDamage = 0f;
        _firstDamageTime = -1f;
        _lastDamageTime = -1f;
        _measurementEventCount = 0;


    }

    private void ClearActiveMeasurement()
    {
        _totalDamage = 0f;
        _peakDamage = 0f;
        _firstDamageTime = -1f;
        _lastDamageTime = -1f;
        _measurementEventCount = 0;
    }

    private void ResetDps()
    {
        ClearActiveMeasurement();

        _hasCompletedResult = false;
        _completedDps = 0f;
        _hasPreviousResult = false;
        _previousDps = 0f;

    }

}


[HarmonyPatch(
    typeof(EnemyManager),
    nameof(EnemyManager.OnRegisterEnemy)
)]
internal static class TrainingDummyRegistrationPatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        EnemyManager __instance,
        Enemy enemy)
    {

        if (
            !Plugin
                .IsSpawningTrainingDummy
        )
        {

            return true;
        }

        if (enemy == null)
        {
            Plugin.Instance
                ?.Log.LogError(
                    "[DUMMY-REGISTER] Enemy was null"
                );

            return true;
        }

        try
        {
            Plugin.Instance
                ?.CaptureTrainingDummy(
                    enemy,
                    __instance
                );

            // Do not register the training dummy as an alive room enemy.
            // Inventory/combat systems use EnemyManager alive-enemy state,
            // so the dummy must remain outside that bookkeeping.
        }
        catch (
            System.Exception ex)
        {
            Plugin.Instance
                ?.Log.LogError(
                    $"[DUMMY-REGISTER] {ex}"
                );
        }

        // Never run vanilla registration for our dummy.
        return false;
    }
}



[HarmonyPatch(
    typeof(DungeonGenerator),
    nameof(DungeonGenerator.GoToNextRoom)
)]
internal static class DungeonRoomTransitionPatch
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        Plugin.Instance
            ?.OnLeavingRoom();
    }
}


[HarmonyPatch(
    typeof(BonfireHandler),
    "Start"
)]
internal static class BonfireLifecyclePatch
{
    [HarmonyPostfix]
    private static void Postfix(
        BonfireHandler __instance)
    {
        // Later floors create the dummy when their real bonfire activates.
        Plugin.Instance
            ?.OnBonfireReady(
                __instance
            );
    }
}


[HarmonyPatch(
    typeof(EventMediator),
    nameof(
        EventMediator
            .ReCreateTemporaryEvents
    )
)]
internal static class ReCreateTemporaryEventsPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        Plugin.Instance
            ?.SubscribeToDamageEvents();
    }
}


[HarmonyPatch(
    typeof(PersistentEvents),
    nameof(
        PersistentEvents
            .InvokeRunStarted
    )
)]
internal static class RunStartedPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        Plugin.Instance
            ?.OnRunStarted();
    }
}


[HarmonyPatch(
    typeof(TemporaryEvents),
    nameof(
        TemporaryEvents
            .InvokeSpawnedPlayer
    )
)]
internal static class SpawnedPlayerPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        global::Player.Player player)
    {
        Plugin.Instance
            ?.OnPlayerSpawned(
                player
            );
    }
}
