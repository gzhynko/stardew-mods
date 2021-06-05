using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;

namespace EasierUiNavigation
{
    public class Utility
    {
        public static void JournalPreviousPage(QuestLog journal)
        {
            var currentPage = ModEntry.ModHelper.Reflection.GetField<int>(journal, "currentPage").GetValue();

            if (currentPage > 0)
            {
                ModEntry.ModHelper.Reflection.GetMethod(journal, "nonQuestPageBackButton").Invoke();
            }
        }
        
        public static void JournalNextPage(QuestLog journal)
        {
            var currentPage = ModEntry.ModHelper.Reflection.GetField<int>(journal, "currentPage").GetValue();
            var pagesCount = ModEntry.ModHelper.Reflection.GetField<List<List<IQuest>>>(journal, "pages").GetValue().Count;

            if (currentPage + 1 < pagesCount)
            {
                ModEntry.ModHelper.Reflection.GetMethod(journal, "nonQuestPageForwardButton").Invoke();
            }
        }
        
        public static void CollectionsPreviousPage(CollectionsPage collectionsPage)
        {
            var currentPage = collectionsPage.currentPage;

            if (currentPage > 0)
            {
                collectionsPage.currentPage--;
                Game1.playSound("shwip");
            }
        }
        
        public static void CollectionsNextPage(CollectionsPage collectionsPage)
        {
            var currentPage = collectionsPage.currentPage;
            var pagesCount = collectionsPage.collections[collectionsPage.currentTab].Count;

            if (currentPage < pagesCount - 1)
            {
                collectionsPage.currentPage++;
                Game1.playSound("shwip");
            }
        }
        
        public static void LetterPreviousPage(LetterViewerMenu letterMenu)
        {
            var currentPage = letterMenu.page;

            if (currentPage > 0)
            {
                letterMenu.page--;
                Game1.playSound("shwip");
                letterMenu.OnPageChange();
            }
        }
        
        public static void LetterNextPage(LetterViewerMenu letterMenu)
        {
            var currentPage = letterMenu.page;
            var pagesCount = letterMenu.mailMessage.Count;

            if (currentPage < pagesCount - 1)
            {
                letterMenu.page++;
                Game1.playSound("shwip");
                letterMenu.OnPageChange();
            }
        }

        public static void AnswerDialogue(DialogueBox dialogueBox, Response response)
        {
            if (Game1.currentLocation.answerDialogue(response))
            {
                Game1.playSound("smallSelect");
                dialogueBox.selectedResponse = -1;
                dialogueBox.beginOutro();
            }
        }
    }
}