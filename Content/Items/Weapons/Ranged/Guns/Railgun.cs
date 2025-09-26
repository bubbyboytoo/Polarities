using System;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polarities.Content.Projectiles;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.Items.Weapons.Ranged.Guns.Hardmode
{
	public class Railgun : ModItem
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Railgun");
			//Tooltip.SetDefault("Accelerates bullets to insane speeds via the power of magnets, causing them to produce shockwaves");
		}

		float trueDamage;
		public override void SetDefaults()
		{
			Item.damage = 400;
			trueDamage = 400;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 33;
			Item.height = 11;
			Item.useTime = 33;
			Item.useAnimation = 33;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 20;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item40;
			Item.autoReuse = true;
			Item.shoot = 10;
			Item.shootSpeed = 16f;
			Item.useAmmo = AmmoID.Bullet;
		}

		public override void AddRecipes()
        {
			CreateRecipe()
				.AddIngredient(ItemID.SniperRifle)
				.AddIngredient(ItemType<Placeable.Bars.PolarizedBar>(), 13)
				.AddIngredient(ItemType<Materials.Hardmode.SmiteSoul>(), 6)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

			trueDamage = MathHelper.Lerp(trueDamage, 1000, 0.005f);
			Item.damage = (int)trueDamage;
        }

        public override void HoldItem(Player player)
		{
			player.scope = true;
		}

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref System.Int32 type, ref System.Int32 damage, ref System.Single knockback)
        {
			position += new Vector2(Item.width, 5).RotatedBy(velocity.ToRotation());
        }

        public override System.Boolean Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, System.Int32 type, System.Int32 damage, System.Single knockback)
        {
			if (Item.damage > 800)
            {
				player.velocity -= velocity.SafeNormalize(Vector2.Zero) * (Item.damage - 600f) / 25f;
				SoundEngine.PlaySound(SoundID.Item96, player.position);
            }

			player.GetModPlayer<PolaritiesPlayer>().screenshakeMagnitude = (Item.damage / 100) - 2;
			player.GetModPlayer<PolaritiesPlayer>().screenshakeTimer = 5 + Item.damage / 50;

			int dustLength = 16;
			for (int v = 0; v < dustLength; v++)
            {
				Vector2 dPos = position + (velocity.SafeNormalize(Vector2.Zero) * v);
				Vector2 dVel = velocity.SafeNormalize(Vector2.Zero) * v;
				Dust d = Dust.NewDustDirect(dPos, 1, 1, DustID.Electric);
				d.scale = 2 - (v / ((dustLength / 4f) + 1f));
				d.noGravity = true;
				d.velocity = dVel;
				if (v < 3)
                {
					Vector2 hPos1 = position + (velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * v);
					Vector2 hVel1 = velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * v * 0.5f;
					Dust h1 = Dust.NewDustDirect(hPos1, 1, 1, DustID.Electric);
					h1.noGravity = true;
					h1.velocity = hVel1;

					Vector2 hPos2 = position + (velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * v);
					Vector2 hVel2 = velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * v;
					Dust h2 = Dust.NewDustDirect(hPos2, 1, 1, DustID.Electric);
					h2.noGravity = true;
					h2.velocity = hVel2;
				}
			}


			Projectile bullet = Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0)];
			bullet.timeLeft *= 8;
			bullet.extraUpdates = 8 * (bullet.extraUpdates + 1) - 1;
			bullet.GetGlobalProjectile<PolaritiesProjectile>().railgunShockwaves = 16;
			trueDamage = 400;
			return false;
		}

		public override Vector2? HoldoutOffset() {
            return new Vector2(-17, -3);
        }
	}

	public class RailgunShockwave : ModProjectile
	{
        public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Railgun Shockwave");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 72, 72, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * 3f * distanceSquared * Math.Exp(1 + 1 / (distanceSquared - 1)));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "RailgunShockwave.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 16;
			Projectile.tileCollide = false;

			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;

			Projectile.scale = 0f;
		}

		public override void AI()
		{
			Vector2 oldCenter = Projectile.Center;

			Projectile.alpha += 16;
			Projectile.scale += 1/16f;

			Projectile.width = (int)(64 * Projectile.scale);
			Projectile.height = (int)(64 * Projectile.scale);
			DrawOffsetX = (int)(0.5f * (Projectile.height - 72));
			DrawOriginOffsetY = (int)(0.5f * (Projectile.height - 72));
			DrawOriginOffsetX = 0;

			Projectile.Center = oldCenter;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}
	}
}