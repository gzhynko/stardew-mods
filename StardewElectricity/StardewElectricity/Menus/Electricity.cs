using Microsoft.Xna.Framework;
using StardewElectricity.Menus.Pages;
using StardewValley.Menus;
using Constants = StardewElectricity.Utility.Constants;

namespace StardewElectricity.Menus;

public static class Electricity
{
    public static void GameMenuOpened(GameMenu menu)
    {
        menu.tabs.Add(new ClickableComponent(new Rectangle(menu.xPositionOnScreen + 704, menu.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), Constants.GameMenuElectricityTabName, "Electricity")
        {
            myID = 12350,
            downNeighborID = 10,
            leftNeighborID = 12349,
            tryDefaultIfNoDownNeighborExists = true,
            fullyImmutable = true
        });
        menu.pages.Add(new ElectricityPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width - 64 - 16, menu.height));
    }
}