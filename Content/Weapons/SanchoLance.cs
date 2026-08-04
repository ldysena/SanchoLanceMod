using System.Collections.Generic;
using SanchoLanceMod.Common.Players;
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
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(SanchoModPlayer.MaxEnhanceDuration);
		public static LocalizedText CurrentHardbloodText { get; private set; }

        public SoundStyle useSFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/smalluse_", 2) with { Volume = 0.4f };

		// Sets up dynamic tooltip modification for displaying hardblood info
        public override void SetStaticDefaults() { CurrentHardbloodText = this.GetLocalization("CurrentHardblood"); }

		public override void SetDefaults()
		{
			Item.DefaultToSpear(ModContent.ProjectileType<SanchoLanceProjectile>(), 1f, 18);
            Item.SetWeaponValues(110, 15f, 0);
			Item.channel = true;
            Item.UseSound = useSFX;
			Item.StopAnimationOnHurt = true;

            Item.DamageType = DamageClass.MeleeNoSpeed; 
			Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 0, 5)); 
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
		
		public override bool AltFunctionUse(Player player) { return true; }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // On right click, tell SanchoModPlayer to check if we can enhance the lance
                if (!player.GetModPlayer<SanchoModPlayer>().EnhanceSanchoLance()) { return false; }

                // TODO: Set transform swing values
                Item.DefaultToSpear(ModContent.ProjectileType<SanchoTransformProjectile>(), 1f, 18);
                Item.SetWeaponValues(110, 15f, 0);
                Item.channel = false;
                Item.UseSound = null; // SFX handled by SanchoModPlayer
			    Item.StopAnimationOnHurt = false;
            }  
            else
            {
                // TODO: clean up unnecessary variable sets???
                Item.DefaultToSpear(ModContent.ProjectileType<SanchoLanceProjectile>(), 1f, 18);
                Item.SetWeaponValues(110, 15f, 0);
                Item.channel = true;
                Item.UseSound = useSFX;
			    Item.StopAnimationOnHurt = true;
            }

            return base.CanUseItem(player);
        }

		// Updates tooltip with current Hardblood
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			SanchoModPlayer modplayer = Main.LocalPlayer.GetModPlayer<SanchoModPlayer>();
			
			string hardbloodPercent = modplayer.HardbloodPercent().ToString();
			string hardblood = modplayer.hardblood.ToString();
			string hardbloodMax = SanchoModPlayer.HardbloodMax.ToString();

			TooltipLine currentHardblood = new TooltipLine(Mod, "CurrentHardblood%", CurrentHardbloodText.Format(hardbloodPercent, hardblood, hardbloodMax));
			//currentHardblood.OverrideColor = null; // TODO: Use this to create a dynamic color based on hardblood?
			tooltips.Add(currentHardblood);
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