using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Core;
using Polarities.Global;
using Polarities.Assets;
using Polarities.Content.Buffs;
using Polarities.Content.Projectiles;
using Polarities.Content.Items;
using Polarities.Content.Items.Weapons;
using Polarities.Content.Items.Armor;
using Polarities.Content.Items.Placeable.Trophies;
using Polarities.Content.Items.Placeable.Relics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.Utilities;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.NPCs.Bosses.Hardmode.MagnetonElectris
{
	public enum PolaritiesAttackIDs
    {
		SpawnTransition = 0,
		AlternateEyeblasts = 1,
		ShrinkRingsThenEyeblasts = 2,
		AuraAttacks = 3,
		HighToMidTransition = 4,
		FastEyeblasts = 5,
		CloserShrinkRings = 6,
		AuraAndBolts = 7,
		SmallVortexAttack = 8,
		ProjectilesAndBolts = 9,
		MagnetonDeathray = 10,
		AurasAndRings = 11,
		CircleDeathrays = 12,
		MidToLowTransition = 13,
		AnticipatingRays = 14,
		LargeVortexAttack = 15,
		HarderCircleDeathrays = 16
	}

	[AutoloadBossHead]
	public class Electris : ModNPC
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electris");
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 64;
			NPC.height = 64;
			DrawOffsetY = 40;

			NPC.defense = 60;
			NPC.lifeMax = 120000;
			if (Main.expertMode)
			{
				NPC.lifeMax = 180000;
			}
			NPC.damage = 120;
			if (Main.expertMode)
			{
				NPC.damage = 80;
			}

			NPC.knockBackResist = 0f;
			NPC.value = Item.buyPrice(1, 0, 0, 0);
			NPC.npcSlots = 15f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit34;
			NPC.DeathSound = SoundID.NPCDeath37;

			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.OnFire] = true;

			Music = MusicID.Boss3;
		}

		//public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
		//{
			//NPC.lifeMax = (int)(180000 * bossLifeScale);
		//}

		private float[] playerOffset = { -600f, 0f, 0f };

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<PolaritiesTrophy>(), 10));
			npcLoot.Add(ItemDropRule.BossBag(ItemType<Items.Consumables.TreasureBags.Hardmode.PolaritiesBag>()));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<EsophageRelic>()));

			npcLoot.Add(ItemDropRule.Common(ItemType<Items.Vanity.Hardmode.ElectrisMask>(), 7));
			//TODO: npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<EsophagePetItem>(), 4));

			//normal mode loot
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Accessories.Wings.ElectricWings>(), 15));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Weapons.Magic.Staffs.Hardmode.DiamagneticDischarge>(), 3));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Weapons.Magic.Books.Hardmode.MagneticStreams>(), 3));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Materials.Hardmode.SmiteSoul>(), 1, 20, 39));

			npcLoot.Add(notExpertRule);

			//npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Contagun>()));
		}

		public override void BossHeadRotation(ref float rotation)
        {
			rotation = NPC.rotation - MathHelper.PiOver2;
        }

        public override void AI()
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead)
				{
					if (NPC.timeLeft > 10)
					{
						NPC.timeLeft = 10;
					}
					NPC.velocity = (NPC.Center - player.Center) / 60;

					//frames code
					NPC.frameCounter++;
					if (NPC.frameCounter >= 7)
					{
						NPC.frameCounter = 0;
					}
					return;
				}
			}

			if (NPC.localAI[0] == 0)
            {
				NPC.localAI[0] = 1;
				NPC.rotation = MathHelper.PiOver2;

				Vector2 spawnPosition = player.MountedCenter * 2 - NPC.Center;

                if (Main.netMode != NetmodeID.MultiplayerClient)
				{
                    //NPC.ai[0] = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), spawnPosition.X, spawnPosition.Y, NPCType<Magneton>(), ai0: NPC.whoAmI);
                    NPC.ai[0] = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)player.Center.X, (int)player.Center.Y, NPCType<Magneton>(), ai0: NPC.whoAmI);
                    //NPC.ai[0] = NPC.NewNPC(spawnPosition.X, spawnPosition.Y, NPCType<Magneton>(), ai0: NPC.whoAmI);
                }
				NPC.netUpdate = true;
			}

			if (NPC.ai[1] == 2)
			{
				NPC.velocity = Vector2.Zero;

				if (NPC.ai[3] == 0)
				{
					SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);

					//Gore.NewGore(NPC.Center, (NPC.Center - Main.npc[(int)NPC.ai[0]].Center).SafeNormalize(Vector2.Zero) * 8, Mod.GetGoreSlot("Gores/ElectrisGore"));

					for (int i = 0; i < 64; i++)
					{
						Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.NextFloat(NPC.width / 2), 0).RotatedBy(i * MathHelper.TwoPi / 64f), DustID.Electric, Vector2.Zero, Scale: 1.5f).noGravity = true;
					}
				}
				Dust.NewDustPerfect(NPC.Center, DustID.Electric, Vector2.Zero, Scale: 1.5f).noGravity = true;

				NPC.hide = true;

				PolaritiesSky.alpha = (float)Math.Sqrt((479 - NPC.ai[3]) / 480f);

				Vector2 goalCenter = (NPC.Center + Main.npc[(int)NPC.ai[0]].Center) / 2f;
				float rotation = 10f / (float)Math.Pow(480 - NPC.ai[3], 1.5f);
				Vector2 goalPosition = goalCenter + (NPC.Center - goalCenter).SafeNormalize(Vector2.Zero).RotatedBy(rotation) * ((NPC.Center - goalCenter).Length() * (float)Math.Sqrt((479f - NPC.ai[3]) / (480f - NPC.ai[3])));
				NPC.Center = goalPosition;// - npc.Center;

				NPC.ai[3]++;
				if (NPC.ai[3] >= 480)
				{
					NPC.life = 0;
					NPC.boss = false;
					foreach (NPC npc in Main.npc)
                    {
						if (npc.type == NPCType<Magneton>())
                        {
							npc.life = 0;
							npc.boss = false;
                        }
                    }
					Main.NewText("The Polarities have been defeated!", 171, 64, 255);
					NPC.checkDead();
				}
				return;
			}

			//motion code
			Vector2 playerVelocity = player.position - player.oldPosition;
			float[] newPlayerOffset = {
				playerOffset[0] + playerVelocity.X * playerOffset[2] / 600f
				,
				playerOffset[1] + playerVelocity.Y * playerOffset[2] / 600f
				,
				playerOffset[2] - playerVelocity.X * playerOffset[0] / 600f - playerVelocity.Y * playerOffset[1] / 600f
			};

			float offsetDist = (float)Math.Sqrt(newPlayerOffset[0] * newPlayerOffset[0] + newPlayerOffset[1] * newPlayerOffset[1] + newPlayerOffset[2] * newPlayerOffset[2]);
			newPlayerOffset[0] *= 600f / offsetDist;
			newPlayerOffset[1] *= 600f / offsetDist;
			newPlayerOffset[2] *= 600f / offsetDist;

			playerOffset = newPlayerOffset;

			NPC.velocity = new Vector2(playerOffset[0], playerOffset[1]) + player.Center - NPC.Center;

			//sky color
			PolaritiesSky.height = playerOffset[2] / 600f;
			PolaritiesSky.alpha = 1f;


			if (NPC.ai[1] == 0)
            {
				NPC.dontTakeDamage = false;
			}

			float angleGoal = NPC.rotation;
			float maxTurn = 0.2f;
			int frameLength = 7;
			NPC.scale = 1;
			NPC.alpha = 0;

			bool canPhaseTransition = false;

			//attack code
			switch ((PolaritiesAttackIDs)NPC.ai[2])
            {
				case PolaritiesAttackIDs.SpawnTransition:
					//spawn animation
					NPC.dontTakeDamage = true;

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					PolaritiesSky.alpha = (NPC.ai[3] + 1) / 240f;
					NPC.scale = (NPC.ai[3] + 1) / 240f;
					NPC.alpha = (int)(255 - 255 * ((NPC.ai[3] + 1) / 240f));
					frameLength = (int)(4f + 3f * (NPC.ai[3] + 1) / 240f);

					if (NPC.ai[3] % 20 == 0)
					{
						SoundEngine.PlaySound(SoundID.Item15, NPC.Center);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 240)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AlternateEyeblasts;

						canPhaseTransition = true;
					}
					break;
				case PolaritiesAttackIDs.AlternateEyeblasts:
					//alternating eyeblasts

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (NPC.ai[3] % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
                    }

					NPC.ai[3]++;
					if (NPC.ai[3] >= 240)
                    {
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.ShrinkRingsThenEyeblasts;

						canPhaseTransition = true;
					}
					break;
				case PolaritiesAttackIDs.ShrinkRingsThenEyeblasts:
					//shrinking rings then more eyeblasts

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 1; i++)
						{
							float angle = i * MathHelper.TwoPi / 1f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f, 0).RotatedBy(angle), new Vector2(0, 20f).RotatedBy(angle), ProjectileType<ElectrisRing>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (NPC.ai[3] % 60 == 0 && NPC.ai[3] >= 120 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 360)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AuraAttacks;

						canPhaseTransition = true;
					}
					break;
				case PolaritiesAttackIDs.AuraAttacks:
					//aura attacks

					if (NPC.ai[3] == 30 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<ElectronCloud>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -1);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 60)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AlternateEyeblasts;
					}
					break;
				case PolaritiesAttackIDs.HighToMidTransition:
					//high to mid health transition
					NPC.dontTakeDamage = true;

					angleGoal = NPC.rotation + 0.2f * (NPC.ai[3]) * (240 - NPC.ai[3]) / (120f * 120f);
					frameLength = (int)(7f + 5f * (NPC.ai[3]) * (240 - NPC.ai[3]) / (120f * 120f));

					NPC.ai[3]++;
					if (NPC.ai[3] >= 240)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.FastEyeblasts;
					}
					break;
				case PolaritiesAttackIDs.FastEyeblasts:
					//buffed eyeblasts, electris's accelerate more rapidly and magnet boi's are shot at angles

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (NPC.ai[3] % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(0.05f) * 8f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.04f);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(-0.05f) * 8f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.04f);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 240)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.CloserShrinkRings;
					}
					break;
				case PolaritiesAttackIDs.CloserShrinkRings:
					//much tighter shrinking rings followed up with more eyeblasts

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 1; i++)
						{
							float angle = i * MathHelper.TwoPi / 1f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f, 0).RotatedBy(angle), new Vector2(0, 20f).RotatedBy(angle), ProjectileType<ElectrisRing2>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					if ((NPC.ai[3] == 60 || NPC.ai[3] == 180) && Main.netMode != NetmodeID.MultiplayerClient)
                    {
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0), ProjectileType<BlastTelegraph>(), 0, 0, Main.myPlayer, NPC.whoAmI, 0);
                    }

					if (NPC.ai[3] % 120 == 0 && NPC.ai[3] >= 120 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i=0; i<4; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(8f,0).RotatedBy(i * MathHelper.TwoPi / 4f), ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
						}
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 360)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AuraAndBolts;
					}
					break;
				case PolaritiesAttackIDs.AuraAndBolts:
					//magneton does an aura while electris shoots telegraphed lightning bolts

					if ((NPC.ai[3] == 0 || NPC.ai[3] == 120 || NPC.ai[3] == 240) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						float angleRotation = Main.rand.NextFloat(MathHelper.TwoPi);
						for (int i = 0; i < 24; i++)
						{
							float angle = i * MathHelper.TwoPi / 24f;
							float angleDiff = Main.rand.NextFloat(-1f, 1f);

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(24f, 0).RotatedBy(angleRotation + angle + angleDiff), ProjectileType<ElectrisLightningBoltTelegraph>(), Main.expertMode ? 30 : 40, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -angleDiff / 50);
						}
					}
					if (NPC.ai[3] == 180 || NPC.ai[3] == 300 || NPC.ai[3] == 420)
					{
						//SoundEngine.PlaySound(Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/LightningStrike").WithVolume(1f), NPC.Center);
					}

					if (NPC.ai[3] >= 420)
					{
						angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 480)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.SmallVortexAttack;
					}
					break;
				case PolaritiesAttackIDs.SmallVortexAttack:
					//both shoot harmless projectiles towards the player, which hit each other and turn into a vortex that does stuff

					frameLength = 10;

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center)*16f, ProjectileType<ElectrisPreSmallVortexProjectile>(), 0, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 540)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.ProjectilesAndBolts;
					}
					break;
				case PolaritiesAttackIDs.ProjectilesAndBolts:
					//electris does bolts while magneton shoots a line of projectiles

					if ((NPC.ai[3] == 0 || NPC.ai[3] == 240) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						float angleRotation = Main.rand.NextFloat(MathHelper.TwoPi);
						for (int i = 0; i < 24; i++)
						{
							float angle = i * MathHelper.TwoPi / 24f;
							float angleDiff = Main.rand.NextFloat(-1f, 1f);

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(24f, 0).RotatedBy(angleRotation + angle + angleDiff), ProjectileType<ElectrisLightningBoltTelegraph>(), Main.expertMode ? 30 : 40, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -angleDiff / 50);
						}
					}
					if (NPC.ai[3] == 180 || NPC.ai[3] == 420)
					{
						//SoundEngine.PlaySound(Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/LightningStrike").WithVolume(1f), NPC.Center);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 510)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.MagnetonDeathray;
					}
					break;
				case PolaritiesAttackIDs.MagnetonDeathray:
					//electris does an aura while magneton does a deathray
					//the deathray moves faster if the player is magnetized

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<ElectronCloud>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -1);
					}

					if (NPC.ai[3] == 180 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 1; i++)
						{
							float angle = i * MathHelper.TwoPi / 1f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f, 0).RotatedBy(angle), new Vector2(0, 20f).RotatedBy(angle), ProjectileType<ElectrisRing2>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					if (NPC.ai[3] >= 180)
					{
						angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

						if (NPC.ai[3] < 540)
						{
							frameLength = 4;
						}
					}

					if (NPC.ai[3] >= 210 && NPC.ai[3] < 540 && NPC.ai[3] % 5 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.04f);
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 570)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AurasAndRings;
					}
					break;
				case PolaritiesAttackIDs.AurasAndRings:
					//auras and rings
					if (NPC.ai[3] == 20 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center), ProjectileType<ElectrisDeathray>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai1: NPC.whoAmI);
					}
					if (NPC.ai[3] < 520)
					{
						angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

						if (NPC.ai[3] < 120 && NPC.ai[3] >= 20)
						{
							maxTurn = 0;

							frameLength = (int)(4f + 3f * (120 - NPC.ai[3]) / 100f);
						}
						else if (NPC.ai[3] >= 20 && NPC.ai[3] < 500)
						{
							maxTurn = 0.017f * (NPC.ai[3] - 20) * (500 - NPC.ai[3]) / 57600f;

							frameLength = 4;
						}
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 520)
					{
						canPhaseTransition = true;

						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.CircleDeathrays;
					}
					break;
				case PolaritiesAttackIDs.CircleDeathrays:
					//circle deathrays attack

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						int numRays = 4;
						if (playerOffset[2] < 0)
						{
							for (int i = 0; i < numRays; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + i * MathHelper.Pi / numRays), ProjectileType<ElectromagneticDeathray>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, NPC.whoAmI, -1);
							}
						}
						else
						{
							for (int i = 0; i < numRays; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), Main.npc[(int)NPC.ai[0]].Center, new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + i * MathHelper.Pi / numRays), ProjectileType<ElectromagneticDeathray>(), Main.expertMode ? 35 : 50, 0f, Main.myPlayer, NPC.ai[0], -1);
							}
						}
					}

					if (NPC.ai[3] >= 360)
					{
						canPhaseTransition = true;
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 390)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.FastEyeblasts;
					}
					break;
				case PolaritiesAttackIDs.MidToLowTransition:
					//mid to low health transition
					NPC.dontTakeDamage = true;

					angleGoal = NPC.rotation + 0.2f * (NPC.ai[3]) * (240 - NPC.ai[3]) / (120f * 120f);
					frameLength = (int)(7f + 5f * (NPC.ai[3]) * (240 - NPC.ai[3]) / (120f * 120f));

					NPC.ai[3]++;
					if (NPC.ai[3] >= 240)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AnticipatingRays;
					}
					break;
				case PolaritiesAttackIDs.AnticipatingRays:
					if (NPC.ai[3] == 0 || NPC.ai[3] == 240)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center), ProjectileType<ElectrisDeathray>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: 1, ai1: NPC.whoAmI);
					}
					if (NPC.ai[3] % 240 < 120 && NPC.ai[3] < 360)
					{
						angleGoal = NPC.DirectionTo(player.Center + (player.velocity - NPC.velocity) * 60).ToRotation() + MathHelper.PiOver2;

						maxTurn = 0.2f * (120 - (NPC.ai[3] % 240)) / 120f;
					}

					if ((NPC.ai[3] == 180 || NPC.ai[3] == 420) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0), ProjectileType<BlastTelegraph>(), 0, 0, Main.myPlayer, NPC.whoAmI, 0);
					}

					if (NPC.ai[3] == 240 || NPC.ai[3] == 480)
                    {
						for (int i=0; i<4; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(8f,0).RotatedBy(i * MathHelper.TwoPi / 4f), ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.04f);
						}
                    }

					NPC.ai[3]++;
					if (NPC.ai[3] >= 630)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.LargeVortexAttack;
					}
					break;
				case PolaritiesAttackIDs.LargeVortexAttack:
					//both shoot harmless projectiles towards the player, which hit each other and turn into a vortex that does stuff
					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 12f, ProjectileType<ElectrisPreLargeVortexProjectile>(), 0, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
					}

					frameLength = 10;

					NPC.ai[3]++;
					if (NPC.ai[3] >= 840)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.HarderCircleDeathrays;
					}
					break;
				case PolaritiesAttackIDs.HarderCircleDeathrays:
					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (NPC.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						int numRays = 4;
						if (playerOffset[2] < 0)
						{
							for (int i = 0; i < numRays; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + i * MathHelper.Pi / numRays), ProjectileType<ElectromagneticDeathray2>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, NPC.whoAmI, i == 0 ? 1 : 0);
							}
						}
						else
						{
							for (int i = 0; i < numRays; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(),Main.npc[(int)NPC.ai[0]].Center, new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + i * MathHelper.Pi / numRays), ProjectileType<ElectromagneticDeathray2>(), Main.expertMode ? 35 : 50, 0f, Main.myPlayer, NPC.ai[0], i == 0 ? 1 : 0);
							}
						}
					}

					NPC.ai[3]++;
					if (NPC.ai[3] >= 870)
					{
						NPC.ai[3] = 0;
						NPC.ai[2] = (int)PolaritiesAttackIDs.AnticipatingRays;
					}
					break;
			}

			if (canPhaseTransition)
            {
				switch ((PolaritiesAttackIDs)NPC.ai[2])
                {
					case PolaritiesAttackIDs.AlternateEyeblasts:
					case PolaritiesAttackIDs.ShrinkRingsThenEyeblasts:
					case PolaritiesAttackIDs.AuraAttacks:
						float highToMidFraction = Main.expertMode ? 0.875f : 0.8334f;

						if ((NPC.life + Main.npc[(int)NPC.ai[0]].life) < highToMidFraction * (NPC.lifeMax * 2))
						{
							NPC.ai[3] = 0;
							NPC.ai[2] = (int)PolaritiesAttackIDs.HighToMidTransition;
						}
						break;
					case PolaritiesAttackIDs.FastEyeblasts:
					case PolaritiesAttackIDs.CloserShrinkRings:
					case PolaritiesAttackIDs.AuraAndBolts:
					case PolaritiesAttackIDs.SmallVortexAttack:
					case PolaritiesAttackIDs.AurasAndRings:
					case PolaritiesAttackIDs.ProjectilesAndBolts:
					case PolaritiesAttackIDs.MagnetonDeathray:
					case PolaritiesAttackIDs.CircleDeathrays:
						if (Main.expertMode && (NPC.life + Main.npc[(int)NPC.ai[0]].life) < 0.4f * (NPC.lifeMax * 2))
						{
							NPC.ai[3] = 0;
							NPC.ai[2] = (int)PolaritiesAttackIDs.MidToLowTransition;
						}
						break;
                }
			}

			//frames code
			NPC.frameCounter++;
			if (NPC.frameCounter >= frameLength)
			{
				NPC.frameCounter = 0;
			}

			//rotation code//
			float angleOffset = NPC.rotation - angleGoal;
			while (angleOffset > MathHelper.Pi)
			{
				angleOffset -= MathHelper.TwoPi;
			}
			while (angleOffset < -MathHelper.Pi)
			{
				angleOffset += MathHelper.TwoPi;
			}
			if (Math.Abs(angleOffset) < maxTurn)
			{
				NPC.rotation = angleGoal;
			}
			else if (angleOffset < 0)
			{
				NPC.rotation += maxTurn;
			}
			else
			{
				NPC.rotation -= maxTurn;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.frameCounter == 0)
			{
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
			}
		}

		public override bool CheckActive()
		{
			return Main.player[NPC.target].dead;
		}

		public override void BossLoot(ref int potionType)
		{
			potionType = ItemID.SuperHealingPotion;
		}

		public override bool CheckDead()
		{
			if (Main.npc[(int)NPC.ai[0]].ai[1] == 0)
			{
				NPC.dontTakeDamage = true;
				NPC.ai[1] = 1;
				NPC.life = 1;
				return false;
			}
			else if (Main.npc[(int)NPC.ai[0]].ai[1] == 1)
			{
				NPC.dontTakeDamage = true;
				NPC.ai[1] = 2;
				NPC.damage = 0;
				NPC.life = 1;

				NPC.ai[3] = 0;

				Main.npc[(int)NPC.ai[0]].damage = 0;
				Main.npc[(int)NPC.ai[0]].ai[1] = 2;
				Main.npc[(int)NPC.ai[0]].ai[3] = 0;
				return false;
			}

			return true;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mask = TextureAssets.Npc[NPC.type].Value;
			Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, (mask.Height / 4) * 0.5f);
			Vector2 drawPos = NPC.Center - Main.screenPosition;
			float alpha = (255 - NPC.alpha) / 255f;
			spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White * alpha, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

			return false;
		}

        public override bool PreKill()
		{
			return true;
		}
	}

	[AutoloadBossHead]
	public class Magneton : ModNPC
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magneton");
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 64;
			NPC.height = 64;
			DrawOffsetY = 30;

			NPC.defense = 60;
			NPC.lifeMax = 120000;
			if (Main.expertMode)
			{
				NPC.lifeMax = 180000;
			}
			NPC.damage = 120;
			if (Main.expertMode)
			{
				NPC.damage = 80;
			}

			NPC.knockBackResist = 0f;
			NPC.npcSlots = 15f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;

			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.OnFire] = true;

			Music = MusicID.Boss3;
		}

		//public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
		//{
			//NPC.lifeMax = (int)(180000 * bossLifeScale);
		//}

		private float[] playerOffset = { 600f, 0f, 0f };

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<Items.Vanity.Hardmode.MagnetonMask>(), 7));
			//TODO: npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<EsophagePetItem>(), 4));

			//normal mode loot
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Weapons.Melee.Misc.EMP>(), 3));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Weapons.Ranged.Bows.Hardmode.ArcBolter>(), 3));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Weapons.Summon.Minions.Hardmode.DipoleStaff>(), 3));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Placeable.Bars.PolarizedBar>(), 1, 15, 29));

			npcLoot.Add(notExpertRule);

			//npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Contagun>()));
		}

		public override void BossHeadRotation(ref float rotation)
		{
			rotation = NPC.rotation - MathHelper.PiOver2;
		}

		public override void AI()
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead)
				{
					if (NPC.timeLeft > 10)
					{
						NPC.timeLeft = 10;
					}
					NPC.velocity = (NPC.Center - player.Center) / 60;

					//frames code
					NPC.frameCounter++;
					if (NPC.frameCounter >= 7)
					{
						NPC.frameCounter = 0;
					}
					return;
				}
			}

            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.rotation = MathHelper.PiOver2;

                Vector2 spawnPosition = player.MountedCenter * 2 - NPC.Center;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    //NPC.ai[0] = NPC.NewNPC(new EntitySource_BossSpawn(), spawnPosition.X, spawnPosition.Y, NPCType<Electris>(), ai0: NPC.whoAmI);
                    //NPC.ai[0] = Main.npc[NPC.NewNPC(new EntitySource_BossSpawn(), spawnPosition.X, spawnPosition.Y, NPCType<Electris>(), ai0: NPC.whoAmI)];
                }
                NPC.netUpdate = true;
            }

            NPC electris = Main.npc[(int)NPC.ai[0]];

			if (NPC.ai[1] == 2)
			{
				NPC.velocity = Vector2.Zero;

				if (NPC.ai[3] == 0)
				{
					SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);

					//Gore.NewGore(NPC.Center, (NPC.Center - Main.npc[(int)NPC.ai[0]].Center).SafeNormalize(Vector2.Zero) * 8, Mod.GetGoreSlot("Gores/MagnetonGore"));

					for (int i=0; i<64; i++)
					{
						Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.NextFloat(NPC.width / 2), 0).RotatedBy(i * MathHelper.TwoPi / 64f), 235, Vector2.Zero, newColor: Color.Red, Scale: 2f);
					}
				}
				Dust.NewDustPerfect(NPC.Center, 235, Vector2.Zero, newColor: Color.Red ,Scale: 2f);

				NPC.hide = true;

				Vector2 goalCenter = (NPC.Center + Main.npc[(int)NPC.ai[0]].Center) / 2f;
				float rotation = 10f / (float)Math.Pow(480 - NPC.ai[3], 1.5f);
				Vector2 goalPosition = goalCenter + (NPC.Center - goalCenter).SafeNormalize(Vector2.Zero).RotatedBy(rotation) * ((NPC.Center - goalCenter).Length() * (float)Math.Sqrt((479f - NPC.ai[3]) / (480f - NPC.ai[3])));
				NPC.Center = goalPosition; //- npc.Center;

				NPC.ai[3]++;
				if (NPC.ai[3] >= 480)
				{
					NPC.life = 0;
					NPC.checkDead();
				}
				return;
			}

			//motion code
			Vector2 playerVelocity = player.position - player.oldPosition;
			float[] newPlayerOffset = {
				playerOffset[0] + playerVelocity.X * playerOffset[2] / 600f
				,
				playerOffset[1] + playerVelocity.Y * playerOffset[2] / 600f
				,
				playerOffset[2] - playerVelocity.X * playerOffset[0] / 600f - playerVelocity.Y * playerOffset[1] / 600f
			};

			float offsetDist = (float)Math.Sqrt(newPlayerOffset[0] * newPlayerOffset[0] + newPlayerOffset[1] * newPlayerOffset[1] + newPlayerOffset[2] * newPlayerOffset[2]);
			newPlayerOffset[0] *= 600f / offsetDist;
			newPlayerOffset[1] *= 600f / offsetDist;
			newPlayerOffset[2] *= 600f / offsetDist;

			playerOffset = newPlayerOffset;

			NPC.velocity = new Vector2(playerOffset[0], playerOffset[1]) + player.Center - NPC.Center;

			if (NPC.ai[1] == 0)
			{
				NPC.dontTakeDamage = false;
			}

			float angleGoal = NPC.rotation;
			float maxTurn = 0.2f;
			int frameLength = 7;
			NPC.scale = 1;
			NPC.alpha = 0;

			//attack code

			switch ((PolaritiesAttackIDs)electris.ai[2])
			{
				case PolaritiesAttackIDs.SpawnTransition:
					NPC.dontTakeDamage = true;

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					NPC.scale = (electris.ai[3] + 1) / 240f;
					NPC.alpha = (int)(255 - 255 * ((electris.ai[3] + 1) / 240f));
					frameLength = (int)(4f + 3f * (electris.ai[3] + 1) / 240f);

					if (NPC.ai[3] % 20 == 0)
					{
						SoundEngine.PlaySound(SoundID.Item15, NPC.Center);
					}

					break;
				case PolaritiesAttackIDs.AlternateEyeblasts:
					//alternating eyeblasts

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (electris.ai[3] % 60 == 30 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 6f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
					}

					break;
				case PolaritiesAttackIDs.ShrinkRingsThenEyeblasts:

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (electris.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 24; i++)
						{
							float angle = i * MathHelper.TwoPi / 24f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f,0).RotatedBy(angle), Vector2.Zero, ProjectileType<MagnetonRing>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					if (electris.ai[3] > 120 && electris.ai[3] % 60 == 30 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 6f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.05f, ai1: player.whoAmI);
					}
					break;
				case PolaritiesAttackIDs.AuraAttacks:
					if (electris.ai[3] == 30 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 16; i++)
						{
							int direction = 1;

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(4.11f,0).RotatedBy(i * MathHelper.TwoPi / 16f), ProjectileType<MagnetonAura>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: direction);
						}
					}
					break;
				case PolaritiesAttackIDs.HighToMidTransition:
					NPC.dontTakeDamage = true;

					angleGoal = NPC.rotation + 0.2f * (electris.ai[3]) * (240 - electris.ai[3]) / (120f * 120f);
					frameLength = (int)(7f + 5f * (electris.ai[3]) * (240 - electris.ai[3]) / (120f * 120f));

					break;
				case PolaritiesAttackIDs.FastEyeblasts:

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if (electris.ai[3] % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(1f) * 6f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(-1f) * 6f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
					}
					break;
				case PolaritiesAttackIDs.CloserShrinkRings:

					if (electris.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 24; i++)
						{
							float angle = i * MathHelper.TwoPi / 24f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f, 0).RotatedBy(angle), Vector2.Zero, ProjectileType<MagnetonRing2>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					if ((electris.ai[3] == 120 || electris.ai[3] == 240) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(MathHelper.PiOver4), ProjectileType<BlastTelegraph>(), 0, 0, Main.myPlayer, NPC.whoAmI, 1);
					}

					if (electris.ai[3] % 120 == 60 && electris.ai[3] >= 120 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 4; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(8f, 0).RotatedBy(i * MathHelper.TwoPi / 4f + MathHelper.PiOver4), ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.05f, ai1: player.whoAmI);
						}
					}
					break;
				case PolaritiesAttackIDs.AuraAndBolts:
					if ((electris.ai[3] == 0 || electris.ai[3] == 120 || electris.ai[3] == 240) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 20; i++)
						{
							int direction = (electris.ai[3] == 120) ? -1 : 1;

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(4.5f, 0).RotatedBy(i * MathHelper.TwoPi / 20f), ProjectileType<MagnetonAura>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: direction);
						}
					}

					if (electris.ai[3] >= 420)
					{
						angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
					}
					break;
				case PolaritiesAttackIDs.SmallVortexAttack:
					if (electris.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 16f, ProjectileType<MagnetonPreSmallVortexProjectile>(), 0, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
					}

					frameLength = 10;
					break;
				case PolaritiesAttackIDs.ProjectilesAndBolts:
					//electris does bolts while magneton shoots a line of projectiles

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

					if ((electris.ai[3] == 180 || electris.ai[3] == 420) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 12; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 8f * (float)Math.Pow(1.02, -i * 10f), ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
						}
					}
					break;
				case PolaritiesAttackIDs.MagnetonDeathray:
					//electris does an aura while magneton does a deathray
					//the deathray moves faster if the player is magnetized

					if (electris.ai[3] == 60 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center), ProjectileType<MagnetonDeathray>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai1: NPC.whoAmI);
					}
					if (electris.ai[3] < 180)
                    {
						angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

						maxTurn = 0.015f * (180 - electris.ai[3]) / 120f;

						frameLength = (int)(4f + 3f * (180 - electris.ai[3]) / 180f);
					}
					else
                    {
						frameLength = 4;
					}

					break;
				case PolaritiesAttackIDs.AurasAndRings:

					if (electris.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 16; i++)
						{
							int direction = -1;

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(2.2f, 0).RotatedBy(i * MathHelper.TwoPi / 16f), ProjectileType<MagnetonAura2>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: direction);
						}

						for (int i = 0; i < 24; i++)
						{
							float angle = i * MathHelper.TwoPi / 24f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(1200f, 0).RotatedBy(angle), Vector2.Zero, ProjectileType<MagnetonRing>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: angle);
						}
					}

					break;
				case PolaritiesAttackIDs.CircleDeathrays:

					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
					break;

				case PolaritiesAttackIDs.MidToLowTransition:
					NPC.dontTakeDamage = true;

					angleGoal = NPC.rotation + 0.2f * (electris.ai[3]) * (240 - electris.ai[3]) / (120f * 120f);
					frameLength = (int)(7f + 5f * (electris.ai[3]) * (240 - electris.ai[3]) / (120f * 120f));

					break;

				case PolaritiesAttackIDs.AnticipatingRays:
					if (electris.ai[3] == 120 || electris.ai[3] == 360)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center), ProjectileType<MagnetonDeathray>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: 1, ai1: NPC.whoAmI);
					}
					if (electris.ai[3] % 240 >= 120 && electris.ai[3] < 480)
					{
						angleGoal = NPC.DirectionTo(player.Center + (player.velocity - NPC.velocity) * 60).ToRotation() + MathHelper.PiOver2;

						maxTurn = 0.2f * (120 - ((electris.ai[3] + 120) % 240)) / 120f;
					}

					if ((electris.ai[3] == 60 || electris.ai[3] == 300) && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1,0), ProjectileType<BlastTelegraph>(), 0, 0, Main.myPlayer, NPC.whoAmI, 1);
					}

					if (electris.ai[3] == 120 || electris.ai[3] == 360)
					{
						for (int i = 0; i < 4; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(8f, 0).RotatedBy(i * MathHelper.TwoPi / 4f), ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.05f, ai1: player.whoAmI);
						}
					}
					break;
				case PolaritiesAttackIDs.LargeVortexAttack:
					if (electris.ai[3] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center) * 12f, ProjectileType<MagnetonPreLargeVortexProjectile>(), 0, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
					}

					frameLength = 10;

					break;
				case PolaritiesAttackIDs.HarderCircleDeathrays:
					angleGoal = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
					break;
			}

			//frames code
			NPC.frameCounter++;
			if (NPC.frameCounter >= frameLength)
			{
				NPC.frameCounter = 0;
			}

			//rotation code
			float angleOffset = NPC.rotation - angleGoal;
			while (angleOffset > MathHelper.Pi)
            {
				angleOffset -= MathHelper.TwoPi;
			}
			while (angleOffset < -MathHelper.Pi)
			{
				angleOffset += MathHelper.TwoPi;
			}
			if (Math.Abs(angleOffset) < maxTurn)
            {
				NPC.rotation = angleGoal;
            }
			else if (angleOffset < 0)
            {
				NPC.rotation += maxTurn;
			}
			else
			{
				NPC.rotation -= maxTurn;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.frameCounter == 0)
			{
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
			}
		}

		public override bool CheckActive()
		{
			return Main.player[NPC.target].dead;
		}

		public override void BossLoot(ref int potionType)
		{
			potionType = ItemID.SuperHealingPotion;
		}

		public override bool CheckDead()
		{
			if (Main.npc[(int)NPC.ai[0]].ai[1] == 0)
			{
				NPC.dontTakeDamage = true;
				NPC.ai[1] = 1;
				NPC.life = 1;
				return false;
			}
			else if (Main.npc[(int)NPC.ai[0]].ai[1] == 1)
			{
				NPC.dontTakeDamage = true;
				NPC.ai[1] = 2;
				NPC.damage = 0;
				NPC.life = 1;

				NPC.ai[3] = 0;

				Main.npc[(int)NPC.ai[0]].damage = 0;
				Main.npc[(int)NPC.ai[0]].ai[1] = 2;
				Main.npc[(int)NPC.ai[0]].ai[3] = 0;
				return false;
			}

			return true;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D mask = TextureAssets.Npc[NPC.type].Value;
            Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, (mask.Height / 4) * 0.5f);
            Vector2 drawPos = NPC.Center - Main.screenPosition;
            float alpha = (255 - NPC.alpha) / 255f;
            spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White * alpha, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            return false;
        }

        public override bool PreKill()
		{
			return true;
		}
	}

	public class ElectricBlast : ModProjectile
	{
        public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

        public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Eyeblast");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 36;
			Projectile.height = 36;
			DrawOffsetX = -22;
			DrawOriginOffsetY = -6;
			DrawOriginOffsetX = 8;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
            {
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
			}

			Projectile.localAI[1] += 0.05f;
			while (Projectile.localAI[1] > MathHelper.TwoPi / 11f)
			{
				Projectile.localAI[1] -= MathHelper.TwoPi / 11f;
			}

			if (Projectile.timeLeft < 10)
            {
				Projectile.scale -= 0.1f;
				Projectile.hostile = false;
            }

			//Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
			Projectile.rotation = Projectile.velocity.ToRotation();

			Projectile.velocity *= Projectile.ai[0];
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
		}

        private static Asset<Texture2D> texture;

        public override void Load()
        {
            
        }

        public override void Unload()
        {
            texture = null;
        }

        public override bool PreDraw(ref Color lightColor)
		{
			//Texture2D texture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_644");//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];
			texture = TextureAssets.Projectile[644];
            Color mainColor = new Color(181, 248, 255);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				float length;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					length = (Projectile.position - Projectile.oldPos[k]).Length();
					rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}
				else
				{
					length = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).Length();
					rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				//Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale,1), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(82 * scale + 512 * (1 - scale)), (int)(179 * scale + 512 * (1 - scale)), (int)(203 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagneticBlast : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magnetic Eyeblast");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 36;
			Projectile.height = 36;
			DrawOffsetX = -22;
			DrawOriginOffsetY = -6;
			DrawOriginOffsetX = 8;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
			}

			Projectile.localAI[1] += 0.05f;
			while (Projectile.localAI[1] > MathHelper.TwoPi / 11f)
            {
				Projectile.localAI[1] -= MathHelper.TwoPi / 11f;
			}

			if (Projectile.timeLeft < 10)
			{
				Projectile.scale -= 0.1f;
				Projectile.hostile = false;
			}

			//Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, 235, newColor: Color.Red, Scale: 2f)].noGravity = true;
			Projectile.rotation = Projectile.velocity.ToRotation();

			//ai[0] is homing charge
			//ai[1] is player being targeted
			Player player = Main.player[(int)Projectile.ai[1]];

			if (Projectile.ai[0] > 0)
            {
				float angleOffset = Projectile.DirectionTo(player.Center).ToRotation() - Projectile.velocity.ToRotation();

				while (angleOffset > MathHelper.Pi)
                {
					angleOffset -= MathHelper.TwoPi;
				}
				while (angleOffset < -MathHelper.Pi)
				{
					angleOffset += MathHelper.TwoPi;
				}

				float maxTurn = /*0.5f*/ Projectile.ai[0] / Projectile.velocity.Length();

				//if (player.GetModPlayer<PolaritiesPlayer>().magnetized)
                //{
					//maxTurn *= 2;
                //}

				if (Math.Abs(angleOffset) < maxTurn)
				{
					Projectile.velocity = Projectile.DirectionTo(player.Center) * Projectile.velocity.Length();
                }
				else if (angleOffset > 0)
                {
					Projectile.velocity = Projectile.velocity.RotatedBy(maxTurn);
				}
				else
				{
					Projectile.velocity = Projectile.velocity.RotatedBy(-maxTurn);
				}

				//projectile.ai[0]--;
			}

			Projectile.velocity *= 1.02f;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
			//projectile.ai[0] = 0;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[644].Value;//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];

			Color mainColor = new Color(255, 181, 181);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}
				else
				{
					rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, 1), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(203 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectronCloud : ModProjectile
	{
        public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electron Cloud");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 758, 758, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					float twistyFactor = (float)(1 + Math.Cos(8 * theta + 8 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor = distanceSquared * (twistyFactor - 1) + 1;

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)) * scaleFactor);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ElectronCloud.png", FileMode.Create), texture.Width, texture.Height);
			*/
			//Main.projectileTexture[projectile.type] = texture;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.alpha = 255;
			Projectile.timeLeft = 260;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];
			Projectile.rotation += Projectile.ai[1] * 0.1f / Projectile.scale;

			Projectile.scale = (float)(0.1f + Math.Sqrt(1 - ((Projectile.timeLeft - 130f) * (Projectile.timeLeft - 130f)) / (130f * 130f)));
			Projectile.alpha = (int)(128 * (1.5f - Math.Sqrt(1 - ((Projectile.timeLeft - 130f) * (Projectile.timeLeft - 130f)) / (130f * 130f))));

			Projectile.light = 5f * Projectile.scale;

			Projectile.width = (int)(640 * Projectile.scale);
			Projectile.height = (int)(640 * Projectile.scale);
			DrawOffsetX = (int)(0.5f * (Projectile.height - 758));
			DrawOriginOffsetY = (int)(0.5f * (Projectile.height - 758));
			DrawOriginOffsetX = 0;
			Projectile.position = electris.Center - Projectile.Hitbox.Size() / 2;
			if (!electris.active || electris.ai[1] == 2) { Projectile.Kill(); }

			if (Projectile.timeLeft % 20 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
			}
		}

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
			target.noKnockback = true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

        public override bool PreDraw(ref Color lightColor)
        {
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 758, 758), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(379, 379), Projectile.scale, SpriteEffects.None, 0f);
			return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonAura : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magneton Orbiter");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 64, 64, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					float twistyFactor = (float)(1 + Math.Cos(8 * theta + 8 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor = distanceSquared * (twistyFactor - 1) + 1;

					float twistyFactor2 = (float)(1 + Math.Cos(MathHelper.Pi + 8 * theta + 8 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor2 = distanceSquared * (twistyFactor2 - 1) + 1;

					int r = 255;
					int g = (int)(128 + 64 * scaleFactor2);
					int b = (int)(128 + 64 * scaleFactor2);
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)) * scaleFactor);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			//texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "MagnetonAura.png", FileMode.Create), texture.Width, texture.Height);

			if (Main.netMode != 2)
				Main.projectileTexture[projectile.type] = texture;*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 54;
			Projectile.height = 54;
			DrawOffsetX = -5;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = -5;

			Projectile.alpha = 0;
			Projectile.timeLeft = 260;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC magneton = Main.npc[(int)Projectile.ai[0]];
			Projectile.rotation += 0.1f * Projectile.ai[1];

			Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.TwoPi / 260f);

			Projectile.position += magneton.velocity;
			if (!magneton.active || magneton.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(32, 32), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonAura2 : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/MagnetonAura";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magneton Orbiter");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 54;
			Projectile.height = 54;
			DrawOffsetX = -5;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = -5;

			Projectile.alpha = 0;
			Projectile.timeLeft = 400;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC magneton = Main.npc[(int)Projectile.ai[0]];
			Projectile.rotation += 0.1f * Projectile.ai[1];

			Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.TwoPi / 400f);

			Projectile.position += magneton.velocity;
			if (!magneton.active || magneton.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(32, 32), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonRing : ModProjectile
	{
        public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/MagnetonAura";

        public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magneton Orbiter");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 54;
			Projectile.height = 54;
			DrawOffsetX = -5;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = -5;

			Projectile.alpha = 0;
			Projectile.timeLeft = 400;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC magneton = Main.npc[(int)Projectile.ai[0]];
			Projectile.rotation -= 0.1f;

			float angle = Projectile.ai[1];

			//radius needs to be 1200 at the start and end and 450 in between
			float radius = (Projectile.timeLeft - 200) * (Projectile.timeLeft - 200) / (40000f / (1200f - 450f)) + 450f;

			Projectile.ai[1] += 4f / (float)radius;

			Vector2 goalPosition = magneton.Center + new Vector2(radius, 0).RotatedBy(angle);
			Projectile.velocity = goalPosition - Projectile.Center;

			Projectile.position += magneton.velocity;
			if (!magneton.active || magneton.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(32, 32), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonRing2 : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/MagnetonAura";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magneton Orbiter");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 54;
			Projectile.height = 54;
			DrawOffsetX = -5;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = -5;

			Projectile.alpha = 0;
			Projectile.timeLeft = 400;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC magneton = Main.npc[(int)Projectile.ai[0]];
			Projectile.rotation -= 0.1f;

			float angle = Projectile.ai[1];

			//radius needs to be 1200 at the start and end and 400 in between
			float radius = (Projectile.timeLeft - 200) * (Projectile.timeLeft - 200) / (40000f / (1200f - 400f)) + 400f;

			Projectile.ai[1] += 4f / (float)radius;

			Vector2 goalPosition = magneton.Center + new Vector2(radius, 0).RotatedBy(angle);
			Projectile.velocity = goalPosition - Projectile.Center;

			Projectile.position += magneton.velocity;
			if (!magneton.active || magneton.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(32, 32), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectrisRing : ModProjectile
	{
		public override string Texture => "Polarities/Content/Projectiles/CallShootProjectile";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Ring");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 768;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 400 * 64;
			Projectile.extraUpdates = 63;

			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];

			float angle = Projectile.ai[1];

			//radius needs to be 1200 at the start and end and 450 in between
			float radius = (Projectile.timeLeft / 64 - 200) * (Projectile.timeLeft / 64 - 200) / (40000f / (1200f - 450f)) + 450f;

			Projectile.ai[1] = (Projectile.Center-electris.Center).ToRotation() + 100f / (float)radius;


			Vector2 goalPoint = electris.Center + new Vector2(radius, 0).RotatedBy(angle);

			float rotation = (goalPoint - Projectile.Center).ToRotation() + Main.rand.NextFloat(-1, 1);

			//don't do random variation if close enough
			if ((goalPoint - Projectile.Center).Length() < Projectile.velocity.Length() * 2)
			{
				rotation = (goalPoint - Projectile.Center).ToRotation();
			}

			Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(rotation);

			if (Projectile.position.X + Projectile.velocity.X < 0 || Projectile.position.X + Projectile.velocity.X > Main.maxTilesX * 16)
            {
				Projectile.position.X -= Projectile.velocity.X;
				Projectile.ai[1] += 1f;
			}
			if (Projectile.position.Y + Projectile.velocity.Y < 0 || Projectile.position.Y + Projectile.velocity.Y > Main.maxTilesY * 16)
			{
				Projectile.position.Y -= Projectile.velocity.Y;
				Projectile.ai[1] += 1f;
			}

			/*for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(projectile.Center + i * projectile.velocity / 5, DustID.Electric, Velocity: Vector2.Zero, Scale: 0.5f).noGravity = true;
			}*/

			if (!electris.active || electris.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Color mainColor = new Color(181, 248, 255);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = 1.5f * Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation();
				}
				else
				{
					rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(0, 1), new Vector2(10, scale), SpriteEffects.None, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectrisRing2 : ModProjectile
	{
		public override string Texture => "Polarities/Content/Projectiles/CallShootProjectile";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Ring");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 768;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 400 * 64;
			Projectile.extraUpdates = 63;

			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];

			float angle = Projectile.ai[1];

			//radius needs to be 1200 at the start and end and 400 in between
			float radius = (Projectile.timeLeft / 64 - 200) * (Projectile.timeLeft / 64 - 200) / (40000f / (1200f - 400f)) + 400f;

			Projectile.ai[1] = (Projectile.Center - electris.Center).ToRotation() + 100f / (float)radius;


			Vector2 goalPoint = electris.Center + new Vector2(radius, 0).RotatedBy(angle);

			float rotation = (goalPoint - Projectile.Center).ToRotation() + Main.rand.NextFloat(-1, 1);

			//don't do random variation if close enough
			if ((goalPoint - Projectile.Center).Length() < Projectile.velocity.Length() * 2)
			{
				rotation = (goalPoint - Projectile.Center).ToRotation();
			}

			Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(rotation);

			/*for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(projectile.Center + i * projectile.velocity / 5, DustID.Electric, Velocity: Vector2.Zero, Scale: 0.5f).noGravity = true;
			}*/

			if (!electris.active || electris.ai[1] == 2) { Projectile.Kill(); }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Color mainColor = new Color(181, 248, 255);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = 1.5f * Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation();
				}
				else
				{
					rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(0, 1), new Vector2(10, scale), SpriteEffects.None, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
	
	public class ElectrisLightningBoltTelegraph : ModProjectile
	{
		public override string Texture => "Polarities/Content/Projectiles/CallShootProjectile";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Lightning Beam");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 180;

			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 0f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];

			Projectile.Center = electris.Center;

			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]);
			Projectile.ai[1] *= 0.98f;

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (!electris.active || electris.ai[1] == 2) { Projectile.active = false; }
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			if (Projectile.timeLeft > 1) return false;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + 10000f * Projectile.velocity);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(20f, 0).RotatedBy(Projectile.rotation), ProjectileType<ElectrisLightningBolt>(), 0, 0f, Main.myPlayer, ai0: Projectile.ai[0], ai1: Projectile.rotation);
			}
        }

        public override bool PreDraw(ref Color lightColor)
        {
			float alpha = 0.5f * (180 - Projectile.timeLeft) / 180f;

			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 2), Color.White * alpha, Projectile.rotation, new Vector2(0, 1), new Vector2(2400, 1), SpriteEffects.None, 0f);

			return false;
        }

        public override bool ShouldUpdatePosition()
        {
			return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectrisLightningBolt : ModProjectile
	{
		public override string Texture => "Polarities/Content/Projectiles/CallShootProjectile";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Ring");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 384;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 1024;
			Projectile.extraUpdates = 15;

			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];

			float angle = Projectile.ai[1];

			float radius = (Projectile.Center - electris.Center).Length() + 40f;

			Vector2 goalPoint = electris.Center + new Vector2(radius, 0).RotatedBy(angle);

			float rotation = (goalPoint - Projectile.Center).ToRotation() + Main.rand.NextFloat(-1, 1);

			//don't do random variation if close enough
			if ((goalPoint - Projectile.Center).Length() < Projectile.velocity.Length())
			{
				rotation = (goalPoint - Projectile.Center).ToRotation();
			}

			Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(rotation);

			/*for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(projectile.Center + i * projectile.velocity / 5, DustID.Electric, Velocity: Vector2.Zero, Scale: 0.5f).noGravity = true;
			}*/
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Color mainColor = new Color(181, 248, 255);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = 1.5f * Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation();
				}
				else
				{
					rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(0, 1), new Vector2(10, scale), SpriteEffects.None, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectrisPreSmallVortexProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Shot");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.scale = 0.25f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
			}

			Vector2 goalPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Projectile.velocity = Projectile.DirectionTo(goalPosition) * Projectile.velocity.Length();

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Distance(goalPosition) < Projectile.velocity.Length() && Projectile.timeLeft > 20)
            {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), goalPosition, Vector2.Zero, ProjectileType<SmallEMVortex>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer);

				Projectile.Center = goalPosition;
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 20;
            }
		}

		public override bool PreDraw(ref Color lightColor)
		{
            Texture2D texture = TextureAssets.Projectile[644].Value;//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];

            Color mainColor = new Color(181, 248, 255);

			for (int k = 1; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation = 0;
				if ((Projectile.oldPos[k - 1] - Projectile.oldPos[k]).Length() > 1)
				{
					rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale * 4), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(82 * scale + 512 * (1 - scale)), (int)(179 * scale + 512 * (1 - scale)), (int)(203 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = Projectile.scale * new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonPreSmallVortexProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magnetic Shot");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.scale = 0.25f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
			}

			Vector2 goalPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Projectile.velocity = Projectile.DirectionTo(goalPosition) * Projectile.velocity.Length();

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Distance(goalPosition) < Projectile.velocity.Length() && Projectile.timeLeft > 20)
			{
				Projectile.Center = goalPosition;
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 20;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[644].Value;//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];

			Color mainColor = new Color(255, 181, 181);

			for (int k = 1; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation = 0;
				if ((Projectile.oldPos[k - 1] - Projectile.oldPos[k]).Length() > 1)
				{
					rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale * 4), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(203 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = Projectile.scale * new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class ElectrisPreLargeVortexProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Shot");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.scale = 0.5f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
			}

			Vector2 goalPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Projectile.velocity = Projectile.DirectionTo(goalPosition) * Projectile.velocity.Length();

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Distance(goalPosition) < Projectile.velocity.Length() && Projectile.timeLeft > 20)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), goalPosition, Vector2.Zero, ProjectileType<LargeEMVortex>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer);

				Projectile.Center = goalPosition;
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 20;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
            Texture2D texture = TextureAssets.Projectile[644].Value;//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];

            Color mainColor = new Color(181, 248, 255);

			for (int k = 1; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation = 0;
				if ((Projectile.oldPos[k - 1] - Projectile.oldPos[k]).Length() > 1)
				{
					rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale * 2), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(82 * scale + 512 * (1 - scale)), (int)(179 * scale + 512 * (1 - scale)), (int)(203 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = Projectile.scale * new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

	public class MagnetonPreLargeVortexProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/EyeBlast";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magnetic Shot");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;

			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 360;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.scale = 0.5f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
			}

			Vector2 goalPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Projectile.velocity = Projectile.DirectionTo(goalPosition) * Projectile.velocity.Length();

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.Distance(goalPosition) < Projectile.velocity.Length() && Projectile.timeLeft > 20)
			{
				Projectile.Center = goalPosition;
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 20;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
            Texture2D texture = TextureAssets.Projectile[644].Value;//Main.projectileTexture[projectile.type];//Main.projectileTexture[ProjectileID.RainbowCrystalExplosion];

            Color mainColor = new Color(255, 181, 181);

			for (int k = 1; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation = 0;
				if ((Projectile.oldPos[k - 1] - Projectile.oldPos[k]).Length() > 1)
				{
					rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
				}

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale * 2), SpriteEffects.None, 0f);
			}


			int numDraws = 11;
			for (int i = 0; i < numDraws; i++)
			{
				float scale = 0.5f + (1 + (float)Math.Sin(Projectile.localAI[1] + (MathHelper.TwoPi * i) / numDraws)) / 4f;
				Color color = new Color((int)(203 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)), (int)(82 * scale + 512 * (1 - scale)));
				float alpha = 0.3f;
				float rotation = Projectile.rotation;
				Vector2 positionOffset = Projectile.scale * new Vector2(4, 0).RotatedBy(6 * i * MathHelper.TwoPi / numDraws + Projectile.localAI[1] * 2);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + positionOffset - Main.screenPosition, new Rectangle(0, 0, 64, 48), color * alpha, rotation, new Vector2(40, 24), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }


	public class SmallEMVortex : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electromagnetic Vortex");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 758, 758, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					float twistyFactor = (float)(1 + Math.Cos(8 * theta + 8 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor = distanceSquared * (twistyFactor - 1) + 1;

					float twistyFactor2 = (float)(1 + Math.Cos(4 * theta + 4 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor2 = distanceSquared * (twistyFactor2 - 1) + 1;

					float twistyFactor3 = (float)(1 + Math.Cos(4 * (theta + MathHelper.TwoPi / 8) + 4 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor3 = distanceSquared * (twistyFactor3 - 1) + 1;

					int r = distanceSquared >= 1 ? 128 : (int)(128 + 128 * scaleFactor2);
					int g = distanceSquared >= 1 ? 0 : (int)(128 * scaleFactor2 + 128 * scaleFactor3);
					int b = distanceSquared >= 1 ? 128 : (int)(128 + 128 * scaleFactor3);
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)) * scaleFactor);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			//texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "Vortex2.png", FileMode.Create), texture.Width, texture.Height);

			if (Main.netMode != 2)
				Main.projectileTexture[projectile.type] = texture;*/
		}

		private const int LIFETIME = 480;

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.alpha = 255;
			Projectile.timeLeft = LIFETIME;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
            {
				Projectile.localAI[0] = 1;

				for (int i=0; i<Main.maxNPCs; i++)
                {
					if (Main.npc[i].type == NPCType<Electris>() && Main.npc[i].active)
                    {
						Projectile.ai[0] = i;
						break;
					}
                }
			}

			Projectile.rotation -= 0.0707f / Projectile.scale;

			if (!Main.npc[(int)Projectile.ai[0]].active || Main.npc[(int)Projectile.ai[0]].ai[1] == 2)
			{
				Projectile.hostile = false;
				Projectile.alpha += 16;
				if (Projectile.alpha > 255)
				{
					Projectile.Kill();
				}
				return;
			}

			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = (float)(0.1f + Math.Sqrt(1 - ((Projectile.timeLeft - (float)(LIFETIME/2)) * (Projectile.timeLeft - (float)(LIFETIME / 2))) / ((float)(LIFETIME / 2) * (float)(LIFETIME / 2))));
			Projectile.alpha = (int)(192 * (1f - Math.Sqrt(1 - ((Projectile.timeLeft - (float)(LIFETIME / 2)) * (Projectile.timeLeft - (float)(LIFETIME / 2))) / ((float)(LIFETIME / 2) * (float)(LIFETIME / 2)))));

			Projectile.light = 5f * Projectile.scale;

			Projectile.width = (int)(640 * Projectile.scale);
			Projectile.height = (int)(640 * Projectile.scale);
			DrawOffsetX = (int)(0.5f * (Projectile.height - 758));
			DrawOriginOffsetY = (int)(0.5f * (Projectile.height - 758));
			DrawOriginOffsetX = 0;

			Projectile.Center = oldCenter;


			Player player = Main.player[Main.npc[(int)Projectile.ai[0]].target];

			Projectile.velocity = (player.Center - Projectile.Center) / 160f;

			if (Projectile.scale > 0.25f)
            {
				Projectile.hostile = true;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					if (Projectile.timeLeft % 60 == 0)
					{
						for (int i = 0; i < 12; i++)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6f, 0).RotatedBy(i * MathHelper.TwoPi / 12), ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
						}
					}
					if (Projectile.timeLeft % 15 == 0)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(player.Center) * 4f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
					}
				}
            }
			else
            {
				Projectile.hostile = false;
			}

			if (Projectile.timeLeft % 20 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 758, 758), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(379, 379), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }


	public class LargeEMVortex : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electromagnetic Vortex");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 1072, 1072, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					float twistyFactor = (float)(1 + Math.Cos(12 * theta + 12 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor = distanceSquared * (twistyFactor - 1) + 1;

					float twistyFactor2 = (float)(1 + Math.Cos(6 * theta + 6 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor2 = distanceSquared * (twistyFactor2 - 1) + 1;

					float twistyFactor3 = (float)(1 + Math.Cos(6 * (theta + MathHelper.TwoPi / 12) + 6 * Math.Sqrt(distanceSquared))) / 2;
					float scaleFactor3 = distanceSquared * (twistyFactor3 - 1) + 1;

					int r = distanceSquared >= 1 ? 128 : (int)(128 + 128 * scaleFactor2);
					int g = distanceSquared >= 1 ? 0 : (int)(128 * scaleFactor2 + 128 * scaleFactor3);
					int b = distanceSquared >= 1 ? 128 : (int)(128 + 128 * scaleFactor3);
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)) * scaleFactor);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			//texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "Vortex3.png", FileMode.Create), texture.Width, texture.Height);

			if (Main.netMode != 2)
				Main.projectileTexture[projectile.type] = texture;*/
		}

		private const int LIFETIME = 720;

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.alpha = 255;
			Projectile.timeLeft = LIFETIME;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				for (int i = 0; i < Main.maxNPCs; i++)
				{
					if (Main.npc[i].type == NPCType<Electris>() && Main.npc[i].active)
					{
						Projectile.ai[0] = i;
						break;
					}
				}
			}

			Projectile.rotation -= 0.05f / Projectile.scale;

			if (!Main.npc[(int)Projectile.ai[0]].active || Main.npc[(int)Projectile.ai[0]].ai[1] == 2)
			{
				Projectile.hostile = false;
				Projectile.alpha += 16;
				if (Projectile.alpha > 255)
				{
					Projectile.Kill();
				}
				return;
			}

			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = (float)(0.01f + Math.Sqrt(1 - ((Projectile.timeLeft - (float)(LIFETIME / 2)) * (Projectile.timeLeft - (float)(LIFETIME / 2))) / ((float)(LIFETIME / 2) * (float)(LIFETIME / 2))));
			Projectile.alpha = (int)(192 * (1f - Math.Sqrt(1 - ((Projectile.timeLeft - (float)(LIFETIME / 2)) * (Projectile.timeLeft - (float)(LIFETIME / 2))) / ((float)(LIFETIME / 2) * (float)(LIFETIME / 2)))));

			Projectile.light = 5f * Projectile.scale;

			Projectile.width = (int)(905 * Projectile.scale);
			Projectile.height = (int)(905 * Projectile.scale);
			DrawOffsetX = (int)(0.5f * (Projectile.height - 1072));
			DrawOriginOffsetY = (int)(0.5f * (Projectile.height - 1072));
			DrawOriginOffsetX = 0;

			Projectile.Center = oldCenter;


			Player player = Main.player[Main.npc[(int)Projectile.ai[0]].target];

			Projectile.velocity = (player.Center - Projectile.Center) / 120f;

			if (Projectile.timeLeft > 20 && Projectile.timeLeft < LIFETIME - 20)
			{
				Projectile.hostile = true;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					if (Projectile.timeLeft % 75 == 0)
					{
						int rotation = (Projectile.timeLeft % 150 == 0) ? 0 : 3;

						for (int i = 0; i < 36; i++)
						{
							if ((i + rotation) % 6 < 3)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6f, 0).RotatedBy(i * MathHelper.TwoPi / 36 + MathHelper.TwoPi / 72), ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
							}
							else
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(6f, 0).RotatedBy(i * MathHelper.TwoPi / 36 + MathHelper.TwoPi / 72), ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
							}
						}
					}

					if (Projectile.timeLeft % 60 == 0)
                    {
						for (int i = 0; i < 16; i++)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.TwoPi / 16) * 4f, ProjectileType<MagneticBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 0.1f, ai1: player.whoAmI);
						}
					}
					if (Projectile.timeLeft % 15 == 0)
					{
						for (int i = 0; i < 8; i++)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.TwoPi / 8) * 6f, ProjectileType<ElectricBlast>(), Main.expertMode ? 30 : 40, 3f, Main.myPlayer, ai0: 1.02f);
						}
					}
				}
			}
			else
			{
				Projectile.hostile = false;
			}

			if (Projectile.timeLeft % 20 == 0)
			{
				SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1072, 1072), Color.White * (1 - Projectile.alpha / 255f), Projectile.rotation, new Vector2(536, 536), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }


	public class MagnetonDeathray : ModProjectile
	{
		// Use a different style for constant so it is very clear in code when a constant is used

		//The distance charge particle from the pixie center
		private const float MOVE_DISTANCE = 49f;

		private const int ACTIVE_TIME = 360;
		private const int INACTIVE_TIME = 120;

		//The electris which owns this laser
		public int owner
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Magnetic Deathray");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = ACTIVE_TIME + INACTIVE_TIME;
		}

        //deleted the draw laser code here
        public override bool PreDraw(ref Color lightColor)
        {
            // We start drawing the laser
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, Projectile.velocity, 32, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)MOVE_DISTANCE);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
        {
            float r = unit.ToRotation() + rotation;

            float offsetScale = Projectile.localAI[1] == 0 ? 1 : (float)Math.Pow(1 - Projectile.localAI[1] * (120 - Projectile.localAI[1]) / 3600f, 8) * 0.75f + 0.25f;
            Vector2 scaleVector = new Vector2(offsetScale, 1) * scale;

            Color c = Color.White;
            int numDraws = 3;
            c.A = (byte)(Projectile.localAI[1] == 0 ? 63 : 10);
            for (int x = 0; x < numDraws; x++)
            {
                Vector2 laserOffset = unit.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(MathHelper.TwoPi * x / numDraws + Projectile.frameCounter / 5f);
                if (Projectile.timeLeft < 8)
                {
                    laserOffset *= Projectile.timeLeft / 2f * offsetScale;
                    spriteBatch.Draw(texture, laserOffset + start + transDist * unit - Main.screenPosition,
                        new Rectangle(16 - Projectile.timeLeft * 2, Projectile.frame * 32 + 128, Projectile.timeLeft * 4, 32), c, r,
                        new Vector2(Projectile.timeLeft * 4 * .5f, 32 * .5f), scaleVector, 0, 0);
                }
                else if (Projectile.timeLeft < ACTIVE_TIME)
                {
                    laserOffset *= 4 * offsetScale;
                    spriteBatch.Draw(texture, laserOffset + start + transDist * unit - Main.screenPosition,
                        new Rectangle(0, Projectile.frame * 32 + 128, 32, 32), c, r,
                        new Vector2(32 * .5f, 32 * .5f), scaleVector, 0, 0);
                }
                else
                {
                    if (x == 0)
                    {
                        laserOffset *= 0;
                        spriteBatch.Draw(texture, start + transDist * unit - Main.screenPosition,
                            new Rectangle(14, Projectile.frame * 32, 4, 32), c * ((((INACTIVE_TIME + ACTIVE_TIME) - Projectile.timeLeft) / (float)(INACTIVE_TIME)) * 0.5f * offsetScale), r,
                            new Vector2(4 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                }

                // Draws the laser 'body'
                for (float i = transDist + 32f; i <= 2200; i += step)
                {
                    var origin = start + i * unit;

                    if (Projectile.timeLeft < 8)
                    {
                        spriteBatch.Draw(texture, laserOffset + origin - Main.screenPosition,
                            new Rectangle(16 - Projectile.timeLeft * 2, Projectile.frame * 32, Projectile.timeLeft * 4, 32), i < transDist ? Color.Transparent : c, r,
                            new Vector2(Projectile.timeLeft * 4 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                    else if (Projectile.timeLeft < ACTIVE_TIME)
                    {
                        spriteBatch.Draw(texture, laserOffset + origin - Main.screenPosition,
                            new Rectangle(0, Projectile.frame * 32, 32, 32), i < transDist ? Color.Transparent : c, r,
                            new Vector2(32 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                    else
                    {
                        if (x == 0)
                        {
                            spriteBatch.Draw(texture, origin - Main.screenPosition,
                                new Rectangle(14, Projectile.frame * 32, 4, 32), i < transDist ? Color.Transparent : c * ((((INACTIVE_TIME + ACTIVE_TIME) - Projectile.timeLeft) / (float)(INACTIVE_TIME)) * 0.5f * offsetScale), r,
                                new Vector2(4 * .5f, 32 * .5f), scaleVector, 0, 0);
                        }
                    }
                }
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft < ACTIVE_TIME)
			{
				Vector2 unit = Projectile.velocity;
				float point = 0f;
				// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
				// It will look for collisions on the given line using AABB
				return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
					Projectile.Center + unit * 2200, 32, ref point);
			}
			else
			{
				return false;
			}
		}

		// The AI of the projectile
		public override void AI()
		{
			NPC magneton = Main.npc[owner];
			Projectile.position = magneton.Center - Projectile.Hitbox.Size() / 2f + new Vector2(0, 1);

			if (Projectile.ai[0] == 0 || Projectile.timeLeft > ACTIVE_TIME)
			{
				Projectile.velocity = new Vector2(0, -1).RotatedBy(magneton.rotation);
			}
			if (!magneton.active || magneton.ai[1] == 2) { Projectile.Kill(); }

			CastLights();
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 2 == 0)
			{
				Projectile.frame = ++Projectile.frame % 4;
			}

			if (Projectile.timeLeft == ACTIVE_TIME)
			{
				SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
			}

			if (Projectile.ai[0] != 0)
			{
				NPC electris = Main.npc[(int)magneton.ai[0]];

				if (Projectile.timeLeft >= 8 && Projectile.timeLeft <= ACTIVE_TIME && electris.ai[2] == (int)PolaritiesAttackIDs.AnticipatingRays && electris.ai[3] <= 600)
				{
					Projectile.timeLeft = 9;
				}
			}

			if (Projectile.localAI[1] > 0)
			{
				Projectile.localAI[1]--;
				if (Projectile.localAI[1] == 0)
                {
					Projectile.hostile = true;
                }
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
			Projectile.localAI[1] = 120;
			Projectile.hostile = false;
		}

		private void CastLights()
		{
			// Cast a light along the line of the laser
			DelegateMethods.v3_1 = new Vector3(1f, 0.8f, 0.8f);
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (2200 - MOVE_DISTANCE), 26, DelegateMethods.CastLight);
		}

		public override bool ShouldUpdatePosition() => false;
	}

	public class ElectrisDeathray : ModProjectile
	{
		// Use a different style for constant so it is very clear in code when a constant is used

		//The distance charge particle from the pixie center
		private const float MOVE_DISTANCE = 49f;

		private const int ACTIVE_TIME = 360;
		private const int INACTIVE_TIME = 120;

		//The electris which owns this laser
		public int owner
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electric Deathray");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = ACTIVE_TIME + INACTIVE_TIME;
		}

        //deleted the draw laser code here
        public override bool PreDraw(ref Color lightColor)
        {
            // We start drawing the laser
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, Projectile.velocity, 32, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)MOVE_DISTANCE);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
        {
            float r = unit.ToRotation() + rotation;

            float offsetScale = Projectile.localAI[1] == 0 ? 1 : (float)Math.Pow(1 - Projectile.localAI[1] * (120 - Projectile.localAI[1]) / 3600f, 8) * 0.75f + 0.25f;
            Vector2 scaleVector = new Vector2(offsetScale, 1) * scale;

            Color c = Color.White;
            int numDraws = 3;
            c.A = (byte)(Projectile.localAI[1] == 0 ? 63 : 10);
            for (int x = 0; x < numDraws; x++)
            {
                Vector2 laserOffset = unit.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(MathHelper.TwoPi * x / numDraws + Projectile.frameCounter / 5f);
                if (Projectile.timeLeft < 8)
                {
                    laserOffset *= Projectile.timeLeft / 2f * offsetScale;
                    spriteBatch.Draw(texture, laserOffset + start + transDist * unit - Main.screenPosition,
                        new Rectangle(16 - Projectile.timeLeft * 2, Projectile.frame * 32 + 128, Projectile.timeLeft * 4, 32), c, r,
                        new Vector2(Projectile.timeLeft * 4 * .5f, 32 * .5f), scaleVector, 0, 0);
                }
                else if (Projectile.timeLeft < ACTIVE_TIME)
                {
                    laserOffset *= 4 * offsetScale;
                    spriteBatch.Draw(texture, laserOffset + start + transDist * unit - Main.screenPosition,
                        new Rectangle(0, Projectile.frame * 32 + 128, 32, 32), c, r,
                        new Vector2(32 * .5f, 32 * .5f), scaleVector, 0, 0);
                }
                else
                {
                    if (x == 0)
                    {
                        laserOffset *= 0;
                        spriteBatch.Draw(texture, start + transDist * unit - Main.screenPosition,
                            new Rectangle(14, Projectile.frame * 32, 4, 32), c * ((((INACTIVE_TIME + ACTIVE_TIME) - Projectile.timeLeft) / (float)(INACTIVE_TIME)) * 0.5f * offsetScale), r,
                            new Vector2(4 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                }

                // Draws the laser 'body'
                for (float i = transDist + 32f; i <= 2200; i += step)
                {
                    var origin = start + i * unit;

                    if (Projectile.timeLeft < 8)
                    {
                        spriteBatch.Draw(texture, laserOffset + origin - Main.screenPosition,
                            new Rectangle(16 - Projectile.timeLeft * 2, Projectile.frame * 32, Projectile.timeLeft * 4, 32), i < transDist ? Color.Transparent : c, r,
                            new Vector2(Projectile.timeLeft * 4 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                    else if (Projectile.timeLeft < ACTIVE_TIME)
                    {
                        spriteBatch.Draw(texture, laserOffset + origin - Main.screenPosition,
                            new Rectangle(0, Projectile.frame * 32, 32, 32), i < transDist ? Color.Transparent : c, r,
                            new Vector2(32 * .5f, 32 * .5f), scaleVector, 0, 0);
                    }
                    else
                    {
                        if (x == 0)
                        {
                            spriteBatch.Draw(texture, origin - Main.screenPosition,
                                new Rectangle(14, Projectile.frame * 32, 4, 32), i < transDist ? Color.Transparent : c * ((((INACTIVE_TIME + ACTIVE_TIME) - Projectile.timeLeft) / (float)(INACTIVE_TIME)) * 0.5f * offsetScale), r,
                                new Vector2(4 * .5f, 32 * .5f), scaleVector, 0, 0);
                        }
                    }
                }
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft < ACTIVE_TIME)
			{
				Vector2 unit = Projectile.velocity;
				float point = 0f;
				// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
				// It will look for collisions on the given line using AABB
				return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
					Projectile.Center + unit * 2200, 32, ref point);
			}
			else
			{
				return false;
			}
		}

		// The AI of the projectile
		public override void AI()
		{
			NPC electris = Main.npc[owner];
			Projectile.position = electris.Center - Projectile.Hitbox.Size() / 2f + new Vector2(0, 1);

			if (Projectile.ai[0] == 0 || Projectile.timeLeft > ACTIVE_TIME)
			{
				Projectile.velocity = new Vector2(0, -1).RotatedBy(electris.rotation);
			}
			if (!electris.active || electris.ai[1] == 2) { Projectile.Kill(); }

			CastLights();
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 2 == 0)
			{
				Projectile.frame = ++Projectile.frame % 4;
			}

			if (Projectile.timeLeft == ACTIVE_TIME)
			{
				SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
			}

			if (Projectile.ai[0] != 0)
            {
				if (Projectile.timeLeft >= 8 && Projectile.timeLeft <= ACTIVE_TIME && electris.ai[2] == (int)PolaritiesAttackIDs.AnticipatingRays && electris.ai[3] <= 600)
                {
					Projectile.timeLeft = 9;
				}
			}

			if (Projectile.localAI[1] > 0)
			{
				Projectile.localAI[1]--;
				if (Projectile.localAI[1] == 0)
				{
					Projectile.hostile = true;
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			Projectile.localAI[1] = 120;
			Projectile.hostile = false;
		}

		private void CastLights()
		{
			// Cast a light along the line of the laser
			DelegateMethods.v3_1 = new Vector3(1f, 0.8f, 0.8f);
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (2200 - MOVE_DISTANCE), 26, DelegateMethods.CastLight);
		}

		public override bool ShouldUpdatePosition() => false;
	}



	public class ElectromagneticDeathray : ModProjectile
	{
		private const int INACTIVE_TIME = 120;
		private const int ACTIVE_TIME = 240;

		private int owner
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		private Vector2 circleCenter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electromagnetic Deathray");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = INACTIVE_TIME + ACTIVE_TIME;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < 4)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + Projectile.timeLeft / 5f);
						Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(8 - 2 * Projectile.timeLeft, Projectile.frame * 16, 4 * Projectile.timeLeft, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(4 * Projectile.timeLeft * .5f, 16 * .5f), Projectile.scale, 0, 0);
					}
				}
			}
			else if (Projectile.timeLeft < ACTIVE_TIME)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + Projectile.timeLeft / 5f);
						Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(0, Projectile.frame * 16, 16, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(16 * .5f, 16 * .5f), Projectile.scale, 0, 0);
					}
				}
			}
			else
			{
				Color c = Color.White;// * ((360 - projectile.timeLeft) / 360f);
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
						new Rectangle(6, Projectile.frame * 16, 4, 16), c, initRotation + (i - 5000) / radius + r,
						new Vector2(4 * .5f, 16 * .5f), new Vector2(Projectile.scale * 0.5f, Projectile.scale), 0, 0);
				}
			}

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft >= ACTIVE_TIME)
			{
				return false;
			}
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			bool isInside = (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY)).Length() < (Projectile.Center - circleCenter).Length() + 8;

			float furthestX = targetHitbox.X + targetHitbox.Size().X / 2 - circleCenter.X > 0 ? targetHitbox.X + targetHitbox.Size().X : targetHitbox.X;
			float furthestY = targetHitbox.Y + targetHitbox.Size().Y / 2 - circleCenter.Y > 0 ? targetHitbox.Y + targetHitbox.Size().Y : targetHitbox.Y;
			bool isOutside = (new Vector2(circleCenter.X - furthestX, circleCenter.Y - furthestY)).Length() > (Projectile.Center - circleCenter).Length() - 8;

			return isInside && isOutside;
		}

		// The AI of the projectile
		public override void AI()
		{
			NPC magneton = Main.npc[owner];
			NPC electris = Main.npc[(int)magneton.ai[0]];
			if (!magneton.active || !electris.active || electris.ai[1] == 2 || magneton.ai[1] == 2) { Projectile.Kill(); }

			Projectile.position = magneton.Center - Projectile.Hitbox.Size() / 2f;
			Projectile.velocity = Projectile.velocity.RotatedBy(-0.0065f);

			//circle center = intersection of bisector between m & e, and perpendicular at m to v
			float a1 = magneton.Center.X - electris.Center.X;
			float b1 = magneton.Center.Y - electris.Center.Y;
			float c1 = (magneton.Center.X * magneton.Center.X + magneton.Center.Y * magneton.Center.Y - electris.Center.X * electris.Center.X - electris.Center.Y * electris.Center.Y) / 2;
			float a2 = Projectile.velocity.X;
			float b2 = Projectile.velocity.Y;
			float c2 = Projectile.velocity.X * magneton.Center.X + Projectile.velocity.Y * magneton.Center.Y;
			circleCenter = new Vector2(
				(b2 * c1 - b1 * c2) / (a1 * b2 - a2 * b1),
				(a2 * c1 - a1 * c2) / (a2 * b1 - a1 * b2)
			);

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 2)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % 4;
			}

			if (Projectile.timeLeft == ACTIVE_TIME)
			{
				SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool ShouldUpdatePosition() => false;
	}

	public class ElectromagneticDeathray2 : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/ElectromagneticDeathray";

		private const int INACTIVE_TIME = 120;
		private const int ACTIVE_TIME = 720;

		private int owner
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		private Vector2 circleCenter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electromagnetic Deathray");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = INACTIVE_TIME + ACTIVE_TIME;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < 4)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + Projectile.timeLeft / 5f);
						Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(8 - 2 * Projectile.timeLeft, Projectile.frame * 16, 4 * Projectile.timeLeft, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(4 * Projectile.timeLeft * .5f, 16 * .5f), Projectile.scale, 0, 0);
					}
				}
			}
			else if (Projectile.timeLeft < ACTIVE_TIME)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + Projectile.timeLeft / 5f);
						Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(0, Projectile.frame * 16, 16, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(16 * .5f, 16 * .5f), Projectile.scale, 0, 0);
					}
				}
			}
			else
			{
				Color c = Color.White;// * ((360 - projectile.timeLeft) / 360f);
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
						new Rectangle(6, Projectile.frame * 16, 4, 16), c, initRotation + (i - 5000) / radius + r,
						new Vector2(4 * .5f, 16 * .5f), new Vector2(Projectile.scale * 0.5f, Projectile.scale), 0, 0);
				}
			}

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft >= ACTIVE_TIME)
			{
				return false;
			}
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			bool isInside = (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY)).Length() < (Projectile.Center - circleCenter).Length() + 8;

			float furthestX = targetHitbox.X + targetHitbox.Size().X / 2 - circleCenter.X > 0 ? targetHitbox.X + targetHitbox.Size().X : targetHitbox.X;
			float furthestY = targetHitbox.Y + targetHitbox.Size().Y / 2 - circleCenter.Y > 0 ? targetHitbox.Y + targetHitbox.Size().Y : targetHitbox.Y;
			bool isOutside = (new Vector2(circleCenter.X - furthestX, circleCenter.Y - furthestY)).Length() > (Projectile.Center - circleCenter).Length() - 8;

			return isInside && isOutside;
		}

		// The AI of the projectile
		public override void AI()
		{
			NPC magneton = Main.npc[owner];
			NPC electris = Main.npc[(int)magneton.ai[0]];
			if (!magneton.active || !electris.active || electris.ai[1] == 2 || magneton.ai[1] == 2) { Projectile.Kill(); }

			Projectile.position = magneton.Center - Projectile.Hitbox.Size() / 2f;
			Projectile.velocity = Projectile.velocity.RotatedBy(-0.005f);

			//circle center = intersection of bisector between m & e, and perpendicular at m to v
			float a1 = magneton.Center.X - electris.Center.X;
			float b1 = magneton.Center.Y - electris.Center.Y;
			float c1 = (magneton.Center.X * magneton.Center.X + magneton.Center.Y * magneton.Center.Y - electris.Center.X * electris.Center.X - electris.Center.Y * electris.Center.Y) / 2;
			float a2 = Projectile.velocity.X;
			float b2 = Projectile.velocity.Y;
			float c2 = Projectile.velocity.X * magneton.Center.X + Projectile.velocity.Y * magneton.Center.Y;
			circleCenter = new Vector2(
				(b2 * c1 - b1 * c2) / (a1 * b2 - a2 * b1),
				(a2 * c1 - a1 * c2) / (a2 * b1 - a1 * b2)
			);

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 2)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % 4;
			}

			if (Projectile.timeLeft == ACTIVE_TIME)
			{
				SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
			}

			if (Projectile.ai[1] == 1)
			{
				//we're in the harder version of the attack, so create expanding rings
				if (Projectile.timeLeft % 120 == 0)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<ElectromagneticDeathray3>(), Main.expertMode ? 40 : 60, 0f, Main.myPlayer, ai0: Projectile.ai[0]);

					if (Projectile.timeLeft <= 720)
					{
						SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
					}
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool ShouldUpdatePosition() => false;
	}

	public class ElectromagneticDeathray3 : ModProjectile
	{
		public override string Texture => "Polarities/Content/NPCs/Bosses/Hardmode/MagnetonElectris/ElectromagneticDeathray";

		private int owner
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		private Vector2 circleCenter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Electromagnetic Deathray");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = 600;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			//don't draw if radius too small
			if (Projectile.ai[1] < 6)
            {
				return false;
            }

			/*if (projectile.timeLeft < 4)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = Main.projectileTexture[projectile.type];

				float radius = (projectile.Center - circleCenter).Length();
				float initRotation = (projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + projectile.timeLeft / 5f);
						spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(8 - 2 * projectile.timeLeft, projectile.frame * 16, 4 * projectile.timeLeft, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(4 * projectile.timeLeft * .5f, 16 * .5f), projectile.scale, 0, 0);
					}
				}
			}
			else*/
			if (Projectile.timeLeft % 120 >= 60)
			{
				Color c = Color.White;
				c.A = 127;
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					for (int j = 0; j < 2; j++)
					{
						float laserOffset = (float)Math.Sin(MathHelper.TwoPi * j / 2 + Projectile.timeLeft / 5f);
						Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + new Vector2(laserOffset, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
							new Rectangle(0, Projectile.frame * 16, 16, 16), c, initRotation + (i - 5000) / radius + r,
							new Vector2(16 * .5f, 16 * .5f), Projectile.scale, 0, 0);
					}
				}
			}
			else
			{
				Color c = Color.White;// * ((360 - projectile.timeLeft) / 360f);
				float r = 0;
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				float radius = (Projectile.Center - circleCenter).Length();
				float initRotation = (Projectile.Center - circleCenter).ToRotation();

				for (int i = 0; i < Math.Min(MathHelper.TwoPi * radius, 10000); i += 15)
				{
					Main.spriteBatch.Draw(texture, new Vector2(radius, 0).RotatedBy(initRotation + (i - 5000) / radius) + circleCenter - Main.screenPosition,
						new Rectangle(6, Projectile.frame * 16, 4, 16), c, initRotation + (i - 5000) / radius + r,
						new Vector2(4 * .5f, 16 * .5f), new Vector2(Projectile.scale * 0.5f, Projectile.scale), 0, 0);
				}
			}

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.timeLeft % 120 < 60)
			{
				return false;
			}
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			bool isInside = (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY)).Length() < (Projectile.Center - circleCenter).Length() + 8;

			float furthestX = targetHitbox.X + targetHitbox.Size().X / 2 - circleCenter.X > 0 ? targetHitbox.X + targetHitbox.Size().X : targetHitbox.X;
			float furthestY = targetHitbox.Y + targetHitbox.Size().Y / 2 - circleCenter.Y > 0 ? targetHitbox.Y + targetHitbox.Size().Y : targetHitbox.Y;
			bool isOutside = (new Vector2(circleCenter.X - furthestX, circleCenter.Y - furthestY)).Length() > (Projectile.Center - circleCenter).Length() - 8;

			return isInside && isOutside;
		}

		// The AI of the projectile
		public override void AI()
		{
			NPC magneton = Main.npc[owner];
			NPC electris = Main.npc[(int)magneton.ai[0]];
			if (!magneton.active || !electris.active || electris.ai[1] == 2 || magneton.ai[1] == 2) { Projectile.Kill(); }

			if ((electris.ai[2] == (int)PolaritiesAttackIDs.HarderCircleDeathrays && electris.ai[3] >= 836) || (magneton.ai[2] == (int)PolaritiesAttackIDs.HarderCircleDeathrays && magneton.ai[3] >= 836) || (magneton.ai[2] != (int)PolaritiesAttackIDs.HarderCircleDeathrays && electris.ai[2] != (int)PolaritiesAttackIDs.HarderCircleDeathrays))
            {
				if (Projectile.timeLeft > 4)
                {
					Projectile.timeLeft = 4;
				}
            } else if ((electris.ai[2] == (int)PolaritiesAttackIDs.HarderCircleDeathrays && electris.ai[3] < 120) || (magneton.ai[2] == (int)PolaritiesAttackIDs.HarderCircleDeathrays && magneton.ai[3] < 120))
            {
				Projectile.timeLeft = 602;
            }

			Projectile.ai[1] += 2f;

			//circle center = midpoint of c and point in the same direction from the origin as c with radius/x the distance
			float innerRadius = Math.Min(Projectile.ai[1], (magneton.Center - electris.Center).Length());
			Vector2 mainCenter = (magneton.Center + electris.Center) / 2f;
			float direction = innerRadius > (magneton.Center - mainCenter).Length() ? -1 : 1;
			Vector2 innerIntersection = magneton.Center + magneton.DirectionTo(electris.Center) * innerRadius;
			Vector2 outerIntersection = mainCenter + direction * (magneton.Center - mainCenter) * (magneton.Center - mainCenter).Length() / (innerIntersection - mainCenter).Length();

			circleCenter = (innerIntersection + outerIntersection) / 2f;

			Projectile.position = innerIntersection - Projectile.Hitbox.Size() / 2f;

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 2)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % 4;
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.noKnockback = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			//target.AddBuff(BuffID.Electrified, 180, true);
			//target.AddBuff(BuffType<Magnetized>(), 180, true);
		}

		public override bool ShouldUpdatePosition() => false;
	}

    public class PolaritiesSky : CustomSky
    {
		private bool isActive;

		public static float height;
		public static float alpha;

		public override void OnLoad()
		{
		}

		public override void Update(GameTime gameTime)
		{
		}

		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
		{
			//draw the sky
			if (maxDepth >= 0 && minDepth < 0)
			{
				//black in the middle, dark blue when electris is stationary, dark red when magneton is stationary
				float multiplier = 0.15f;
				Color drawColor = new Color(Math.Max(0,height) * multiplier, 0f, Math.Max(0, -height) * multiplier, alpha);

				spriteBatch.Draw(ModContent.Request<Texture2D>("Polarities/Content/Projectiles/CallShootProjectile").Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), drawColor);
			}
		}

		public override float GetCloudAlpha()
		{
			return 1f;
		}

		public override void Activate(Vector2 position, params object[] args)
		{
			isActive = true;
		}

		public override void Deactivate(params object[] args)
		{
			isActive = false;
		}

		public override void Reset()
		{
			isActive = false;
		}

		public override bool IsActive()
		{
			return isActive;
		}
	}


	public class BlastTelegraph : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_644";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Telegraph");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 60;

			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 0f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC electris = Main.npc[(int)Projectile.ai[0]];

			Projectile.Center = electris.Center;

			if (!electris.active || electris.ai[1] == 2) { Projectile.Kill(); }
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float alpha = 0.5f * (60 - Projectile.timeLeft) / 60f;
			Color color = new Color(181, 248, 255);
			float rotation = Projectile.velocity.ToRotation();

			if (Projectile.ai[1] == 1)
            {
				color = new Color(255, 181, 181);
			}

			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), color * alpha, rotation, new Vector2(36, 36), new Vector2(0.5f, 12), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), color * alpha, rotation + MathHelper.PiOver2, new Vector2(36, 36), new Vector2(0.5f, 12), SpriteEffects.None, 0f);

			return false;
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}
