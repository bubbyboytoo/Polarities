using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Polarities.Content.Items.Ammo.Hardmode;
using Terraria.DataStructures;

namespace Polarities.Content.Items.Weapons.Ranged.Bows.Hardmode
{
	public class ArcBolter : ModItem
	{
        private int parity;

		public override void SetStaticDefaults() {
			//Tooltip.SetDefault("Summons a ring of arrows around the cursor"+"\nElectrifies and magnetizes wooden arrows");
		}

		public override void SetDefaults() {
			Item.damage = 55;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 17;
			Item.height = 33;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.knockBack = 4;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
			Item.shoot = 10;
			Item.shootSpeed = 8f;
			Item.useAmmo = AmmoID.Arrow;
		}



        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			for (int i = 0; i < 6; i++)
			{
				int newType = (type == ProjectileID.WoodenArrowFriendly) ? (i % 2 == parity ? ProjectileType<ElectricArrowProjectile>() : ProjectileType<MagneticArrowProjectile>()) : type;

				Projectile projectile = Main.projectile[Projectile.NewProjectile(source, Main.MouseWorld + new Vector2(360, 0).RotatedBy(i * MathHelper.Pi / 3), new Vector2(-velocity.Length(), 0).RotatedBy(i * MathHelper.Pi / 3), newType, damage, knockback, player.whoAmI)];
				projectile.noDropItem = true;
			}
			parity = (parity + 1) % 2;
			return false;
		}
	}
}