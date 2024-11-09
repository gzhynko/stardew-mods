using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewElectricity.Types;
using StardewModdingAPI;

namespace StardewElectricity.Managers;

public class AssetManager
{
    public Texture2D PoleTexture;
    public Texture2D SidewaysPoleTexture;
    public Texture2D PoleShadowTexture;
    
    public Texture2D IconsTexture;

    public Dictionary<IconsTextureItem, Rectangle> IconsTextureBoundsMap = new()
    {
        { IconsTextureItem.MenuElectricityTabIcon, new Rectangle(0, 0, 16, 16) },
        { IconsTextureItem.BuildingPoweredIcon, new Rectangle(16, 0, 16, 16) },
        { IconsTextureItem.NoPowerIcon, new Rectangle(32, 0, 16, 16) },
        { IconsTextureItem.JojaEnergyLogoSimple, new Rectangle(0, 16, 48, 16) }
    };

    public void PrepareAssets(IModContentHelper modContent)
    {
        PoleTexture = modContent.Load<Texture2D>("assets/utilityPole.png");
        SidewaysPoleTexture = modContent.Load<Texture2D>("assets/utilityPoleSideways.png");
        PoleShadowTexture = modContent.Load<Texture2D>("assets/utilityPole_shadow.png");
        
        IconsTexture = modContent.Load<Texture2D>("assets/icons.png");
    }
}