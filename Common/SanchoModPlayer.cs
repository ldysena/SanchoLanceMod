using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LaSangreMod.Common
{
	// This file shows the very basics of using ModPlayer classes.
	// The comments in this file and https://github.com/tModLoader/tModLoader/wiki/ModPlayer are useful for learning how to use ModPlayer in your mod.

	// ModPlayer classes provide a way to attach data to Players and act on that data.
	// This example will hopefully provide you with an understanding of the basic building blocks of how ModPlayer works.
	// This example will teach the most commonly sought after effect: "How to do X if the player has Y?"
	// X in this example will be "Apply a debuff to enemies."
	// Y in this example will be "Wearing an accessory."
	// After studying this example, you can change X to other effects by changing the "hook" you use or the code within the hook you use. For example, you could use OnHitByNPC and call Projectile.NewProjectile within that hook to change X to "When the player is hit by NPC, spawn Projectiles".
	// We can change Y to other conditions as well. For example, you could give the player the effect by having a "potion" ModItem give a ModBuff that sets the ModPlayer variable in ModBuff.Update
	// Another example would be an armor set effect. Simply use the ModItem.UpdateArmorSet hook.
	// The point is, each of these effects follow the same pattern.

	// Below you will see the ModPlayer class (SimpleModPlayer), and below that will be a ModItem class called SimpleAccessory which is an accessory item. These are both in the same file for your reading convenience. This accessory will give our effect to our ModPlayer.

	// This is the ModPlayer class. Make note of the classname, which is SimpleModPlayer, since we will be using this in the accessory item below.
	public class SanchoModPlayer : ModPlayer
	{
		// Here we declare the FrostBurnSummon variable which will represent whether this player has the effect or not.
        public const int HARDBLOOD_MAX = 9999;
		public int hardBlood = 0; // TODO: Reset hardblood when we don't have the weapon / we drop it and only track when we have it

		// ResetEffects is used to reset effects back to their default value. Terraria resets all effects every frame back to defaults so we will follow this design. (You might think to set a variable when an item is equipped and un-assign the value when the item in unequipped, but Terraria is not designed that way.)
		/*public override void ResetEffects() 
        {
			hardBlood = 0;
		}*/

		// Here we use a "hook" to actually let our FrostBurnSummon status take effect. This hook is called anytime a player owned projectile hits an enemy.
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) 
        {
            // TODO: Use OnHitNPCWithProj and OnHitNPCWithItem to implement double hardblood gain when using La Sangre itself
			if(hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed))
            {
                hardBlood += damageDone;
            }

            if(hardBlood > HARDBLOOD_MAX) { hardBlood = HARDBLOOD_MAX; }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) 
        {
            // TODO: Use OnHitNPCWithProj and OnHitNPCWithItem to implement double hardblood gain when using La Sangre itself
			if(hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.MeleeNoSpeed))
            {
                hardBlood += damageDone;
            }

            if(hardBlood > HARDBLOOD_MAX) { hardBlood = HARDBLOOD_MAX; }
        }

		// As a recap. Make a class variable, reset that variable in ResetEffects, and use that variable in the logic of whatever hooks you use.
	}
}

