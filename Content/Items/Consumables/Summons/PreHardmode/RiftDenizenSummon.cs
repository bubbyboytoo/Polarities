using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Core;
using Polarities.Global;
using Polarities.Content.NPCs.Bosses.PreHardmode.RiftDenizen;
using ReLogic.Content;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Content.Items.Consumables.Summons.PreHardmode
{
	public class RiftDenizenSummon : ModItem
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Instability");
			//Tooltip.SetDefault("Summons the Rift Denizen");
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.maxStack = 20;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
			Item.noUseGraphic = true;

			Item.shoot = ProjectileType<RiftDenizenSummonProjectile>();
			Item.shootSpeed = 2f;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(NPCType<RiftDenizen>()) && player.ownedProjectileCounts[Item.shoot] == 0;
		}

		/*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			NPC.SpawnOnPlayer(player.whoAmI, NPCType<RiftDenizen>());
			return true;
		}*/
	}

	public class RiftDenizenSummonProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Content/Items/Consumables/Summons/PreHardmode/RiftDenizenSummon";

		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("Instability");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 22;

			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 360;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = true;
		}

		public override void OnKill(int timeLeft)
		{
			Player player = Main.player[Projectile.owner];
			NPC.SpawnOnPlayer(player.whoAmI, NPCType<RiftDenizen>());
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            PolaritiesSystem.DrawCacheProjsBehindWalls.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft > 60)
			{
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
				Rectangle frame = texture.Frame();

				float distance = (Projectile.timeLeft - 60) / 300f;

				Vector2 eyePosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);
				Vector2 projectilePosition = Projectile.Center - Main.screenPosition;

				Vector2 drawPosition = (distance * projectilePosition + (1 - distance) * eyePosition);

				Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2, Projectile.scale * distance, SpriteEffects.None, 0f);
			}
			else if (Projectile.timeLeft > 50)
			{
				Texture2D texture = TextureAssets.Projectile[644].Value;
                Rectangle frame = texture.Frame();

				Vector2 drawPosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);

				float scale = Math.Max(0, 1 - (55 - Projectile.timeLeft) * (55 - Projectile.timeLeft) / 25f);

				Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White * 0.5f, Projectile.rotation + MathHelper.PiOver4, frame.Size() / 2, scale * 0.5f, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White * 0.5f, Projectile.rotation + 3 * MathHelper.PiOver4, frame.Size() / 2, scale * 0.5f, SpriteEffects.None, 0f);
			}

			return false;
		}
	}
}
