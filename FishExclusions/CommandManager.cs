using StardewModdingAPI;

namespace FishExclusions
{
    public static class CommandManager
    {
        public static void RegisterCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add("fex_toggle", "Toggle exclusions.", Toggle);
        }

        private static void Toggle(string command, string[] args)
        {
            ModEntry.ExclusionsEnabled = !ModEntry.ExclusionsEnabled;
            ModEntry.ModMonitor.Log($"Exclusions { (ModEntry.ExclusionsEnabled ? "enabled" : "disabled") }.", LogLevel.Info);
        }
    }
}
