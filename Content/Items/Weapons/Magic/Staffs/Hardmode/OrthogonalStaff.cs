using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Core;
using Polarities.Global;
using Polarities.Content.NPCs.Enemies.Fractal.PostSentinel;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.Items.Weapons.Magic.Staffs.Hardmode
{
	public class OrthogonalStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
			//DisplayName.SetDefault("Orthogonal Staff");
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 45;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 5;
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 5;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item8;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<OrthogonalStaffProjectile>();
			Item.shootSpeed = 30f;
		}

		public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
		{
			//if (player.GetModPlayer<PolaritiesPlayer>().fractalManaReduction)
			//{
			//	mult = 0;
			//}
		}
	}

	public class OrthogonalStaffProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Orthogonal Energy");
			Main.projFrames[Projectile.type] = 1;

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 24;
			Projectile.height = 24;
			DrawOffsetX = 0;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 0;

			Projectile.DamageType = DamageClass.Magic;

			Projectile.alpha = 0;
			Projectile.timeLeft = 360;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Scale: 0.75f)].noGravity = true;

			float speed = Projectile.velocity.Length();
			float param = 0.0000005f;
			Projectile.velocity = (new Vector2(Projectile.velocity.X + param*(float)Math.Pow(Projectile.velocity.X,5), Projectile.velocity.Y + param*(float)Math.Pow(Projectile.velocity.Y, 5))).SafeNormalize(Vector2.Zero) * speed;

			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Scale: 1f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Color mainColor = Color.White;

			for (int k = 1; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale;

				float rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation();

				Main.spriteBatch.Draw(texture, Projectile.Center + (-Projectile.position + Projectile.oldPos[k])/2f - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), scale, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}