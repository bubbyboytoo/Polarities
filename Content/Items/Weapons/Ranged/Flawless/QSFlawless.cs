using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Global;
using Polarities.Content.Items.Weapons.Ranged.Atlatls;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.Items.Weapons.Ranged.Flawless
{
    public class QSFlawless : AtlatlBase
    {
        private Projectile shot;
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(40) };
        public override float BaseShotDistance => 40;

        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Force Projector");
            //Tooltip.SetDefault("Shoots projectiles encased in forcefields");
            Item.ResearchUnlockCount = 1;
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 46;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = RarityType<QS_RW_FlawlessRarity>();
            Item.UseSound = SoundID.Item1;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 14.5f;
            Item.useAmmo = AmmoID.Dart;
        }

        public override bool RealShoot(Player player, EntitySource_ItemUse_WithAmmo source, int index, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Projectile dart = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            Projectile forcefield = Projectile.NewProjectileDirect(source, position, velocity, ProjectileType<QSFlawlessCrystal>(), damage, knockback, player.whoAmI, ai0: dart.whoAmI);

            return false;
        }
    }

    public class QSFlawlessCrystal : ModProjectile
    {
        public Projectile shot;
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Forcefield");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.width = 18;
            Projectile.height = 44;

            DrawOffsetX = -2;
            DrawOriginOffsetY = -2;
            DrawOriginOffsetX = 0;

            Projectile.alpha = 128;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        int stickTarget = -1;
        Vector2 stickOffset = Vector2.Zero;
        bool grounded = false;

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                shot = Main.projectile[(int)Projectile.ai[0]];
                Projectile.frame = Main.rand.Next(0, 3);
                Projectile.localAI[0] = 1;
            }

            if (shot.GetGlobalProjectile<Projectiles.PolaritiesProjectile>().justCollidedWithGround)
            {
                SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
                stickOffset = -Projectile.velocity;
                grounded = true;
                Projectile.timeLeft = 300;
                Projectile.localAI[0] = 2;
                shot.GetGlobalProjectile<Projectiles.PolaritiesProjectile>().justCollidedWithGround = false;
            }

            if (Projectile.localAI[0] == 1)
            {
                Projectile.Center = shot.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = shot.rotation;
            }
            if (Projectile.localAI[0] == 2)
            {
                if (!grounded)
                {
                    Projectile.position = Main.npc[stickTarget].position + stickOffset;
                    if (Main.npc[stickTarget].life <= 0) Projectile.localAI[0] = 3;
                }
            }
            if (Projectile.localAI[0] == 3)
            {
                Projectile.Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item27, target.position);
            if (target.GetGlobalNPC<PolaritiesNPC>().qsFlawlessCrystal == -1 && !grounded)
            {
                target.GetGlobalNPC<PolaritiesNPC>().qsFlawlessCrystal = Projectile.whoAmI;
                stickTarget = target.whoAmI;
                stickOffset = Projectile.position - target.position;
                Projectile.localAI[0] = 2;
                Projectile.timeLeft = 300;
                Projectile.friendly = false;
            }
            else if (!grounded)
            {
                Main.projectile[target.GetGlobalNPC<PolaritiesNPC>().qsFlawlessCrystal].scale += 0.3f / Main.projectile[target.GetGlobalNPC<PolaritiesNPC>().qsFlawlessCrystal].scale;
                Projectile.Kill();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            if (stickTarget > -1)
            {
                Main.npc[stickTarget].GetGlobalNPC<PolaritiesNPC>().qsFlawlessCrystal = -1;
                for (int i = -2; i < (Projectile.scale - 0.7f) / 0.3f; i++)
                {
                    Vector2 velocity = (6 * stickOffset.SafeNormalize(Vector2.Zero)) + Main.rand.NextVector2CircularEdge(16, 16);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[stickTarget].position, velocity, ProjectileType<QSFlawlessShard>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.frame);
                }
            }
            else if (grounded)
            {
                for (int i = -2; i < (Projectile.scale - 0.7f) / 0.3f; i++)
                {
                    Vector2 velocity = (6 * stickOffset.SafeNormalize(Vector2.Zero)) + Main.rand.NextVector2CircularEdge(16, 16);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, velocity, ProjectileType<QSFlawlessShard>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.frame);
                }
            }
            base.OnKill(timeLeft);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }

    public class QSFlawlessShard : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Forcefield");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 14;
            Projectile.height = 10;
            Projectile.alpha = 0;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.frame = (int)Projectile.ai[0];

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.3f;
        }
    }
}