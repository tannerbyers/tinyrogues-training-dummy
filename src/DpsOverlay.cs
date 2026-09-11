using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TinyRogues.TrainingDummy;

public sealed class DpsOverlay : MonoBehaviour
{
    private static readonly Vector3 LabelOffset =
        new Vector3(
            0f,
            1.55f,
            0f
        );

    private GameObject? _worldLabel;

    private readonly List<TMP_Text>
        _textComponents =
            new();

    private int _attachedDummyId = -1;

    private string _lastRenderedText =
        "";

    private float _nextCreateAttempt;

    public DpsOverlay(
        IntPtr pointer)
        : base(pointer)
    {
    }

    public void Update()
    {
        Plugin? plugin =
            Plugin.Instance;

        if (plugin == null)
            return;

        plugin.Tick();

        if (!plugin.ShowOverlay)
        {
            DestroyLabel();
            return;
        }

        GameObject? dummy =
            plugin.TrackedDummyObject;

        if (dummy == null)
        {
            DestroyLabel();
            return;
        }

        int dummyId;

        try
        {
            dummyId =
                dummy.GetInstanceID();
        }
        catch
        {
            DestroyLabel();
            return;
        }

        if (
            _worldLabel == null ||
            _attachedDummyId !=
                dummyId
        )
        {
            DestroyLabel();

            if (
                Time.unscaledTime >=
                _nextCreateAttempt
            )
            {
                _nextCreateAttempt =
                    Time.unscaledTime +
                    0.5f;

                CreateNativeLabel(
                    dummy,
                    dummyId
                );
            }
        }

        if (
            _worldLabel == null
        )
        {
            return;
        }

        try
        {
            _worldLabel
                .transform.position =
                dummy.transform.position +
                LabelOffset;

            string text =
                BuildDisplayText(
                    plugin
                );

            if (
                text !=
                _lastRenderedText
            )
            {
                UpdateText(text);

                _lastRenderedText =
                    text;
            }
        }
        catch
        {
            DestroyLabel();
        }
    }

    private void CreateNativeLabel(
        GameObject dummy,
        int dummyId)
    {
        try
        {
            WorldTextSpawner spawner =
                WorldTextSpawner.Instance;

            if (
                spawner == null ||
                spawner.worldTextPrefab ==
                    null
            )
            {
                Plugin.Instance
                    ?.Log.LogWarning(
                        "[UI] Native world-text prefab unavailable"
                    );

                return;
            }

            // Clone Tiny Rogues' world-text prefab to reuse its font/material.
            _worldLabel =
                UnityEngine.Object
                    .Instantiate(
                        spawner
                            .worldTextPrefab
                    );

            if (
                _worldLabel == null
            )
            {
                return;
            }

            _worldLabel.name =
                "Training Dummy DPS";

            _worldLabel
                .transform.position =
                dummy.transform.position +
                LabelOffset;

            _worldLabel
                .transform.localScale =
                _worldLabel
                    .transform.localScale *
                Plugin.Instance!
                    .TextScale;

            _textComponents.Clear();

            TMP_Text[] texts =
                _worldLabel
                    .GetComponentsInChildren<
                        TMP_Text
                    >(true);

            foreach (
                TMP_Text text in texts)
            {
                if (text == null)
                    continue;

                text.alignment =
                    TextAlignmentOptions
                        .Center;

                _textComponents
                    .Add(text);

                Plugin.Instance
                    ?.DebugLog(
                        $"[UI-FONT] " +
                        $"font={text.font?.name} " +
                        $"material=" +
                        $"{text.fontSharedMaterial?.name}"
                    );
            }

            if (
                _textComponents.Count ==
                0
            )
            {
                Plugin.Instance
                    ?.Log.LogWarning(
                        "[UI] Native prefab contained no TMP text"
                    );

                UnityEngine.Object.Destroy(
                    _worldLabel
                );

                _worldLabel = null;

                return;
            }

            _attachedDummyId =
                dummyId;

            _lastRenderedText =
                "";

            string initialText =
                BuildDisplayText(
                    Plugin.Instance!
                );

            UpdateText(
                initialText
            );

            _lastRenderedText =
                initialText;

            _worldLabel.SetActive(
                true
            );

            Plugin.Instance
                ?.DebugLog(
                    $"[UI] Native DPS text created " +
                    $"using {_textComponents.Count} " +
                    $"game text component(s)"
                );
        }
        catch (
            Exception ex)
        {
            Plugin.Instance
                ?.Log.LogWarning(
                    $"[UI] Native label creation failed: " +
                    $"{ex.Message}"
                );

            DestroyLabel();
        }
    }

    private static string BuildDisplayText(
        Plugin plugin)
    {
        if (
            !plugin.MeasurementStarted
        )
        {
            return
                "<size=70%>TRAINING DUMMY</size>\n" +
                "HIT TO TEST DPS";
        }

        if (!plugin.DpsReady)
        {
            return
                "<size=70%>TRAINING DUMMY</size>\n" +
                "DPS  --";
        }

        return
            "<size=70%>TRAINING DUMMY</size>\n" +
            $"DPS  {plugin.CurrentDps:F0}\n" +
            "<size=65%>" +
            $"{plugin.TotalDamage:F0} DMG · " +
            $"{plugin.MeasurementTime:F1}s · " +
            $"PEAK {plugin.PeakDamage:F0}" +
            "</size>";
    }

    private void UpdateText(
        string value)
    {
        foreach (
            TMP_Text text in
            _textComponents)
        {
            if (text != null)
                text.text = value;
        }
    }

    private void DestroyLabel()
    {
        try
        {
            if (
                _worldLabel != null
            )
            {
                UnityEngine.Object.Destroy(
                    _worldLabel
                );
            }
        }
        catch
        {
        }

        _worldLabel = null;

        _attachedDummyId =
            -1;

        _lastRenderedText =
            "";

        _textComponents.Clear();
    }

    public void OnDestroy()
    {
        DestroyLabel();
    }
}
