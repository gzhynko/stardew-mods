using StardewModdingAPI;

namespace FishExclusions
{
    public static class CommandManager
    {
        public static void RegisterCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add("fex_toggle", "Toggle exclusions.", Toggle);
            helper.ConsoleCommands.Add("fex_reload", "Reload the mod config.", ReloadConfig);
        }

        private static void Toggle(string command, string[] args)
        {
            ModEntry.ExclusionsEnabled = !ModEntry.ExclusionsEnabled;
            ModEntry.ModMonitor.Log($"Exclusions { (ModEntry.ExclusionsEnabled ? "enabled" : "disabled") }.", LogLevel.Info);
        }

        private static void ReloadConfig(string command, string[] args)
        {
            ModEntry.ReloadConfig();
            ModEntry.ModMonitor.Log("Config has been reloaded.", LogLevel.Info);
        }
    }
}
