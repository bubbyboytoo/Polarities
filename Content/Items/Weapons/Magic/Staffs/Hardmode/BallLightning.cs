using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.Items.Weapons.Magic.Staffs.Hardmode
{
	public class BallLightning : ModItem
	{
		public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.damage = 55;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 20;
			Item.width = 16;
			Item.height = 16;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.knockBack = 8;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item9;
			Item.shoot = ProjectileType<BallLightningOrb>();
			Item.shootSpeed = 10f;
		}

        public override void AddRecipes() {
            //          ModRecipe recipe = new ModRecipe(mod);
            //          recipe.AddIngredient(ItemType<Items.Placeable.PolarizedBar>(),6);
            //          recipe.AddIngredient(ItemType<Items.SmiteSoul>(),16);
            //          recipe.AddTile(TileID.MythrilAnvil);
            //          recipe.SetResult(this);
            //          recipe.AddRecipe();
            CreateRecipe()
                          .AddIngredient(ItemType<Placeable.Bars.PolarizedBar>(), 6)
                          .AddIngredient(ItemType<Materials.Hardmode.SmiteSoul>(), 16)
                          .AddTile(TileID.MythrilAnvil)
                          .Register();
        }

		public override void UpdateInventory(Player player)
		{
			Item.autoReuse = false;
		}
	}

    public class BallLightningOrb : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = true;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;

            if (Main.player[Projectile.owner].channel)
            {
                Projectile.timeLeft = 2;
                Projectile.velocity = Main.MouseWorld - Projectile.Center;
                if (Projectile.velocity.Length() > 16)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 16;
                }
                Projectile.netUpdate = true;
            }

            //sound delay counts down automatically
            if (Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(SoundID.Item93, Projectile.Center);
                Projectile.soundDelay = 20;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 2;
            }

            Projectile.rotation += (float)Math.PI / 2 + 0.1f;

            if (Main.rand.NextBool(10) && Main.myPlayer == Projectile.owner)
            {
                //release projectile in random direction
                Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(4, 0).RotatedByRandom(2 * Math.PI), ProjectileType<BallLightningProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner)].netUpdate = true;
            }
        }

        public override System.Boolean TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 10; i++)
                {
                    //release projectile in random direction
                    Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(4, 0).RotatedByRandom(2 * Math.PI), ProjectileType<BallLightningProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner)].netUpdate = true;
                }
            }
        }
    }

    public class BallLightningProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Lightning");
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 127;
        }

        public override void AI()
        {
            Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Electric, newColor: Color.LightBlue, Scale: 0.5f);
            d.noGravity = true;
            d.velocity /= 4;

            //int targetID = PolaritiesProjectile.FindMinionTarget(projectile, 2000f, requireLineOfSight: false, respectTarget: false);
            int targetID = -1;
            Projectile.Minion_FindTargetInRange(2000, ref targetID, skipIfCannotHitWithOwnBody: false);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 4;

            if (target != null)
            {
                Vector2 a = target.Center - Projectile.Center + target.velocity / 0.2f;
                if (a.Length() > 1) { a.Normalize(); }
                a *= 0.2f;
                Projectile.velocity += a;
            }

            if (Projectile.ai[0]++ % 10 == 0)
            {
                if (target != null) Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-15, 15)));
                else Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-30, 30)));
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }
        }
    }
}