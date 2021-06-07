using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace EasierUiNavigation
{
    /// <summary> The mod entry class loaded by SMAPI. </summary>
    public class ModEntry : Mod
    {
        #region Variables
        
        public static IMonitor ModMonitor;
        public static IModHelper ModHelper;
        public static ModConfig Config;
        
        #endregion
        #region Public methods
        
        /// <summary> The mod entry point, called after the mod is first loaded. </summary>
        /// <param name="helper"> Provides simplified APIs for writing mods. </param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            ModHelper = Helper;
            
            Config = Helper.ReadConfig<ModConfig>();
            
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
        }
        
        #endregion
        #region Private methods

        /// <summary>
        /// Raised when a key is pressed and released. Used for all of the keybindings.
        /// </summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            var activeMenu = Game1.activeClickableMenu;

            if (Config.NextPage.JustPressed())
            {
                ActionHandlers.NextPage(activeMenu);
            }
            
            if (Config.PreviousPage.JustPressed())
            {
                ActionHandlers.PreviousPage(activeMenu);
            }

            if (Config.DialogueContinue.JustPressed())
            {
                if (activeMenu is DialogueBox dialogueBox)
                {
                    ActionHandlers.DialogueContinue(dialogueBox);
                }
            }
            
            if (Config.NextChoice.JustPressed())
            {
                if (activeMenu is DialogueBox dialogueBox)
                {
                    ActionHandlers.NextChoice(dialogueBox);
                }
            }
            
            if (Config.PreviousChoice.JustPressed())
            {
                if (activeMenu is DialogueBox dialogueBox)
                {
                    ActionHandlers.PreviousChoice(dialogueBox);
                }
            }
            
            if (Config.ConfirmChoice.JustPressed())
            {
                if (activeMenu is DialogueBox dialogueBox)
                {
                    ActionHandlers.ConfirmChoice(dialogueBox);
                }
            }

            if (Config.Exit.JustPressed())
            {
                ActionHandlers.Exit(activeMenu);
            }

            if (Config.SortItems.JustPressed())
            {
                ActionHandlers.SortItems(activeMenu);
            }
            
            if (Config.StackToChest.JustPressed())
            {
                ActionHandlers.StackToChest(activeMenu);
            }
            
            if (KeybindList
                .Parse($"{Config.AdditionalCombinationForMenuTabs} + {Config.NextPage}")
                .JustPressed())
            {
                ActionHandlers.NextMenuTab(activeMenu);
            }
            
            if (KeybindList
                .Parse($"{Config.AdditionalCombinationForMenuTabs} + {Config.PreviousPage}")
                .JustPressed())
            {
                ActionHandlers.PreviousMenuTab(activeMenu);
            }
        }
        
        /// <summary>
        /// Raised when the mouse wheel is scrolled.
        /// </summary>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            var direction = e.Delta < 0; // true - down, false - up
            var activeMenu = Game1.activeClickableMenu;

            if (direction)
            {
                switch (activeMenu)
                {
                    case DialogueBox dialogueBox:
                        ActionHandlers.NextChoice(dialogueBox);
                        break;
                    
                    default:
                        ActionHandlers.NextPage(activeMenu);
                        break;
                }
            }
            else
            {
                switch (activeMenu)
                {
                    case DialogueBox dialogueBox:
                        ActionHandlers.PreviousChoice(dialogueBox);
                        break;
                    
                    default:
                        ActionHandlers.PreviousPage(activeMenu);
                        break;
                }
            }
        }

        #endregion
    }
}
