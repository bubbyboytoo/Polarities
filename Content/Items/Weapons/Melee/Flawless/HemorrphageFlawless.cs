using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using Polarities.Global;

namespace Polarities.Content.Items.Weapons.Melee.Flawless
{
	public class HemorrphageFlawless : ModItem
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Gosperian");
			//Tooltip.SetDefault("Releases waves of spikes");

			// These are all related to gamepad controls and don't seem to affect anything else
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
			PolaritiesItem.IsFlawless.Add(Type);
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 38;
			Item.height = 32;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.shootSpeed = 16f;
			Item.knockBack = 3.5f;
			Item.damage = 170;
			Item.rare = RarityType<Hemorrphage_RW_FlawlessRarity>();

			Item.DamageType = DamageClass.Melee;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.UseSound = SoundID.Item1;
			Item.value = 50000;
			Item.shoot = ProjectileType<HemorrphageFlawlessProjectile>();
		}
	}

    public class HemorrphageFlawlessProjectile : ModProjectile
	{
        private float scale = 1;
		private int hits = 0;

		public override void SetStaticDefaults() {
            //DisplayName.SetDefault("Gosperian");
            
			// The following sets are only applicable to yoyo that use aiStyle 99.
			// YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
			// Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = -1f;
			// YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
			// Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 400f;
			// YoyosTopSpeed is top speed of the yoyo Projectile. 
			// Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 17.5f;
		}

		public override void SetDefaults() {
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			// aiStyle 99 is used for all yoyos, and is Extremely suggested, as yoyo are extremely difficult without them
			Projectile.aiStyle = 99;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1f;

			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.usesIDStaticNPCImmunity = true;
		}
		// notes for aiStyle 99: 
		// localAI[0] is used for timing up to YoyosLifeTimeMultiplier
		// localAI[1] can be used freely by specific types
		// ai[0] and ai[1] usually point towards the x and y world coordinate hover point
		// ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
		// ai[0] being negative makes the yoyo move back towards the player
		// Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

		/*public override void PostAI() {
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
				dust.noGravity = true;
				dust.scale = 0.75f;
			}
		}*/

		List<Vector2> positions = new List<Vector2>();
        public override void AI() {
			Projectile.scale = scale;


			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f + (100 * Projectile.scale);

			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 15f + (2.5f * Projectile.scale);

			Player player = Main.player[Projectile.owner];
			Vector2 controlPoint = player.HandPosition.GetValueOrDefault(player.Center).DirectionTo(Main.MouseWorld) * Projectile.Center.Distance(player.HandPosition.GetValueOrDefault(player.Center)) * 0.75f;
			controlPoint += player.HandPosition.GetValueOrDefault(player.Center);

			positions = new List<Vector2>();
			for (int i = 0; i < 100; i++)
            {
				Vector2 bez1 = Vector2.Lerp(player.HandPosition.GetValueOrDefault(player.Center), controlPoint, i / 100f);
				Vector2 bez2 = Vector2.Lerp(controlPoint, Projectile.Center, i / 100f);
				Vector2 position = Vector2.Lerp(bez1, bez2, i / 100f) + Main.rand.NextVector2Circular(0.5f, 0.5f);

				Vector2 la1 = Vector2.Lerp(player.HandPosition.GetValueOrDefault(player.Center), controlPoint, i / 100f + 0.01f);
				Vector2 la2 = Vector2.Lerp(controlPoint, Projectile.Center, i / 100f + 0.01f);
				Vector2 lookAhead = Vector2.Lerp(la1, la2, i / 100f + 0.01f);

				positions.Add(position);

				if (i % 5 == 0)
                {
					Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), position, Vector2.Zero, ProjectileType<HemorrphageFlawlessVertebra>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.whoAmI);
					p.rotation = position.DirectionTo(lookAhead).ToRotation();
				}
            }
        }

		private void DrawLine(List<Vector2> list, Color mainColor)
		{
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			for (int i = 1; i < list.Count - 1; i++)
			{
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), mainColor);
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}


		public override bool PreDrawExtras()
		{
			DrawLine(positions, Color.Red);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			hits++;
			scale = MathHelper.Lerp(scale, 2f, 0.1f);
        }
	}

	public class HemorrphageFlawlessVertebra : ModProjectile
    {
		public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = true;


			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.usesIDStaticNPCImmunity = true;

		}

		public override void AI()
        {
			if (Main.rand.NextFloat(0, 1) < Main.projectile[(int)Projectile.ai[0]].scale / 300)
            {
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Main.projectile[(int)Projectile.ai[0]].Center, Main.rand.NextVector2CircularEdge(4, 4), ProjectileType<HemorrphageFlawlessOffshoot>(), Projectile.damage, Projectile.knockBack, Projectile.owner).ai[2] = -1;
            }
        }
	}

	public class HemorrphageFlawlessOffshoot : ModProjectile
    {
		public override string Texture => "Polarities/Content/Projectiles/CallShootProjectile";

		Vector2 targetPosition;
		bool hideEnd = false;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 999;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		bool canSpawnBranch = true;
		List<Vector2> positions = new List<Vector2>();
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 2;
			Projectile.scale = 0.2f;
			if (Projectile.ai[1] > 4) canSpawnBranch = false;

			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.usesIDStaticNPCImmunity = true;
		}

		public override void AI()
        {
			Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-3, 3)));
			Projectile.velocity *= 0.99f;

			int targetID = -1;
			Projectile.Minion_FindTargetInRange(1200, ref targetID, skipIfCannotHitWithOwnBody: false);
			if (targetID != -1)
			{
				NPC target = Main.npc[targetID];
				Vector2 targetVelocity = 8 * Projectile.DirectionTo(target.Center + Main.rand.NextVector2Circular(target.width, target.height)).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-10, 10)));
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.25f);
			}

				if (Projectile.timeLeft < 60) Projectile.velocity = Vector2.Zero;
			else positions.Add(Projectile.position);

			if (Projectile.ai[0]++ == 90)
			{
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Main.rand.NextVector2CircularEdge(4, 4), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: Projectile.ai[1] + 1);
			}
		}

		private void DrawLine(List<Vector2> list, Color mainColor)
		{
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 2);

			Vector2 pos = list[0];
			for (int i = 1; i < list.Count - 1; i++)
			{
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				float alpha = Projectile.timeLeft / 60f;
				if (alpha > 1) alpha = 1;
				Color color = Lighting.GetColor(element.ToTileCoordinates(), mainColor) * alpha;
				Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawLine(positions, Color.Red);
			return false;
		}
	}
}
