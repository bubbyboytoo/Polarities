using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;

namespace Polarities.Content.Items.Ammo.Hardmode
{
	public class BlacksunBullet : ModItem
	{
		public override void SetStaticDefaults() {
            //DisplayName.SetDefault("Blacksun Bullet");
			//Tooltip.SetDefault("Shoots solar bullets that are orbited by moons when fired");
		}

		public override void SetDefaults() {
			Item.damage = 16;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 14;
			Item.maxStack = 9999;
			Item.consumable = true;             //You need to set the item consumable so that the ammo would automatically consumed
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(0,0,0,2);
			Item.rare = 8;
			Item.shoot = ProjectileType<SunBullet>();   //The projectile shoot when your weapon using this ammo
			Item.shootSpeed = 2f;                  //The speed of the projectile
			Item.ammo = AmmoID.Bullet;              //The ammo class this ammo belongs to.
		}

        public override void AddRecipes() {
            CreateRecipe(100)
                .AddIngredient(ItemID.EmptyBullet, 100)
                .AddIngredient(ItemType<Materials.Hardmode.EclipxieDust>())
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
    
    public class SunBullet : ModProjectile
	{
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Solar Bullet");
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.extraUpdates = 1;
        }

        private Vector2 trueVelocity;
        private float timer;
        private Projectile shot;
        private bool orbiting = true;

        public override void AI() {
            if (Projectile.localAI[0] == 0) {
                Projectile.localAI[0] = 1;

                Projectile.damage = (3*Projectile.damage)/4;

                shot = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(),Projectile.Center,Projectile.velocity,ProjectileType<MoonBullet>(),Projectile.damage,Projectile.knockBack,Projectile.owner,Projectile.whoAmI)];
                shot.extraUpdates = Projectile.extraUpdates;
                shot.GetGlobalProjectile<Projectiles.PolaritiesProjectile>().eclipxieBulletOrbit = Projectile.whoAmI;


                trueVelocity = Projectile.velocity*MathHelper.PiOver2;
                if (trueVelocity.Length() > 16) {
                    trueVelocity.Normalize();
                    trueVelocity *= 16;
                }

                timer = 0.15f;
            }

            Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, Scale: 1f)];
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            dust = Main.dust[Dust.NewDust(Projectile.position-Projectile.velocity/2, Projectile.width, Projectile.height, 6, Scale: 1f)];
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;

            Projectile.rotation = Projectile.velocity.ToRotation()+(float)Math.PI/2;
        }
        
        public override void OnKill(int timeLeft)
		{
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Tink, Projectile.position);
		}
    }
    
    public class MoonBullet : ModProjectile
	{
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Lunar Bullet");
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
        }

        private Vector2 trueVelocity;
        private float timer;
        private Projectile shot;
        private bool orbiting = true;

        public override void AI() {

            Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 15, Scale: 1f)];
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            dust = Main.dust[Dust.NewDust(Projectile.position-Projectile.velocity/2, Projectile.width, Projectile.height, 15, Scale: 1f)];
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;

            Projectile.rotation = Projectile.velocity.ToRotation()+(float)Math.PI/2;
        }
        
        public override void OnKill(int timeLeft)
		{
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			//SoundEngine.PlaySound(0, Projectile.position);
		}
    }
}
