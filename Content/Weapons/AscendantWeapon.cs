using LaSangreMod.Common;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.Audio;

namespace LaSangreMod.Content.Weapons
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class AscendantWeapon : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToSpear(ModContent.ProjectileType<Projectiles.AscendantProjectile>(), 1.5f, 18);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.SetWeaponValues(600, 12f, 0);
			Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 1, 5)); // TODO: make a crazy item color????
			Item.channel = true; 
			Item.UseSound = new SoundStyle("LaSangreMod/Assets/Sounds/sanchodon_3_3-1") with { Volume = 0.5f };
			Item.StopAnimationOnHurt = false;
		}		

		// Transforms weapon according to isEnhanced state in SanchoModPlayer
        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

			if (!player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<LaSangreWeapon>());
				Item.Prefix(prefix);
			}
		}

	}
}