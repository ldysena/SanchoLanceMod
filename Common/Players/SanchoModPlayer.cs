using System;
using SanchoLanceMod.Content.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SanchoLanceMod.Common.Players
{
	/// <summary>
	/// Class for managing Hardblood (SanchoLance resource) per player
	/// </summary>
	public class SanchoModPlayer : ModPlayer
	{
        public static readonly int HardbloodMax = 15000; // Maximum Hardblood and the amount required to enhance SanchoLance
        public static readonly int MaxEnhanceDuration = 7; // Approximate duration of enhanced SanchoLance (in seconds)
		public static readonly int DecrementPerTick = HardbloodMax / (MaxEnhanceDuration * 60); // We use Hardblood as a timer to track the duration of the enhanced state

		public int hardblood = 0; // Resource for SanchoLance, measured in damage dealt (externally called "Bloodfeast")
		public bool readyToEnhance = false; // Flag to check when Hardblood is full
		public bool isEnhanced = false; // Flag to check if SanchoLance is currently enhanced

		public SoundStyle enhanceBeginSFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhancesound") with { Volume = 0.7f };
		public SoundStyle enhanceEndSFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhanceend") with { Volume = 0.7f };
		public SoundStyle enhanceReadySFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/enhanceready") with { Volume = 0.7f };

		/// <summary>
		/// Helper method to enhance La Sangre if Hardblood is full
		/// </summary>
		public void EnhanceSanchoLance()
		{
			if(readyToEnhance) 
			{ 
				isEnhanced = true; 
				readyToEnhance = false;
				SoundEngine.PlaySound(enhanceBeginSFX);
			}
            // TODO: SFX for right clicking when NOT ready?
		}

		/// <summary>
		/// Helper method to display Hardblood percentage in tooltip
		/// </summary>
		/// <returns>Percent of max Hardblood currently held as an integer</returns>
		public int HardbloodPercent()
		{
			return 100 * hardblood / HardbloodMax;
		}

        public override void PostUpdate()
        {
			if(isEnhanced)
			{
				hardblood -= DecrementPerTick; // Calculated from static readonly values at top of file
				if (hardblood < 0) 
				{ 
					hardblood = 0; 
					isEnhanced = false;
					SoundEngine.PlaySound(enhanceEndSFX);
				}
			}
        }

		// Resets all Hardblood tracking variables on player death
        public override void UpdateDead()
        {
			hardblood = 0;
			readyToEnhance = false;
			isEnhanced = false;
        }

		 // We use a ModPlayer to track all melee damage dealt by the player when not enhanced and add to Hardblood
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) 
        {
			if( !isEnhanced && !readyToEnhance 
			    && target.type != NPCID.TargetDummy // Prevents target dummy abuse
			    && (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed))
			    && Player.HasItem(ModContent.ItemType<SanchoLance>()) ) // Only track if we are holding SanchoLance
            {
				if(Player.HeldItem.ModItem is SanchoLance) { hardblood += 2 * Math.Min(damageDone, target.lifeMax); } // SanchoLance gains double Hardblood
				else { hardblood += Math.Min(damageDone, target.lifeMax); } // We cap Hardblood gain to enemy's max HP to prevent bunny abuse

				if(hardblood >= HardbloodMax) 
				{ 
					hardblood = HardbloodMax; 
					readyToEnhance = true;
					SoundEngine.PlaySound(enhanceReadySFX);
				}
            }
        }
	}
}

