using Humanizer;
using LaSangreMod.Content.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LaSangreMod.Common
{
	// All hardblood (resource) management happens here
	public class SanchoModPlayer : ModPlayer
	{
        public const int HARDBLOOD_MAX = 15000; // Maximum hardblood the player can have at once, also the amount required to buff the weapon
		public const int MAX_ENHANCE_DURATION = 7; // Duration of enhanced La Sangre (guesstimate in seconds)
		public const int DECREMENT_PER_TICK = HARDBLOOD_MAX / (MAX_ENHANCE_DURATION * 60); // Due to integer rounding we can't get the exact time but that doens't matter too much over performance

		public int hardblood = 0; // Weapon resource
		public bool readyToEnhance = false; // Check to play sound when Hardblood is first filled
		public bool isEnhanced = false; // Check if enhanced La Sangre is being used

		// Helper method to check hardblood requirement to enhance La Sangre
		public void enhanceLaSangre()
		{
			if(readyToEnhance) 
			{ 
				isEnhanced = true; 
				readyToEnhance = false;
			}
		}

		// Helper method to display hardblood % in tooltip
		public int hardbloodPercent()
		{
			return 100 * hardblood / HARDBLOOD_MAX;
		}

		// Decrements hardblood and manages reset during enhanced state
        public override void PostUpdate()
        {
			if(isEnhanced)
			{
				hardblood -= DECREMENT_PER_TICK;
				if (hardblood < 0) 
				{ 
					hardblood = 0; 
					isEnhanced = false;
				}
			}

            base.PostUpdate();
        }

		// Reset all hardblood variables on death
        public override void UpdateDead()
        {
			hardblood = 0;
			readyToEnhance = false;
			isEnhanced = false;
            base.UpdateDead();
        }

		// Tracks all melee damage when not enhanced and adds to hardblood
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) 
        {
			// If it's slow, is it because of Player.HasItem?
			if(!isEnhanced && !readyToEnhance && Player.HasItem(ModContent.ItemType<LaSangreWeapon>()) && (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed)))
            {
				if(Player.HeldItem.ModItem is LaSangreWeapon) { hardblood += 2 * damageDone; } // La Sangre gets double hardblood
				else { hardblood += damageDone; }

				if(hardblood >= HARDBLOOD_MAX) 
				{ 
					// TODO: Play SFX

					hardblood = HARDBLOOD_MAX; 
					readyToEnhance = true;
				}
            }
        }
	}
}

