using Humanizer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LaSangreMod.Common
{
	public class SanchoModPlayer : ModPlayer
	{
        public const int HARDBLOOD_MAX = 50000; // Maximum hardblood the player can have at once, also the amount required to buff the weapon
		//public const int HARDBLOOD_BUFF_REQ = (int)(HARDBLOOD_MAX * 0.5);
		public const int MAX_ENHANCE_DURATION = 15; // Duration of the Ascendant buff when used at maximum hardblood (in seconds)
		public const int DECREMENT_PER_TICK = HARDBLOOD_MAX / (MAX_ENHANCE_DURATION * 60);

		public int hardBlood = 0; 
		public bool isEnhanced = false;

		// Helper method to check hardblood requirement to enhance weapon
		public void activateEnhancement()
		{
			if( hardBlood >= HARDBLOOD_MAX) 
			{ 
				isEnhanced = true; 
			}
		}

		// Manage hardblood here
        public override void PostUpdate()
        {
			if(isEnhanced)
			{
				hardBlood -= DECREMENT_PER_TICK;
				if (hardBlood < 0) 
				{ 
					hardBlood = 0; 
					isEnhanced = false;
				}
			}

            base.PostUpdate();
        }


		// Tracks all melee damage dealt and adds to hardblood
		// Additional hardblood from hits with La Sangre are tracked in LaSangreWeapon.cs?? TODO: Make that work??
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) 
        {
			// If it's slow, is it because of Player.HasItem?
			if(!isEnhanced && Player.HasItem(ModContent.ItemType<Content.Weapons.LaSangreWeapon>()) && (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed)))
            {
                hardBlood += damageDone;
				if(hardBlood > HARDBLOOD_MAX) { hardBlood = HARDBLOOD_MAX; }
            }
        }
	}
}

