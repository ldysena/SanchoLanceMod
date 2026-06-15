using System;
using SanchoLanceMod.Content.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SanchoLanceMod.Common
{
	/// <summary>
	/// Class for managing Hardblood (resource for La Sangre)
	/// </summary>
	public class SanchoModPlayer : ModPlayer
	{
		// TODO: Make static readonly and refactor naming convention for safety
        public const int HARDBLOOD_MAX = 15000; // Maximum Hardblood the player can have at once, also the amount required to enhance the weapon
		public const int MAX_ENHANCE_DURATION = 7; // Duration of enhanced La Sangre (guesstimate in seconds)
		public const int DECREMENT_PER_TICK = HARDBLOOD_MAX / (MAX_ENHANCE_DURATION * 60); //  Due to integer rounding we can't match the exact time but that doesn't matter too much

		public int hardblood = 0; // Resource for La Sangre, measured in damage dealt
		public bool readyToEnhance = false; // Flag to check when Hardblood is full (ie hardblood == HARDBLOOD_MAX)
		public bool isEnhanced = false; // Flag to check if La Sangre is currently enhanced

		public SoundStyle enhanceBegin = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhancesound") with { Volume = 0.7f };
		public SoundStyle enhanceEnd = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhanceend") with { Volume = 0.7f };
		public SoundStyle enhanceReady = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhanceready") with { Volume = 0.7f };

		/// <summary>
		/// Helper method to enhance La Sangre if Hardblood is full
		/// </summary>
		public void EnhanceLaSangre()
		{
			if(readyToEnhance) 
			{ 
				isEnhanced = true; 
				readyToEnhance = false;
				SoundEngine.PlaySound(enhanceBegin);
			}
		}

		/// <summary>
		/// Helper method to display Hardblood percentage in tooltip
		/// </summary>
		/// <returns>Percent of max Hardblood currently held</returns>
		public int HardbloodPercent()
		{
			return 100 * hardblood / HARDBLOOD_MAX;
		}

		// We use Hardblood as a timer to track the duration of the enhanced state
        public override void PostUpdate()
        {
			if(isEnhanced)
			{
				hardblood -= DECREMENT_PER_TICK; // Calculated from const values at top of file
				if (hardblood < 0) 
				{ 
					hardblood = 0; 
					isEnhanced = false;
					SoundEngine.PlaySound(enhanceEnd);
				}
			}

            base.PostUpdate();
        }

		// Resets all Hardblood tracking variables on player death
        public override void UpdateDead()
        {
			hardblood = 0;
			readyToEnhance = false;
			isEnhanced = false;
            base.UpdateDead();
        }

		 // We use a ModPlayer to track all melee damage dealt by the player when not enhanced and add to Hardblood
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) 
        {
			if( !isEnhanced && !readyToEnhance 
			    && target.type != NPCID.TargetDummy // Prevents target dummy abuse
			    && (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed))
			    && Player.HasItem(ModContent.ItemType<SanchoLance>()) ) // Only track if we are holding La Sangre
            {
				if(Player.HeldItem.ModItem is SanchoLance) { hardblood += 2 * Math.Min(damageDone, target.lifeMax); } // La Sangre gains double Hardblood
				else { hardblood += Math.Min(damageDone, target.lifeMax); } // We cap Hardblood gain to enemy's max HP to prevent bunny abuse while allowing some 'overflow'

				if(hardblood >= HARDBLOOD_MAX) 
				{ 
					hardblood = HARDBLOOD_MAX; 
					readyToEnhance = true;
					SoundEngine.PlaySound(enhanceReady);
				}
            }
        }
	}
}

