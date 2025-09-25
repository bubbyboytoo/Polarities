using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace Polarities.Content.Biomes.Fractal
{
	public class FractalWaterStyle : ModWaterStyle
	{
		
		public override int ChooseWaterfallStyle()
		{
			return ModContent.GetInstance<FractalWaterfallStyle>().Slot;
		}

		public override int GetSplashDust()
		{
			return ModContent.DustType<Assets.Dusts.SaltWaterSplash>();
		}

		public override int GetDropletGore()
		{
			return ModContent.GoreType<FractalDroplet>();
		}
		

		public override void LightColorMultiplier(ref float r, ref float g, ref float b)
		{
			r = 0f;
			g = 0.5f;
			b = 1f;
		}

		public override Color BiomeHairColor()
		{
			return Color.Cyan;
		}
	}

	public class FractalDroplet : ModGore
	{
		public override void SetStaticDefaults()
		{
			ChildSafety.SafeGore[Type] = true;
			GoreID.Sets.LiquidDroplet[Type] = true;

			// Rather than copy in all the droplet specific gore logic, this gore will pretend to be another gore to inherit that logic.
			UpdateType = GoreID.WaterDrip;
		}
	}

	public class FractalWaterfallStyle : ModWaterfallStyle
    {

    }
}