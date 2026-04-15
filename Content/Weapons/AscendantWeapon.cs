using LaSangreMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;

namespace LaSangreMod.Content.Weapons
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class AscendantWeapon : ModItem
	{
			public override void SetDefaults()
			{
				// A special method that sets a variety of item parameters that make the item act like a spear weapon.
				// To see everything DefaultToSpear() does, right click the method in Visual Studios and choose "Go To Definition" (or press F12). You can also hover over DefaultToSpear to see the documentation.
				// The shoot speed will affect how far away the projectile spawns from the player's hand.
				// If you are using the custom AI in your projectile (and not aiStyle 19 and AIType = ProjectileID.JoustingLance), the standard value is 1f.
				// If you are using aiStyle 19 and AIType = ProjectileID.JoustingLance, then multiply the value by about 3.5f.
				Item.DefaultToSpear(ModContent.ProjectileType<Projectiles.AscendantProjectile>(), 1.5f, 18);

				Item.DamageType = DamageClass.MeleeNoSpeed; // We need to use MeleeNoSpeed here so that attack speed doesn't effect our held projectile.

				Item.SetWeaponValues(600, 12f, 0); // A special method that sets the damage, knockback, and bonus critical strike chance.

				// TODO: make a crazy item color????
				Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 0, 5)); // A special method that sets the rarity and value.

				Item.channel = true; // Channel is important for our projectile.

				// This will make sure our projectile completely disappears on hurt.
				// It's not enough just to stop the channel, as the lance can still deal damage while being stowed
				// If two players charge at each other, the first one to hit should cancel the other's lance
				Item.StopAnimationOnHurt = false;
		}

		// Transform weapon back and stop channel @ 0 hardblood durring holdout
        public override void HoldItem(Player player)
        {
			if (!player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<LaSangreWeapon>());
				Item.Prefix(prefix);

				player.channel = false;
			}

            base.HoldItem(player);
        }

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
			
			// Temporary way to check hardblood 
			// TODO: Implement final using ModifyTooltip() to display on tooltip???
			if(Main.GameUpdateCount % 180 == 0)
			{
				Main.NewText("Hardblood  = " + player.GetModPlayer<SanchoModPlayer>().hardblood + " / " + SanchoModPlayer.HARDBLOOD_MAX );
			}

			// Transform weapon back @ 0 hardblood in inventory
			if (!player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<LaSangreWeapon>());
				Item.Prefix(prefix);
			}
		}

	}
}