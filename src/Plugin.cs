using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace TinyRogues.TrainingDummy;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BasePlugin
{
    public const string Guid = "tanner.tinyrogues.trainingdummy";
    public const string Name = "Tiny Rogues Training Dummy";
    public const string Version = "0.0.1";

    public override void Load()
    {
        Log.LogInfo($"{Name} {Version} loaded successfully");
    }
}
