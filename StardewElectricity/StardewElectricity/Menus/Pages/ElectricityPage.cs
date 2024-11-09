using Microsoft.Xna.Framework;
using StardewElectricity.Menus.UIComponents;
using StardewModdingAPI;
using StardewUI;
using StardewValley;

namespace StardewElectricity.Menus.Pages;

public class ElectricityPageData
{
    public int MenuPositionX { get; set; } = 0;
    public int MenuPositionY { get; set; } = 0;
    public int MenuWidth { get; set; }
    public int MenuHeight { get; set; }
}

public class ElectricityPage(ElectricityPageData viewData) 
    : GameMenuPage<ElectricityPageView>(viewData.MenuPositionX, viewData.MenuPositionY, viewData.MenuWidth, viewData.MenuHeight)
{
    protected override ElectricityPageView CreateView()
    {
        return new ElectricityPageView(viewData);
    }
}

public class ElectricityPageView(ElectricityPageData viewData) : WrapperView<Frame>
{
    protected override Frame CreateView()
    {
        var kwhConsumedText =
            $"KWh consumed {Utility.Utility.GetBillingCycleContextString(ModEntry.ElectricityManager.GetBillingCycle())}: " +
            ModEntry.ElectricityManager.GetKwhConsumedThisCycle();
        var content = new Lane
        {
            Layout = LayoutParameters.Fill(),
            Orientation = Orientation.Vertical,
            Children = [
                Label.Simple(kwhConsumedText, Game1.dialogueFont),
                new Image
                {
                    Layout = LayoutParameters.FixedSize(240, 68),
                    Fit = ImageFit.Stretch,
                    Sprite = new Sprite(Game1.mouseCursors, new Rectangle(432, 439, 9, 9), new Edges(2), new SliceSettings(Scale: 4)),
                }
            ]
        };
        return new Frame
        {
            Padding = new(32 + 16, 32 + 80 + 4),
            Content = content
        };
    }
}