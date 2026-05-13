using System.Collections.Generic;
using LaSangreMod.Common;
using LaSangreMod.Content.Projectiles;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.Localization;

namespace LaSangreMod.Content.Weapons
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class LaSangreWeapon : ModItem
	{
		public static LocalizedText CurrentHardbloodText { get; private set; }

		// Temp function for exampleresourcebar to function
        public override void SetStaticDefaults()
        {
           	CurrentHardbloodText = this.GetLocalization("CurrentHardblood");
        }

		public override void SetDefaults()
		{
			Item.DefaultToSpear(ModContent.ProjectileType<LaSangreProjectile>(), 1.0f, 18);

			Item.DamageType = DamageClass.MeleeNoSpeed; 
			Item.SetWeaponValues(180, 12f, 0);
			Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 0, 5)); 
			Item.channel = true;
			Item.StopAnimationOnHurt = true;
		}
		
		// Transforms weapon according to isEnhanced state in SanchoModPlayer
        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

			if (player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<AscendantWeapon>());
				Item.Prefix(prefix);
			}
		}

		// On right click, tell SanchoModPlayer to check if we can enhance the lance
		public override bool AltFunctionUse(Player player)
		{
			// TODO: Implement "flair" swing on successful enhance
			player.GetModPlayer<SanchoModPlayer>().EnhanceLaSangre();
			return false; // Return false so it does not attack when we do this?
		}

		// Updates tooltip with current Hardblood %
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			int hardbloodPercent = Main.LocalPlayer.GetModPlayer<SanchoModPlayer>().HardbloodPercent();
			TooltipLine currentHardblood = new TooltipLine(Mod, "CurrentHardblood%", CurrentHardbloodText.Format(hardbloodPercent.ToString()));
			currentHardblood.OverrideColor = null; // TODO: Use this to create a dynamic color based on hardbloodPercent
			tooltips.Add(currentHardblood);
            base.ModifyTooltips(tooltips);
        }
	}
}