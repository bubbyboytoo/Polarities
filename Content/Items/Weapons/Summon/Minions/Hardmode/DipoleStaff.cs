using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Polarities.Content.Items.Weapons.Summon.Minions.Hardmode
{
	public class DipoleStaff : ModItem
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Dipole Staff");
			//Tooltip.SetDefault("Summons miniature polarities to protect you");
		}

		public override void SetDefaults() {
			Item.damage = 80;
			Item.DamageType = DamageClass.Summon;
			Item.width = 46;
			Item.height = 46;
			Item.mana = 10;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.buffType = BuffType<Polariminis>();
			Item.shoot = ProjectileType<MagnetoMini>();
			Item.shootSpeed = 2f;
		}

        public override System.Boolean Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, System.Int32 type, System.Int32 damage, System.Single knockback)
        {
			position = Main.MouseWorld;
			player.AddBuff(Item.buffType, 18000, true);
			int magnetomini = Projectile.NewProjectile(source, position, velocity, ProjectileType<MagnetoMini>(), damage, knockback, player.whoAmI);
			int electrimini = Projectile.NewProjectile(source, 2 * player.Center - position, Vector2.Zero, ProjectileType<ElectriMini>(), damage, knockback, player.whoAmI, magnetomini);
			Main.projectile[magnetomini].ai[0] = electrimini;
			return false;
		}
	}

	public class ElectriMini : ModProjectile
	{
		private float[] playerOffset = new float[3];
		private float radius;
		private int attackCooldown;
		private int turnCooldown;
		private int magnetomini
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Electrimini");
			Main.projFrames[Projectile.type] = 1;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 17;
			Projectile.height = 18;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.minionSlots = 0.5f;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
		}


		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				Projectile.active = false;
				return;
			}
			if (player.dead)
			{
				player.ClearBuff(BuffType<Polariminis>());
			}
			if (player.HasBuff(BuffType<Polariminis>()))
			{
				Projectile.timeLeft = 2;
				if (magnetomini != 0) { Main.projectile[magnetomini].active = true; }
			}

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				playerOffset[0] = Projectile.Center.X - player.Center.X;
				playerOffset[1] = Projectile.Center.Y - player.Center.Y;
				radius = (Projectile.Center - player.Center).Length();
			}

			//motion code
			Vector2 playerVelocity = player.position - player.oldPosition;
			float[] newPlayerOffset = {
				playerOffset[0] + playerVelocity.X * playerOffset[2]/radius
				,
				playerOffset[1] + playerVelocity.Y * playerOffset[2]/radius
				,
				playerOffset[2] - playerVelocity.X * playerOffset[0]/radius - playerVelocity.Y * playerOffset[1]/radius
			};

			float offsetDist = (float)Math.Sqrt(newPlayerOffset[0] * newPlayerOffset[0] + newPlayerOffset[1] * newPlayerOffset[1] + newPlayerOffset[2] * newPlayerOffset[2]);
			newPlayerOffset[0] *= radius / offsetDist;
			newPlayerOffset[1] *= radius / offsetDist;
			newPlayerOffset[2] *= radius / offsetDist;

			playerOffset = newPlayerOffset;

			Projectile.velocity = Vector2.Lerp(Projectile.velocity,
				new Vector2(playerOffset[0], playerOffset[1]) + player.Center - Projectile.Center, 0.05f);
			Projectile.rotation = MathHelper.ToRadians(Projectile.velocity.X * 2);
			
			
			if (player.ownedProjectileCounts[Projectile.type] == 0)
			{
				return;
			}

			int index = 0;
			for (int i = 0; i < Projectile.whoAmI; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
				{
					index++;
				}
			}

			if (turnCooldown > 0)
			{
				turnCooldown--;
			}

			//int targetID = PolaritiesProjectile.FindMinionTarget(projectile, 750f, usePlayerDistance: true);
			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, skipIfCannotHitWithOwnBody: false);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			if (target != null)
			{
				if (attackCooldown == (240 * index / player.ownedProjectileCounts[Projectile.type]) % 240)
				{
					Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
					Projectile.ai[2] = (target.Center - Projectile.Center).ToRotation();
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(1, 0).RotatedBy(Projectile.ai[2]), ProjectileType<ElectriminiLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: Projectile.whoAmI);
					turnCooldown = 120;
				}
				attackCooldown = (attackCooldown + 1) % 240;
			}
			else
			{
				attackCooldown = 0;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return false;
		}
	}

	public class ElectriminiLaser : ModProjectile
	{
		// Use a different style for constant so it is very clear in code when a constant is used

		//The distance charge particle from the pixie center
		private const float MOVE_DISTANCE = 0f;

		// The actual distance is stored in the ai0 field
		// By making a property to handle this it makes our life easier, and the accessibility more readable
		public float Distance
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		//The electrimini which owns this laser
		public int owner
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Electrimini Deathray");
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = 120;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// We start drawing the laser
			DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Type].Value, Projectile.Center, Projectile.velocity, 8, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)MOVE_DISTANCE);
			return false;
		}

		// The core function of drawing a laser
		public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
		{
			float r = unit.ToRotation() + rotation;

			Color c = Color.White;
			int numDraws = 2;
			c.A = 127;
			for (int x = 0; x < numDraws; x++)
			{
				Vector2 laserOffset = unit.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(MathHelper.TwoPi * x / numDraws + Projectile.timeLeft / 5f);

				// Draws the laser 'body'
				for (float i = transDist; i <= Distance; i += step)
				{
					var origin = laserOffset + start + i * unit;

					spriteBatch.Draw(texture, origin - Main.screenPosition,
						new Rectangle(0, 0, 8, 8), i < transDist ? Color.Transparent : c, r,
						new Vector2(8 * .5f, 8 * .5f), scale, 0, 0);
				}
			}
		}

		public override void DrawBehind(System.Int32 index, System.Collections.Generic.List<System.Int32> behindNPCsAndTiles, System.Collections.Generic.List<System.Int32> behindNPCs, System.Collections.Generic.List<System.Int32> behindProjectiles, System.Collections.Generic.List<System.Int32> overPlayers, System.Collections.Generic.List<System.Int32> overWiresUI)
		{
			behindNPCs.Add(index);
		}

		private void SpawnDusts()
		{
			Vector2 unit = Projectile.velocity * -1;
			Vector2 dustPos = Projectile.Center + Projectile.velocity * (Distance + 8);

			if (Main.rand.NextBool(5))
			{
				Vector2 offset = Projectile.velocity.RotatedBy(1.57f) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
				Dust dust = Main.dust[Dust.NewDust(dustPos + offset - Vector2.One * 4f, 8, 8, DustID.Electric, 0.0f, 0.0f, 100, new Color(), 1.5f)];
				dust.noGravity = true;
			}
		}

		// Change the way of collision check of the projectile
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 unit = Projectile.velocity;
			float point = 0f;
			// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
			// It will look for collisions on the given line using AABB
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
				Projectile.Center + unit * Distance, 8, ref point);
		}

		public override void AI()
		{
			Projectile electris = Main.projectile[owner];
			Projectile.position = electris.BottomRight - Projectile.Hitbox.Size() / 2f;
			Projectile.velocity = new Vector2(1, 0).RotatedBy(electris.ai[2]);
			if (!electris.active) { Projectile.Kill(); }

			SetLaserPosition();
			SpawnDusts();
			CastLights();
		}

		/*
		 * Sets the end of the laser position based on where it collides with something
		 */
		private void SetLaserPosition()
		{
			for (Distance = MOVE_DISTANCE; Distance <= 1600f; Distance += 4f)
			{
				var start = Projectile.Center + Projectile.velocity * Distance;
				if (!Collision.CanHit(Projectile.Center, 1, 1, start, 1, 1))
				{
					//Distance -= 4f;
					break;
				}
			}
		}

		private void CastLights()
		{
			// Cast a light along the line of the laser
			DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - MOVE_DISTANCE), 26, DelegateMethods.CastLight);
		}

		public override bool ShouldUpdatePosition() => false;

		public override bool? CanCutTiles()
		{
			return false;
		}
	}

	public class MagnetoMini : ModProjectile
	{
		private float[] playerOffset = new float[3];
		private float radius;
		private int attackCooldown;
		private int electrimini
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Magnetomini");
			Main.projFrames[Projectile.type] = 1;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.minionSlots = 0.5f;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				Projectile.active = false;
				return;
			}
			if (player.dead)
			{
				player.ClearBuff(BuffType<Polariminis>());
			}
			if (player.HasBuff(BuffType<Polariminis>()))
			{
				Projectile.timeLeft = 2;
				if (electrimini != 0) { Main.projectile[electrimini].active = true; }
			}

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				playerOffset[0] = Projectile.Center.X - player.Center.X;
				playerOffset[1] = Projectile.Center.Y - player.Center.Y;
				radius = (Projectile.Center - player.Center).Length();
			}

			//motion code
			Vector2 playerVelocity = player.position - player.oldPosition;
			float[] newPlayerOffset = {
				playerOffset[0] + playerVelocity.X * playerOffset[2]/radius
				,
				playerOffset[1] + playerVelocity.Y * playerOffset[2]/radius
				,
				playerOffset[2] - playerVelocity.X * playerOffset[0]/radius - playerVelocity.Y * playerOffset[1]/radius
			};

			float offsetDist = (float)Math.Sqrt(newPlayerOffset[0] * newPlayerOffset[0] + newPlayerOffset[1] * newPlayerOffset[1] + newPlayerOffset[2] * newPlayerOffset[2]);
			newPlayerOffset[0] *= radius / offsetDist;
			newPlayerOffset[1] *= radius / offsetDist;
			newPlayerOffset[2] *= radius / offsetDist;

			playerOffset = newPlayerOffset;

			Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(playerOffset[0], playerOffset[1]) + player.Center - Projectile.Center, 0.05f);
			Projectile.rotation = MathHelper.ToRadians(Projectile.velocity.X * 2);


			//int targetID = PolaritiesProjectile.FindMinionTarget(projectile, 750f, usePlayerDistance: true);
			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, skipIfCannotHitWithOwnBody: false);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			if (target != null)
			{

				if (attackCooldown == 60)
				{
					attackCooldown = 0;

					Vector2 velocity = 4 * Projectile.Center.DirectionTo(target.Center);
					Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
					SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileType<MagnetominiRocket>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
					Projectile.velocity -= velocity * 2f;
				}
				attackCooldown++;
			}
			else
			{
				attackCooldown = 0;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return false;
		}
	}

	public class MagnetominiRocket : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Magnetomini Rocket");
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			//int targetID = PolaritiesProjectile.FindMinionTarget(projectile, 2000f, strictMaxDistance: false);
			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, skipIfCannotHitWithOwnBody: false);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			if (target != null)
			{
				Vector2 targetVelocity = target.position - target.oldPosition;

				float v = Projectile.velocity.Length();
				float a = targetVelocity.Y;
				float b = targetVelocity.X;
				float c = (target.Center.X - Projectile.Center.X);
				float d = (target.Center.Y - Projectile.Center.Y);

				float internalVal = -a * a * c * c + 2 * a * b * c * d - b * b * d * d + v * v * (c * c + d * d);

				float theta = internalVal >= 0 ?
					2 * (float)Math.Atan2(c * v - Math.Sqrt(internalVal),
					a * c - d * (b + v))
					:
					targetVelocity.ToRotation();

				if (theta > MathHelper.Pi)
				{
					theta -= MathHelper.TwoPi;
				}
				else if (theta < -MathHelper.Pi)
				{
					theta += MathHelper.TwoPi;
				}

				float angleDiff = theta - Projectile.velocity.ToRotation();
				for (int i = 0; i < 2; i++)
				{
					if (angleDiff > MathHelper.Pi)
					{
						angleDiff -= MathHelper.TwoPi;
					}
					else if (angleDiff < -MathHelper.Pi)
					{
						angleDiff += MathHelper.TwoPi;
					}
				}

				Projectile.velocity = Projectile.velocity.RotatedBy(Math.Min(1f / Projectile.velocity.Length(), Math.Max(-1f / Projectile.velocity.Length(), angleDiff)));
			}

			Projectile.velocity *= 1.01f;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 235, newColor: Color.Red, Scale: 0.75f)].noGravity = true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.Kill();
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			Projectile.Kill();
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.Kill();
			return false;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			for (int i = 0; i < 5; i++)
			{
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, 235, newColor: Color.Red, Scale: 0.5f)].noGravity = true;
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, DustID.Smoke, newColor: Color.Black, Scale: 0.5f)].noGravity = true;
			}
		}
	}

	public class Polariminis : ModBuff
	{

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Polariminis");
			//Description.SetDefault("The Polariminis will fight for you");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<MagnetoMini>()] + player.ownedProjectileCounts[ProjectileType<ElectriMini>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}