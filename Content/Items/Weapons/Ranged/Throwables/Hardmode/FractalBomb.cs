using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.GameContent.Achievements;

namespace Polarities.Content.Items.Weapons.Ranged.Throwables.Hardmode
{
	public class FractalBomb : ModItem
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Fractal Bomb");
			//Tooltip.SetDefault("Splits into more bombs on exploding");
			Item.ResearchUnlockCount = 99;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 24;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.value = 50;
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
			Item.shoot = ProjectileType<FractalBombProjectile>();
			Item.shootSpeed = 5.5f;
			Item.maxStack = 9999;
		}

        public override void AddRecipes()
        {
			CreateRecipe(50)
				.AddIngredient(ItemType<Placeable.Bars.FractalBar>())
				.AddIngredient(ItemID.Bomb, 50)
				.Register();
		}
    }

	public class FractalBombProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/Items/Weapons/Ranged/Throwables/Hardmode/FractalBomb";

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Fractal Bomb");
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			DrawOriginOffsetY = -2;

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
				Projectile.damage = 160;
				Projectile.knockBack = 8f;
			}
			else
            {
				Projectile.damage = 0;

				if (Main.rand.NextBool())
				{
					Vector2 position101 = new Vector2(Projectile.position.X, Projectile.position.Y);
					int width73 = Projectile.width;
					int height73 = Projectile.height;
					Color newColor4 = default(Color);
					int num2300 = Dust.NewDust(position101, width73, height73, DustID.Smoke, 0f, 0f, 100, newColor4);
					Main.dust[num2300].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[num2300].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[num2300].noGravity = true;
					Dust obj3 = Main.dust[num2300];
					Vector2 center30 = Projectile.Center;
					Vector2 spinningpoint24 = new Vector2(0f, (float)(-Projectile.height / 2));
					double radians2 = Projectile.rotation;
					Vector2 val4 = default(Vector2);
					obj3.position = center30 + Utils.RotatedBy(spinningpoint24, radians2, val4) * 1.1f;
					Main.rand.Next(2);
					Vector2 position102 = new Vector2(Projectile.position.X, Projectile.position.Y);
					int width74 = Projectile.width;
					int height74 = Projectile.height;
					newColor4 = default(Color);
					num2300 = Dust.NewDust(position102, width74, height74, DustID.Electric, 0f, 0f, 100, newColor4, 0.5f);
					Main.dust[num2300].scale = 1f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[num2300].noGravity = true;
					Dust obj4 = Main.dust[num2300];
					Vector2 center31 = Projectile.Center;
					Vector2 spinningpoint25 = new Vector2(0f, (float)(-Projectile.height / 2 - 6));
					double radians3 = Projectile.rotation;
					val4 = default(Vector2);
					obj4.position = center31 + Utils.RotatedBy(spinningpoint25, radians3, val4) * 1.1f;
				}
            }

			Projectile.ai[0]++;

			if (Projectile.ai[0] > 5f)
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
			num229 = Gore.NewGore(Projectile.GetSource_FromThis(), position70, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y -= 1f;


			if (Projectile.ai[1] == 0f)
			{
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -5.5f).RotatedByRandom(MathHelper.PiOver4), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: 1);
				}
			}


			//explode stuff
			{
				int explosionRadius = 4;

				int minTileX = (int)(Projectile.Center.X / 16f - (float)explosionRadius);
				int maxTileX = (int)(Projectile.Center.X / 16f + (float)explosionRadius);
				int minTileY = (int)(Projectile.Center.Y / 16f - (float)explosionRadius);
				int maxTileY = (int)(Projectile.Center.Y / 16f + (float)explosionRadius);
				if (minTileX < 0)
				{
					minTileX = 0;
				}
				if (maxTileX > Main.maxTilesX)
				{
					maxTileX = Main.maxTilesX;
				}
				if (minTileY < 0)
				{
					minTileY = 0;
				}
				if (maxTileY > Main.maxTilesY)
				{
					maxTileY = Main.maxTilesY;
				}
				bool canKillWalls = false;
				for (int x = minTileX; x <= maxTileX; x++)
				{
					for (int y = minTileY; y <= maxTileY; y++)
					{
						float diffX = Math.Abs((float)x - Projectile.Center.X / 16f);
						float diffY = Math.Abs((float)y - Projectile.Center.Y / 16f);
						double distance = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distance < (double)explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].WallType == 0)
						{
							canKillWalls = true;
							break;
						}
					}
				}
				AchievementsHelper.CurrentlyMining = true;
				for (int i = minTileX; i <= maxTileX; i++)
				{
					for (int j = minTileY; j <= maxTileY; j++)
					{
						float diffX = Math.Abs((float)i - Projectile.Center.X / 16f);
						float diffY = Math.Abs((float)j - Projectile.Center.Y / 16f);
						double distanceToTile = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distanceToTile < (double)explosionRadius)
						{
							bool canKillTile = true;
							if (Main.tile[i, j] != null && Main.tile[i, j].HasTile)
							{
								canKillTile = true;
								if (Main.tileDungeon[(int)Main.tile[i, j].TileType] || Main.tile[i, j].TileType == 88 || Main.tile[i, j].TileType == 21 || Main.tile[i, j].TileType == 26 || Main.tile[i, j].TileType == 107 || Main.tile[i, j].TileType == 108 || Main.tile[i, j].TileType == 111 || Main.tile[i, j].TileType == 226 || Main.tile[i, j].TileType == 237 || Main.tile[i, j].TileType == 221 || Main.tile[i, j].TileType == 222 || Main.tile[i, j].TileType == 223 || Main.tile[i, j].TileType == 211 || Main.tile[i, j].TileType == 404)
								{
									canKillTile = false;
								}
								if (!Main.hardMode && Main.tile[i, j].TileType == 58)
								{
									canKillTile = false;
								}
								if (!TileLoader.CanExplode(i, j))
								{
									canKillTile = false;
								}
								if (canKillTile)
								{
									WorldGen.KillTile(i, j, false, false, false);
									if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
									{
										NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
									}
								}
							}
							if (canKillTile)
							{
								for (int x = i - 1; x <= i + 1; x++)
								{
									for (int y = j - 1; y <= j + 1; y++)
									{
										if (Main.tile[x, y] != null && Main.tile[x, y].WallType > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].WallType))
										{
											WorldGen.KillWall(x, y, false);
											if (Main.tile[x, y].WallType == 0 && Main.netMode != NetmodeID.SinglePlayer)
											{
												NetMessage.SendData(17, -1, -1, null, 2, (float)x, (float)y, 0f, 0, 0, 0);
											}
										}
									}
								}
							}
						}
					}
				}
				AchievementsHelper.CurrentlyMining = false;
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