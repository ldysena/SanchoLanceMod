using System.Collections.Generic;
using SanchoLanceMod.Common.Players;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;

namespace SanchoLanceMod.Content.Weapons
{
	public class SanchoLanceEnhanced : ModItem
	{
		public static LocalizedText CurrentHardbloodText { get; private set; }

		// Sets up dynamic tooltip modification for displaying hardblood info
		public override void SetStaticDefaults()
        {
           	CurrentHardbloodText = this.GetLocalization("CurrentHardblood");
        }

		public override void SetDefaults()
		{
			Item.DefaultToSpear(ModContent.ProjectileType<Projectiles.SanchoLanceEnhancedProjectile>(), 1.4f, 18);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.SetWeaponValues(390, 10f, 0);
			Item.SetShopValues(ItemRarityColor.StrongRed10, Item.buyPrice(1, 6, 1, 5)); // TODO: make a crazy item color????
			Item.channel = true; 
			Item.UseSound = new SoundStyle("SanchoLanceMod/Assets/Sounds/biguse_", 2) with { Volume = 0.4f };
			Item.StopAnimationOnHurt = false;
		}		

		// Transforms weapon according to isEnhanced state in SanchoModPlayer
        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

			if (!player.GetModPlayer<SanchoModPlayer>().isEnhanced) 
			{
				int prefix = Item.prefix;
				Item.ChangeItemType(ModContent.ItemType<SanchoLance>());
				Item.Prefix(prefix);
			}
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

	}
}