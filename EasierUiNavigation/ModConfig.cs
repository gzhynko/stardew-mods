using StardewModdingAPI.Utilities;

namespace EasierUiNavigation
{
    /// <summary> The mod config class. More info here: https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config </summary>
    public class ModConfig
    {
        /// <summary>
        /// Continue to the next replica in a dialogue or skip the 'letter typing'.
        /// Also works for location messages.
        /// </summary>
        public KeybindList DialogueContinue { get; set; } = KeybindList.Parse("Enter, Space");
        
        /// <summary>
        /// Switch to the next page in menus like the journal.
        /// </summary>
        public KeybindList NextPage { get; set; } = KeybindList.Parse("Right");
        
        /// <summary>
        /// Switch to the previous page in menus like the journal.
        /// </summary>
        public KeybindList PreviousPage { get; set; } = KeybindList.Parse("Left");
        
        /// <summary>
        /// Change the highlighted choice (in a question) to the next one.
        /// </summary>
        public KeybindList NextChoice { get; set; } = KeybindList.Parse("Down");

        /// <summary>
        /// Change the highlighted choice (in a question) to the previous one.
        /// </summary>
        public KeybindList PreviousChoice { get; set; } = KeybindList.Parse("Up");

        /// <summary>
        /// Confirm the selected choice (in a question).
        /// </summary>
        public KeybindList ConfirmChoice { get; set; } = KeybindList.Parse("Enter");
        
        /// <summary>
        /// Exit the current menu/submenu.
        /// </summary>
        public KeybindList Exit { get; set; } = KeybindList.Parse("Escape");

        /// <summary>
        /// Sort items in the inventory or a chest.
        /// </summary>
        public KeybindList SortItems { get; set; } = KeybindList.Parse("J");
        
        /// <summary>
        /// Stack items to chest.
        /// </summary>
        public KeybindList StackToChest { get; set; } = KeybindList.Parse("K");

        /// <summary>
        /// The additional key (or a combination of) that needs to be pressed with either the NextPage or the PreviousPage keys
        /// simultaneously to switch the GameMenu tabs.
        /// </summary>
        public KeybindList AdditionalCombinationForMenuTabs { get; set; } = KeybindList.Parse("LeftShift");
    }
}