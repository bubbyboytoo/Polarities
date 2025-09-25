using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;

namespace Polarities.Content.Items.Weapons.Ranged.Throwables.Hardmode
{
	public class FractalGrenade : ModItem
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Fractal Grenade");
			//Tooltip.SetDefault("Splits into more grenades on exploding");
			Item.ResearchUnlockCount = 99;
		}

		public override void SetDefaults()
		{
			Item.damage = 80;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 14;
			Item.height = 18;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2;
			Item.value = 50;
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
			Item.shoot = ProjectileType<FractalGrenadeProjectile>();
			Item.shootSpeed = 5.5f;
			Item.maxStack = 9999;
		}

		public override void AddRecipes()
		{
			CreateRecipe(50)
				.AddIngredient(ItemType<Placeable.Bars.FractalBar>())
				.AddIngredient(ItemID.Grenade, 50)
				.Register();
		}
	}

    public class FractalGrenadeProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Content/Items/Weapons/Ranged/Throwables/Hardmode/FractalGrenade";

		public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Fractal Grenade");
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            DrawOriginOffsetY = -4;

            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
        }

        public override void AI()
		{
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
			{
				Projectile.tileCollide = false;
				Projectile.alpha = 255;

				Projectile.position.X += Projectile.width / 2;
				Projectile.position.Y += Projectile.height / 2;
				Projectile.width = 128;
				Projectile.height = 128;
				Projectile.position.X -= Projectile.width / 2;
				Projectile.position.Y -= Projectile.height / 2;
				Projectile.knockBack = 8f;
			}

			Projectile.ai[0]++;

			if (Projectile.ai[0] > 10f)
            {
				Projectile.ai[0] = 10f;

				if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
				{
					Projectile.velocity.X *= 0.97f;
					if ((double)Projectile.velocity.X > -0.01 && (double)Projectile.velocity.X < 0.01)
					{
						Projectile.velocity.X = 0f;
						Projectile.netUpdate = true;
					}
				}
				Projectile.velocity.Y += 0.2f;
			}
			Projectile.rotation += Projectile.velocity.X * 0.1f;
		}


        public override void OnKill(int timeLeft)
        {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			for (int num231 = 0; num231 < 20; num231++)
			{
				int num217 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
				Dust dust71 = Main.dust[num217];
				Dust dust362 = dust71;
				dust362.velocity *= 1.4f;
			}
			for (int num230 = 0; num230 < 10; num230++)
			{
				int num220 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default(Color), 1.25f);
				Main.dust[num220].noGravity = true;
				Dust dust74 = Main.dust[num220];
				Dust dust362 = dust74;
				dust362.velocity *= 5f;
				num220 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default(Color), 0.75f);
				dust74 = Main.dust[num220];
				dust362 = dust74;
				dust362.velocity *= 3f;
			}
			Vector2 position67 = new Vector2(Projectile.position.X, Projectile.position.Y);
			Vector2 val = default(Vector2);
			int num229 = Gore.NewGore(Projectile.GetSource_FromThis(), position67, val, Main.rand.Next(61, 64), 1f);
			Gore gore20 = Main.gore[num229];
			Gore gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position68 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_FromThis(), position68, val, Main.rand.Next(61, 64), 1f);
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position69 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_FromThis(), position69, val, Main.rand.Next(61, 64), 1f);
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y -= 1f;
			Vector2 position70 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_FromThis(), position70, val, Main.rand.Next(61, 64), 1f);
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y -= 1f;


			if (Projectile.ai[1] == 0f)
            {
				for (int i=0; i<3; i++)
                {
					Projectile p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -5.5f).RotatedByRandom(MathHelper.PiOver4), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: 1);
					p.timeLeft += (int)Main.rand.Next(-3, 3);
                }
            }
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.timeLeft = Math.Min(Projectile.timeLeft, 3);
        }

		public override void OnHitPlayer(Player target, Player.HurtInfo hurt)
		{
			Projectile.timeLeft = Math.Min(Projectile.timeLeft, 3);
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity *= 0.5f;
            return false;
        }
    }
}