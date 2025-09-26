using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.DataStructures;

namespace Polarities.Content.Items.Weapons.Ranged.Launchers.Hardmode
{
	public class MissileCharger : ModItem
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Missile Charger");
			//Tooltip.SetDefault("Shoots homing magnetized rockets");
		}

		public override void SetDefaults() {
			Item.damage = 130;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 32;
			Item.height = 24;
			Item.useTime = 3;
			Item.useAnimation = 15;
            Item.reuseDelay = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 10;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item61;
			Item.autoReuse = true;
			Item.shoot = 134;
			Item.shootSpeed = 8f;
			Item.useAmmo = AmmoID.Rocket;
		}

        public override System.Boolean Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, System.Int32 type, System.Int32 damage, System.Single knockback)
        {
            int size = 80;
            int explode = 0;
            switch (type) {
                case 134:
                    break;
                case 137:
                    explode = 1;
                    break;
                case 140:
                    size = 160;
                    break;
                case 143:
                    size = 160;
                    explode = 1;
                    break;
            }
            type = ProjectileType<MissileChargerRocket>();
            float angle = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
            Projectile.NewProjectile(source,position,velocity.RotatedBy(angle),type,damage,knockback,player.whoAmI,ai0: size, ai1: explode);
			return false;
		}

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-19, -3);
        }

        public override void AddRecipes() 
        {
            CreateRecipe()
                    .AddIngredient(ItemID.RocketLauncher)
                    .AddIngredient(ItemType<Placeable.Bars.PolarizedBar>(), 13)
                    .AddIngredient(ItemType<Materials.Hardmode.SmiteSoul>(), 6)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
        }
	}


	public class MissileChargerRocket : ModProjectile
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Magnetic Missile");
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.aiStyle = -1;
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = 0;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 0;
			Projectile.alpha = 0;
			Projectile.timeLeft = 480;
            Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

        public override void AI() {
            if (Projectile.timeLeft == 1) {
                Projectile.width = (int)Projectile.ai[0];
                Projectile.height = (int)Projectile.ai[0];
                Projectile.position -= new Vector2((Projectile.ai[0]-16)/2,(Projectile.ai[0]-16)/2);
                Projectile.hide = true;
                return;
            }

            //int targetID = PolaritiesProjectile.FindMinionTarget(projectile, 750f, requireLineOfSight: false, respectTarget: false);
            int targetID = -1;
            Projectile.Minion_FindTargetInRange(750, ref targetID, skipIfCannotHitWithOwnBody: false);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            if (target != null) {
                Vector2 targetVelocity = target.position - target.oldPosition;

                float v = Projectile.velocity.Length();
                float a = targetVelocity.Y;
                float b = targetVelocity.X;
                float c = (target.Center.X-Projectile.Center.X);
                float d = (target.Center.Y-Projectile.Center.Y);

                float internalVal = -a*a*c*c+2*a*b*c*d-b*b*d*d+v*v*(c*c+d*d);

                float theta = internalVal >= 0 ? 
                    2*(float)Math.Atan2(c*v - Math.Sqrt(internalVal),
                    a*c-d*(b+v))
                    :
                    targetVelocity.ToRotation();

                if (theta > MathHelper.Pi) {
                    theta -= MathHelper.TwoPi;
                } else if (theta < -MathHelper.Pi) {
                    theta += MathHelper.TwoPi;
                }

                float angleDiff = theta - Projectile.velocity.ToRotation();
                for(int i = 0; i<2; i++) {
                    if (angleDiff > MathHelper.Pi) {
                        angleDiff -= MathHelper.TwoPi;
                    } else if (angleDiff < -MathHelper.Pi) {
                        angleDiff += MathHelper.TwoPi;
                    }
                }

                Projectile.velocity = Projectile.velocity.RotatedBy(Math.Min(1f/Projectile.velocity.Length(),Math.Max(-1f/Projectile.velocity.Length(),angleDiff)));
            }

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 5) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
			}

            Projectile.velocity *= 1.01f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Main.dust[Dust.NewDust(Projectile.position,Projectile.width,Projectile.height, 235 ,newColor:Color.Red,Scale:2f)].noGravity = true;
        }

		public override bool OnTileCollide(Vector2 oldVelocity) {
            if (Projectile.timeLeft > 1) {
			    Projectile.timeLeft = 2;
                Projectile.tileCollide = false;
            }
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (Projectile.timeLeft > 1) {
                target.immune[Projectile.owner] = 0;
			    Projectile.timeLeft = 2;
                Projectile.tileCollide = false;
            }
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
			for (int i=0; i<Projectile.ai[0]/4; i++) {
				Main.dust[Dust.NewDust(Projectile.position,Projectile.width,Projectile.height, 235 ,newColor:Color.Red, Scale:4f)].noGravity = true;
				Main.dust[Dust.NewDust(Projectile.position,Projectile.width,Projectile.height, DustID.Smoke ,newColor:Color.Black, Scale:2f)].noGravity = true;
			}

            if (Projectile.ai[1] == 1) {
                //explode stuff

                {
                    int explosionRadius = (int)Projectile.ai[0]/32;

                    int minTileX = (int)(Projectile.Center.X / 16f - (float)explosionRadius);
                    int maxTileX = (int)(Projectile.Center.X / 16f + (float)explosionRadius);
                    int minTileY = (int)(Projectile.Center.Y / 16f - (float)explosionRadius);
                    int maxTileY = (int)(Projectile.Center.Y / 16f + (float)explosionRadius);
                    if (minTileX < 0) {
                        minTileX = 0;
                    }
                    if (maxTileX > Main.maxTilesX) {
                        maxTileX = Main.maxTilesX;
                    }
                    if (minTileY < 0) {
                        minTileY = 0;
                    }
                    if (maxTileY > Main.maxTilesY) {
                        maxTileY = Main.maxTilesY;
                    }
                    bool canKillWalls = false;
                    for (int x = minTileX; x <= maxTileX; x++) {
                        for (int y = minTileY; y <= maxTileY; y++) {
                            float diffX = Math.Abs((float)x - Projectile.Center.X / 16f);
                            float diffY = Math.Abs((float)y - Projectile.Center.Y / 16f);
                            double distance = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
                            if (distance < (double)explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].WallType == 0) {
                                canKillWalls = true;
                                break;
                            }
                        }
                    }
                    AchievementsHelper.CurrentlyMining = true;
                    for (int i = minTileX; i <= maxTileX; i++) {
                        for (int j = minTileY; j <= maxTileY; j++) {
                            float diffX = Math.Abs((float)i - Projectile.Center.X / 16f);
                            float diffY = Math.Abs((float)j - Projectile.Center.Y / 16f);
                            double distanceToTile = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
                            if (distanceToTile < (double)explosionRadius) {
                                bool canKillTile = true;
                                if (Main.tile[i, j] != null) {
                                    canKillTile = true;
                                    if (Main.tileDungeon[(int)Main.tile[i, j].TileType] || Main.tile[i, j].TileType == 88 || Main.tile[i, j].TileType == 21 || Main.tile[i, j].TileType == 26 || Main.tile[i, j].TileType == 107 || Main.tile[i, j].TileType == 108 || Main.tile[i, j].TileType == 111 || Main.tile[i, j].TileType == 226 || Main.tile[i, j].TileType == 237 || Main.tile[i, j].TileType == 221 || Main.tile[i, j].TileType == 222 || Main.tile[i, j].TileType == 223 || Main.tile[i, j].TileType == 211 || Main.tile[i, j].TileType == 404) {
                                        canKillTile = false;
                                    }
                                    if (!Main.hardMode && Main.tile[i, j].TileType == 58) {
                                        canKillTile = false;
                                    }
                                    if (!TileLoader.CanExplode(i, j)) {
                                        canKillTile = false;
                                    }
                                    if (canKillTile) {
                                        WorldGen.KillTile(i, j, false, false, false);
                                        if (Main.netMode != 0) {
                                            NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
                                        }
                                    }
                                }
                                if (canKillTile) {
                                    for (int x = i - 1; x <= i + 1; x++) {
                                        for (int y = j - 1; y <= j + 1; y++) {
                                            if (Main.tile[x, y] != null && Main.tile[x, y].WallType > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].WallType)) {
                                                WorldGen.KillWall(x, y, false);
                                                if (Main.tile[x, y].WallType == 0 && Main.netMode != 0) {
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
		}
	}
}
