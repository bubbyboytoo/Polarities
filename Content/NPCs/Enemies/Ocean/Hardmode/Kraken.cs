﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using MultiHitboxNPCLibrary;
using Polarities.Content.Items.Placeable.Banners.Items;
using Polarities.Core;
using Polarities.Global;
using Polarities.Assets;
using Polarities.Content.Items.Materials.Hardmode;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.NPCs.Enemies.Ocean.Hardmode
{
    public class Kraken : ModNPC
    {
        private Vector2 wanderGoal;
        private bool doAnimation;
        private int AttackPattern
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private int AttackCooldown
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;

            //NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            //{
                //SpecificallyImmuneTo = new int[] {
                    //BuffID.Confused,
                    //BuffID.OnFire
                //}
            //};
            // NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);

            //MultiHitboxNPC.MultiHitboxNPCTypes.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 14;
            NPC.width = 42;
            NPC.height = 42;

            NPC.damage = 50;
            NPC.lifeMax = 4000;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.value = 50000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = false;

            Banner = Type;
            BannerItem = ItemType<KrakenBanner>();
        }

        private const int numSegments = 5;

        public override bool PreAI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;

                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<KrakenTentacle>(), ai0: NPC.whoAmI, ai1: -0.1f);
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<KrakenTentacle>(), ai0: NPC.whoAmI, ai1: 0.1f);
            }

            NPC.wet = Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16].LiquidAmount > 64;
            if (NPC.wet)
            {
                NPC.noGravity = true;
                NPC.ai[2] = 60;

                NPC.TargetClosest(false);
                Player player = Main.player[NPC.target];

                if (player.wet)
                {
                    //attac
                    switch (AttackPattern)
                    {
                        case 0:
                            //direct charge series
                            if (AttackCooldown % 70 == 1)
                            {
                                doAnimation = true;
                            }
                            if (AttackCooldown % 70 <= 20 && AttackCooldown < 300)
                            {
                                NPC.velocity += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.9f;
                                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                            }
                            else if (AttackCooldown >= 300)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    AttackPattern = Main.rand.Next(3);
                                }
                                NPC.netUpdate = true;
                                AttackCooldown = 0;
                            }
                            NPC.velocity *= 0.975f;
                            AttackCooldown++;
                            break;
                        case 1:
                            //orbit while tentacles attack
                            if (AttackCooldown % 40 == 1)
                            {
                                doAnimation = true;
                            }
                            if (AttackCooldown % 40 <= 20 && AttackCooldown < 240)
                            {
                                NPC.velocity += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Math.PI / 4) * 0.75f;
                                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                            }
                            else if (AttackCooldown >= 240)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    AttackPattern = Main.rand.Next(3);
                                }
                                NPC.netUpdate = true;
                                AttackCooldown = 0;
                            }
                            NPC.velocity *= 0.975f;
                            AttackCooldown++;
                            break;
                        case 2:
                            //move away from player while releasing ink cloud
                            if (AttackCooldown < 20)
                            {
                                NPC.velocity += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.9f;
                                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                            }
                            else if (AttackCooldown >= 70 && AttackCooldown < 90)
                            {
                                NPC.velocity -= (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.4f;
                                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -NPC.velocity.SafeNormalize(Vector2.Zero) * 5f, ProjectileType<KrakenInk>(), 1, 0, Main.myPlayer);
                            }
                            else if (AttackCooldown >= 110)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    AttackPattern = Main.rand.Next(2);
                                }
                                NPC.netUpdate = true;
                                AttackCooldown = 0;
                            }
                            NPC.velocity *= 0.975f;
                            AttackCooldown++;
                            break;
                    }
                }
                else
                {
                    //wander
                    if (AttackCooldown % 90 == 1)
                    {
                        doAnimation = true;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float rotationAmount = (NPC.velocity + new Vector2(0.5f, 0).RotatedByRandom(MathHelper.TwoPi)).ToRotation();

                            wanderGoal = NPC.Center + new Vector2(200, 0).RotatedBy(rotationAmount);
                        }
                        NPC.netUpdate = true;
                    }
                    if (AttackCooldown % 90 <= 20)
                    {
                        NPC.velocity += (wanderGoal - NPC.Center).SafeNormalize(Vector2.Zero) * 0.75f;
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    }
                    NPC.velocity *= 0.975f;
                    AttackCooldown++;
                }
            }
            else
            {
                NPC.noGravity = false;

                if (NPC.velocity.Y == 0)
                {
                    //realize that we're beached so tentacles stop attacking

                    //try to jump to the ocean
                    NPC.ai[2]++;
                    if (NPC.ai[2] == 120)
                    {
                        NPC.ai[2] = 0;
                        NPC.velocity.Y = -6;
                        NPC.velocity.X = NPC.Center.X < Main.maxTilesX * 8 ? -4 : 4;
                    }
                    else
                    {
                        NPC.velocity.X = 0;
                    }
                }
                else
                {
                    if (NPC.ai[2] < 60)
                    {
                        NPC.ai[2]++;
                        NPC.velocity.X = NPC.Center.X < Main.maxTilesX * 8 ? -4 : 4;
                    }

                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }

            return false;
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 4; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("KrakenGore" + i).Type);

            //adjust hitboxes for custom loot dropping
            //this system is a little kludgy
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            if (doAnimation || NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter == 5)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y = (NPC.frame.Y + frameHeight) % (8 * frameHeight);
                    if (NPC.frame.Y == 0)
                    {
                        doAnimation = false;
                    }
                }
            }
            else
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 center = NPC.frame.Size() / 2;
            Color color = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, center, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.hardMode)
            {
                return SpawnCondition.Ocean.Chance * 0.05f;
            }
            else
            {
                return 0f;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Tentacle>(), 1, 3, 8));
        }
    }

    public class KrakenTentacle : ModNPC
    {
        private int kraken
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                //don't show up in bestiary
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 18;
            NPC.height = 18;
            DrawOffsetY = -4;

            NPC.damage = 36;
            NPC.lifeMax = 4000;

            NPC.dontTakeDamage = true;
            NPC.dontCountMe = true;
            NPC.npcSlots = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override void AI()
        {
            if (!Main.npc[kraken].active)
            {
                //die
                NPC.active = false;
                return;
            }

            bool trailBody = true;

            if (Main.player[Main.npc[kraken].target].wet && Main.npc[kraken].ai[0] == 1)
            {
                //tentacle attack
                if ((NPC.Center - Main.npc[kraken].Center).Length() < 600)
                {
                    trailBody = false;

                    Player player = Main.player[Main.npc[kraken].target];
                    if ((NPC.ai[1] == -0.1f && NPC.ai[2] == 30) ||
                        (NPC.ai[1] == 0.1f && NPC.ai[2] == 60) ||
                        NPC.ai[2] == 90
                    )
                    {
                        //dash at player
                        NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 16f;
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi;
                    }
                    NPC.velocity *= 0.97f;
                }
                NPC.ai[2]++;
                if (NPC.ai[2] >= 120)
                {
                    NPC.ai[2] = 0;
                }
            }

            if (trailBody)
            {
                //trail body
                Vector2 goalPos = Main.npc[kraken].Center + new Vector2(0, 180).RotatedBy(Main.npc[kraken].rotation + NPC.ai[1]);
                Vector2 goalVelocity = Main.npc[kraken].velocity + (goalPos - NPC.Center) / 10;
                NPC.velocity += (goalVelocity - NPC.velocity) / 20;
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi;//Main.npc[kraken].rotation + MathHelper.Pi / 2;//NPC.velocity.ToRotation()-MathHelper.PiOver2;
            }
        }

        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.whoAmI == 0) return true;
            NPC owner = Main.npc[kraken];

            Vector2[] bezierPoints = { owner.Center, owner.Center + new Vector2(0, 80).RotatedBy(owner.rotation), NPC.Center + new Vector2(-80, 0).RotatedBy(NPC.rotation), NPC.Center };
            float bezierProgress = 0;
            float bezierIncrement = 6;

            Texture2D chainTexture = ChainTexture.Value;
            Vector2 chainTextureCenter = new(4, 4);

            float rotation;

            while (bezierProgress < 1)
            {
                //draw stuff
                Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

                //increment progress
                while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
                {
                    bezierProgress += 0.2f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
                }

                Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
                rotation = (newPos - oldPos).ToRotation() + MathHelper.Pi;

                Vector2 drawPos = (oldPos + newPos) / 2;
                Color segmentColor = owner.GetAlpha(owner.GetNPCColorTintedByBuffs(Lighting.GetColor((int)drawPos.X / 16, (int)drawPos.Y / 16)));

                spriteBatch.Draw(chainTexture, drawPos - Main.screenPosition, new Rectangle(0, 0, 8, 8), segmentColor, rotation, chainTextureCenter, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
            }

            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 center = NPC.frame.Size() / 2;
            Color color = owner.GetAlpha(owner.GetNPCColorTintedByBuffs(drawColor));

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, center, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            return false;
        }
    }

    public class KrakenInk : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;

            Projectile.width = 10;
            Projectile.height = 10;

            DrawOffsetX = -31;
            DrawOriginOffsetY = -31;
            DrawOriginOffsetX = 0;

            Projectile.alpha = 0;
            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 5;
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.ai[1] = Main.rand.NextFloat(-0.15f, 0.15f);
                }
                Projectile.netUpdate = true;
            }
            Projectile.ai[0]--;

            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]) * 0.996f;

            if (Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16].LiquidAmount != 255)
            {
                Projectile.velocity.Y += 0.15f;
                if (Projectile.velocity.Y < 0)
                {
                    Projectile.velocity.Y = 0;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft < 120)
            {
                Projectile.scale -= 1 / 120f;
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            if (Projectile.timeLeft < 120)
            {
                return false;
            }
            if (Collision.CheckAABBvAABBCollision(target.position, new Vector2(target.width, target.height), Projectile.position, new Vector2(Projectile.width, Projectile.height)))
            {
                target.AddBuff(BuffID.Obstructed, 10, true);
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Color mainColor = Color.Black;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(36, 36), Projectile.scale, SpriteEffects.None, 0);

            for (int k = 0; k < Math.Min(Projectile.oldPos.Length, 358 - Projectile.timeLeft); k++)
            {
                Color color = mainColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale), SpriteEffects.None, 0);
            }
            return false;
        }
    }
}