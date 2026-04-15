using System.Collections.Generic;
using System.Linq;
using LaSangreMod.Common;
using LaSangreMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria.Localization;

namespace LaSangreMod.Content.Weapons
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class LaSangreWeapon : ModItem
	{
			public static LocalizedText CurrentHardbloodText { get; private set; }
        	public override void SetStaticDefaults()
        	{
            	CurrentHardbloodText = this.GetLocalization("CurrentHardblood");
        	}
			public override void SetDefaults()
			{
				// A special method that sets a variety of item parameters that make the item act like a spear weapon.
				// To see everything DefaultToSpear() does, right click the method in Visual Studios and choose "Go To Definition" (or press F12). You can also hover over DefaultToSpear to see the documentation.
				// The shoot speed will affect how far away the projectile spawns from the player's hand.
				// If you are using the custom AI in your projectile (and not aiStyle 19 and AIType = ProjectileID.JoustingLance), the standard value is 1f.
				// If you are using aiStyle 19 and AIType = ProjectileID.JoustingLance, then multiply the value by about 3.5f.
				Item.DefaultToSpear(ModContent.ProjectileType<Projectiles.LaSangreProjectile>(), 1.1f, 18);

				Item.DamageType = DamageClass.MeleeNoSpeed; // We need to use MeleeNoSpeed here so that attack speed doesn't effect our held projectile.

				Item.SetWeaponValues(180, 12f, 0); // A special method that sets the damage, knockback, and bonus critical strike chance.

				Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 0, 5)); // A special method that sets the rarity and value.

				Item.channel = true; // Channel is important for our projectile.

				// This will make sure our projectile completely disappears on hurt.
				// It's not enough just to stop the channel, as the lance can still deal damage while being stowed
				// If two players charge at each other, the first one to hit should cancel the other's lance
				Item.StopAnimationOnHurt = true;
		}

		// On right click, tell SanchoModPlayer to check if we can enhance the lance
		public override bool AltFunctionUse(Player player)
		{
			player.GetModPlayer<SanchoModPlayer>().activateEnhancement();
			return false; // Return false so it does not attack when we do this?
		}

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			int hardbloodPercent = Main.LocalPlayer.GetModPlayer<SanchoModPlayer>().hardbloodPercent();
			TooltipLine currentHardblood = new TooltipLine(Mod, "CurrentHardblood%", CurrentHardbloodText.Format(hardbloodPercent.ToString()));
			currentHardblood.OverrideColor = null; // TODO: Use this to create a dynamic color based on hardbloodPercent
			tooltips.Add(currentHardblood);
            base.ModifyTooltips(tooltips);
        }


		// Reset hardblood when picking up
		// TODO: since we check if we have the item at all times to gain, do we really need thsi???? Not really
        public override bool OnPickup(Player player)
        {
			// TODO: This logic does not work and also manage case for picking up from a chest

			// Do not reset hardblood if we alreayd have La Sangre
			if(!player.HasItem(ModContent.ItemType<LaSangreWeapon>()) && !player.HasItem(ModContent.ItemType<AscendantWeapon>()))
			{
				player.GetModPlayer<SanchoModPlayer>().hardBlood = 0; // Reset hardblood when obtaining weapon
			}

            return base.OnPickup(player);
        }

		// Adds damage to hardblood a second time, when hitting with La Sangre
		// TODO: This does not proc!
        /*public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            player.GetModPlayer<SanchoModPlayer>().hardBlood += damageDone * 2;
			if(player.GetModPlayer<SanchoModPlayer>().hardBlood > SanchoModPlayer.HARDBLOOD_MAX) { player.GetModPlayer<SanchoModPlayer>().hardBlood = SanchoModPlayer.HARDBLOOD_MAX; }
            base.OnHitNPC(player, target, hit, damageDone);
        }*/

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
			//player.GetModPlayer<SanchoModPlayer>().hardBlood
			
			// Temporary way to check hardblood 
			// TODO: Implement final using ModifyTooltip() to display on tooltip???
			if(Main.GameUpdateCount % 180 == 0)
			{
				Main.NewText("Hardblood  = " + player.GetModPlayer<SanchoModPlayer>().hardBlood + " / " + SanchoModPlayer.HARDBLOOD_MAX + " = " + player.GetModPlayer<SanchoModPlayer>().hardbloodPercent());
			}

			if (player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<AscendantWeapon>());
				Item.Prefix(prefix);
			}
		}

		// Draws the hardblood meter
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			// TODO: Draw the hardblood meter
        }

	}
}