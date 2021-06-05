using StardewValley;
using StardewValley.Menus;

namespace EasierUiNavigation
{
    public class ActionHandlers
    {
        public static void NextPage(IClickableMenu activeMenu)
        {
            switch (activeMenu)
            {
                case QuestLog journal:
                    Utility.JournalNextPage(journal);
                    break;
                    
                case GameMenu gameMenu:
                {
                    if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage collectionsPage)
                    {
                        Utility.CollectionsNextPage(collectionsPage);
                    }

                    break;
                }
                
                case LetterViewerMenu letterMenu:
                    Utility.LetterNextPage(letterMenu);
                    break;
            }
        }
        
        public static void PreviousPage(IClickableMenu activeMenu)
        {
            switch (activeMenu)
            {
                case QuestLog journal:
                    Utility.JournalPreviousPage(journal);
                    break;
                    
                case GameMenu gameMenu:
                {
                    if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage collectionsPage)
                    {
                        Utility.CollectionsPreviousPage(collectionsPage);
                    }

                    break;
                }
                
                case LetterViewerMenu letterMenu:
                    Utility.LetterPreviousPage(letterMenu);
                    break;
            }
        }

        public static void DialogueContinue(DialogueBox dialogueBox)
        {
            if (dialogueBox.isQuestion) return;
            
            dialogueBox.receiveLeftClick(0, 0);
        }
        
        public static void NextChoice(DialogueBox dialogueBox)
        {
            if (!dialogueBox.isQuestion || dialogueBox.responses == null) return;

            if (dialogueBox.selectedResponse == -1)
            {
                dialogueBox.selectedResponse = 0;
                return;
            }
            
            var responseCount = dialogueBox.responses.Count;

            if (dialogueBox.selectedResponse < responseCount - 1)
            {
                dialogueBox.selectedResponse++;
                Game1.playSound("Cowboy_gunshot");
            }
        }
        
        public static void PreviousChoice(DialogueBox dialogueBox)
        {
            if (!dialogueBox.isQuestion || dialogueBox.responses == null) return;

            if (dialogueBox.selectedResponse == -1)
            {
                dialogueBox.selectedResponse = 0;
                return;
            }
            
            if (dialogueBox.selectedResponse > 0)
            {
                dialogueBox.selectedResponse--;
                Game1.playSound("Cowboy_gunshot");
            }
        }

        public static void ConfirmChoice(DialogueBox dialogueBox)
        {
            if (!dialogueBox.isQuestion && dialogueBox.responses == null) return;
            if (dialogueBox.selectedResponse == -1) return;

            var selectedResponse = dialogueBox.responses[dialogueBox.selectedResponse];
            Utility.AnswerDialogue(dialogueBox, selectedResponse);
        }

        public static void Exit(IClickableMenu activeMenu)
        {
            switch (activeMenu)
            {
                case TitleMenu titleMenu:
                    titleMenu.backButtonPressed();
                    break;
            }
        }
    }
}