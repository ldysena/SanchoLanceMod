using System.Collections.Generic;
using SanchoLanceMod.Common;
using SanchoLanceMod.Content.Projectiles;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Audio;
using System.Buffers.Text;
using Terraria.ID;

namespace SanchoLanceMod.Content.Weapons
{
	public class SanchoLance : ModItem
	{
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(SanchoModPlayer.MAX_ENHANCE_DURATION);
		public static LocalizedText CurrentHardbloodText { get; private set; }

		// Sets up dynamic tooltip modification for displaying hardblood info
        public override void SetStaticDefaults()
        {
           	CurrentHardbloodText = this.GetLocalization("CurrentHardblood");
        }

		public override void SetDefaults()
		{
			Item.DefaultToSpear(ModContent.ProjectileType<SanchoLanceProjectile>(), 1f, 18);

			Item.DamageType = DamageClass.MeleeNoSpeed; 
			Item.SetWeaponValues(110, 15f, 0);
			Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 0, 5)); 
			Item.channel = true;
            Item.UseSound = new SoundStyle("SanchoLanceMod/Assets/Sounds/smalluse_", 2) with { Volume = 0.4f };
			Item.StopAnimationOnHurt = true;
		}
		
		// Transforms weapon according to isEnhanced state in SanchoModPlayer
        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

			if (player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<SanchoLanceEnhanced>());
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

		// Updates tooltip with current Hardblood
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			SanchoModPlayer modplayer = Main.LocalPlayer.GetModPlayer<SanchoModPlayer>();
			
			string hardbloodPercent = modplayer.HardbloodPercent().ToString();
			string hardblood = modplayer.hardblood.ToString();
			string hardbloodMax = SanchoModPlayer.HARDBLOOD_MAX.ToString();

			TooltipLine currentHardblood = new TooltipLine(Mod, "CurrentHardblood%", CurrentHardbloodText.Format(hardbloodPercent, hardblood, hardbloodMax));
			//currentHardblood.OverrideColor = null; // TODO: Use this to create a dynamic color based on hardblood?
			tooltips.Add(currentHardblood);
            base.ModifyTooltips(tooltips);
        }

        // Weapon should be unlocked by post-Plantera eclipse
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BrokenHeroSword)
                .AddIngredient(ItemID.BloodMoonStarter)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}