using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewValley;
using StardewValley.Menus;

namespace StardewElectricity.Menus.Pages;

public class ElectricityPage_old : IClickableMenu
{
    private SpriteFont _textFont;
    
    private Point _kwhConsumedTextPos;
    private Point _payBillsAreaPos;
    
    private string _noBillsTextString;
    private Point _noBillsTextPos;
    
    private OptionsButton _payBillsButton;
    private Point _payBillsButtonPos;

    private string _billsHintTextString = "";
    private Point _billsHintTextPos;
    
    public ElectricityPage_old(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        _textFont = Game1.dialogueFont;
        
        var basePos = new Point(xPositionOnScreen + 16 + 32, yPositionOnScreen + 80 + 4 + 32);
        _payBillsAreaPos = new Point(xPositionOnScreen + width / 2, yPositionOnScreen + height - 128);

        _kwhConsumedTextPos = new Point(basePos.X, basePos.Y);

        void PayBillsAction()
        {
            if (ModEntry.ElectricityManager.PayAllBills())
            {
                _payBillsButton!.label = "Pay bills";
            }
            else
            {
                _payBillsButton!.label = "Pay bills (" + ModEntry.ElectricityManager.GetTotalOwed() + "g)";
                Game1.showRedMessage("Not Enough Funds Left");
            }
        }
        
        _payBillsButton = new OptionsButton("Pay bills (" + ModEntry.ElectricityManager.GetTotalOwed() + "g)", PayBillsAction);
        _payBillsButtonPos = new Point(_payBillsAreaPos.X - _payBillsButton.bounds.Width / 2 - _payBillsButton.bounds.X, _payBillsAreaPos.Y - _payBillsButton.bounds.Height - _payBillsButton.bounds.Y);

        _noBillsTextString = "No bills posted";
        var noBillsTextSize = _textFont.MeasureString(_noBillsTextString);
        _noBillsTextPos = new Point(_payBillsAreaPos.X - (int)noBillsTextSize.X / 2,
            _payBillsAreaPos.Y - (int)noBillsTextSize.Y / 2);
    }
    
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        _payBillsButton.receiveLeftClick(x - _payBillsButtonPos.X, y - _payBillsButtonPos.Y);
    }

    public override void draw(SpriteBatch b)
    {
        var kwhConsumedText =
            $"KWh consumed {Utility.Utility.GetBillingCycleContextString(ModEntry.ElectricityManager.GetBillingCycle())}: " +
            ModEntry.ElectricityManager.GetKwhConsumedThisCycle();
        StardewValley.Utility.drawTextWithShadow(b, kwhConsumedText, _textFont,  _kwhConsumedTextPos.ToVector2(), Game1.textColor);

        if (ModEntry.ElectricityManager.AreBillsPending())
        {
            _payBillsButton.draw(b, _payBillsButtonPos.X, _payBillsButtonPos.Y);
            _billsHintTextString = "";
        }
        else
        {
            StardewValley.Utility.drawTextWithShadow(b, _noBillsTextString, _textFont, _noBillsTextPos.ToVector2(),
                Game1.textColor);

            var daysUntilBill = ModEntry.ElectricityManager.GetDaysUntilNextBill();
            if (daysUntilBill != -1)
            {
                var daysText = daysUntilBill > 1 ? "in " + daysUntilBill + " days" : "tomorrow";
                _billsHintTextString = "Next bill: " + daysText;
                var billsHintTextSize = Game1.smallFont.MeasureString(_billsHintTextString);
                _billsHintTextPos = new Point(_payBillsAreaPos.X - (int)billsHintTextSize.X / 2,
                    _payBillsAreaPos.Y + 40 - (int)billsHintTextSize.Y / 2);
            }
        }
        
        StardewValley.Utility.drawTextWithShadow(b, _billsHintTextString, Game1.smallFont, _billsHintTextPos.ToVector2(),
            Game1.textColor);
    }
}