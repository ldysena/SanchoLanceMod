using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LaSangreMod.Content.Projectiles
{
	public class LaSangreProjectile : ModProjectile
	{
        public static readonly float lifestealPercent = 0.025f;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DismountsPlayersOnHit[Type] = true;
			ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;

			// The width and height do not affect the collision of the Jousting Lance because we calculate that separately (see Colliding() below)
			Projectile.width = 128;
			Projectile.height = 128;

			// Since we are using custom AI below, we set the aiStyle to -1.
			Projectile.aiStyle = -1;

			Projectile.alpha = 255; 
			Projectile.friendly = true; 
			Projectile.penetrate = -1; 
			Projectile.tileCollide = false;
			Projectile.scale = 1f; 
			Projectile.hide = true; 
			Projectile.ownerHitCheck = true; 
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
		}

		// This is the behavior of the Jousting Lances.
		public override void AI()
		{
			Player owner = Main.player[Projectile.owner]; // Get the owner of the projectile.
			Projectile.direction = owner.direction; // Direction will be -1 when facing left and +1 when facing right. 
			owner.heldProj = Projectile.whoAmI; // Set the owner's held projectile to this projectile. heldProj is used so that the projectile will be killed when the player drops or swap items.

			int itemAnimationMax = owner.itemAnimationMax;
			// Remember, frames count down from itemAnimationMax to 0
			// Frame at which the lance is fully extended. Hold at this frame before retracting.
			// Scale factor (0.34f) means the last 34% of the animation will be used for retracting.
			int holdOutFrame = (int)(itemAnimationMax * 0.34f);
			if (owner.channel && owner.itemAnimation < holdOutFrame) {
				owner.SetDummyItemTime(holdOutFrame); // This makes it so the projectile never dies while we are holding it (except when we take damage, see ExampleJoustingLancePlayer).
			}

			// If the Jousting Lance is no longer being used, kill the projectile.
			if (owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
				return;
			}

			int itemAnimation = owner.itemAnimation;
			// extension and retraction factors (0-1). As the animation plays out, extension goes from 0-1 and stays at 1 while holding, then retraction goes from 0-1.
			float extension = 1 - Math.Max(itemAnimation - holdOutFrame, 0) / (float)(itemAnimationMax - holdOutFrame);
			float retraction = 1 - Math.Min(itemAnimation, holdOutFrame) / (float)holdOutFrame;

			// Distances are in pixels
			float extendDist = 24; // How far to fly out during extension
			float retractDist = extendDist / 2; // How far to fly back during retraction
			float tipDist = 98 + extension * extendDist - retraction * retractDist; // If your Jousting Lance is larger or smaller than the standard size, it is recommended to change the shoot speed of the item instead of this value.

			Vector2 center = owner.RotatedRelativePoint(owner.MountedCenter); // Get the center of the owner. This accounts for the player being shifted up or down while riding a mount, sitting in a chair, etc.
			Projectile.Center = center; // Set the center of the projectile to the center of the owner. Projectile.Center is now actually the tip of the Jousting Lance.

			// TODO: We should be able to get the lance to follow the mouse by changing Projectsile.velocity to be based on the mouse position and player position

			Projectile.position += Projectile.velocity * tipDist; // The projectile velocity contains the orientation of the lance, multiply it by the tipDist to position the tip.

			// Set the rotation of the projectile.
			// For reference, 0 is the top left, 180 degrees or pi radians is the bottom right.
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + (float)Math.PI * 3 / 4f;

			// Fade the projectile in when it first spawns
			Projectile.alpha -= 40;
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}

			// The Hallowed and Shadow Jousting Lance spawn dusts when the player is moving above a certain speed.
			float minimumDustVelocity = 6f;

			// This Vector2.Dot is the dot product between the projectile's velocity and the player's velocity normalized to be between -1 and 1.
			// What this means in this context is that the speed value will be closer to positive 1 if the player is moving in the same direction as the direction the lance was shot.
			// Example: if the lance is shot up and to the right, the value here will be closer to 1 if the player is also moving up and to the right.
			float movementInLanceDirection = Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.UnitX * owner.direction), owner.velocity.SafeNormalize(Vector2.UnitX * owner.direction));

			float playerVelocity = owner.velocity.Length();

			if (playerVelocity > minimumDustVelocity && movementInLanceDirection > 0.8f) {
				// The chance for the dust to spawn. The actual chance (see below) is 1/dustChance. We make the chance higher the faster the player is moving by making the denominator smaller.
				int dustChance = 8;
				if (playerVelocity > minimumDustVelocity + 1f) {
					dustChance = 5;
				}
				if (playerVelocity > minimumDustVelocity + 2f) {
					dustChance = 2;
				}

				// Set your dust types here.
				//int dustTypeCommon = ModContent.DustType<Dusts.Sparkle>();
				int dustTypeCommon = DustID.WhiteTorch;
				int dustTypeRare = DustID.WhiteTorch;

				int offset = 4; // This offset will affect how much the dust spreads out.

				// Spawn the dusts based on the dustChance. The dusts are spawned at the tip of the Jousting Lance.
				if (Main.rand.NextBool(dustChance)) {
					int newDust = Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeCommon, Projectile.velocity.X * 0.2f + (Projectile.direction * 3), Projectile.velocity.Y * 0.2f, 100, default, 1.2f);
					Main.dust[newDust].noGravity = true;
					Main.dust[newDust].velocity *= 0.25f;
					newDust = Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeCommon, 0f, 0f, 150, default, 1.4f);
					Main.dust[newDust].velocity *= 0.25f;
				}

				if (Main.rand.NextBool(dustChance + 3)) {
					Dust.NewDust(Projectile.Center - new Vector2(offset, offset), offset * 2, offset * 2, dustTypeRare, 0f, 0f, 150, default, 1.4f);
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// Modifiers from player velocity
			modifiers.Knockback *= Main.player[Projectile.owner].velocity.Length() / 7f;
			modifiers.SourceDamage *= 0.1f + Main.player[Projectile.owner].velocity.Length() / 7f * 0.9f;
		}

        // Manages lifesteal on hit
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            if(owner.lifeSteal > 0) // For balance, we respect vanilla lifesteal mechanics
            {
                int healAmount = (int)(damageDone * lifestealPercent);
                owner.Heal(healAmount);
                owner.lifeSteal -= Math.Min(healAmount, owner.statLifeMax - owner.statLife); // Since we deal a lot of damage, we cap lifesteal decrement by current missing HP
            }
        }


		// This is the custom collision that Jousting Lances uses. 
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float rotationFactor = Projectile.rotation + (float)Math.PI / 4f; // The rotation of the Jousting Lance.
			float scaleFactor = 95f; // How far back the hit-line will be from the tip of the Jousting Lance. You will need to modify this if you have a longer or shorter Jousting Lance. Vanilla uses 95f
			float widthMultiplier = 23f; // How thick the hit-line is. Increase or decrease this value if your Jousting Lance is thicker or thinner. Vanilla uses 23f
			float collisionPoint = 0f; // collisionPoint is needed for CheckAABBvLineCollision(), but it isn't used for our collision here. Keep it at 0f.

			// This Rectangle is the width and height of the Jousting Lance's hitbox which is used for the first step of collision.
			// You will need to modify the last two numbers if you have a bigger or smaller Jousting Lance.
			// Vanilla uses (0, 0, 300, 300) which that is quite large for the size of the Jousting Lance.
			// The size doesn't matter too much because this rectangle is only a basic check for the collision (the hit-line is much more important).
			Rectangle lanceHitboxBounds = new Rectangle(0, 0, 300, 300);

			// Set the position of the large rectangle.
			lanceHitboxBounds.X = (int)Projectile.position.X - lanceHitboxBounds.Width / 2;
			lanceHitboxBounds.Y = (int)Projectile.position.Y - lanceHitboxBounds.Height / 2;

			// This is the back of the hit-line with Projectile.Center being the tip of the Jousting Lance.
			Vector2 hitLineEnd = Projectile.Center + rotationFactor.ToRotationVector2() * scaleFactor;

			// The following is for debugging the size of the hit line. This will allow you to easily see where it starts and ends.
			// Dust.NewDustPerfect(Projectile.Center, DustID.Pixie, Velocity: Vector2.Zero, Scale: 0.5f);
			// Dust.NewDustPerfect(hitLineEnd, DustID.Pixie, Velocity: Vector2.Zero, Scale: 0.5f);

			// First check that our large rectangle intersects with the target hitbox.
			// Then we check to see if a line from the tip of the Jousting Lance to the "end" of the lance intersects with the target hitbox.
			if (lanceHitboxBounds.Intersects(targetHitbox)
				&& Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, hitLineEnd, widthMultiplier * Projectile.scale, ref collisionPoint)) {
				return true;
			}
			return false;
		}

		// We need to draw the projectile manually. If you don't include this, the Jousting Lance will not be aligned with the player.
		public override bool PreDraw(ref Color lightColor)
		{

			// SpriteEffects change which direction the sprite is drawn.
			SpriteEffects spriteEffects = SpriteEffects.None;

			// Get texture of projectile.
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			// Get the currently selected frame on the texture.
			Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

			// The origin in this case is (0, 0) of our projectile because Projectile.Center is the tip of our Jousting Lance.
			Vector2 origin = Vector2.Zero;

			// The rotation of the projectile.
			float rotation = Projectile.rotation;

			// If the projectile is facing right, we need to rotate it by -90 degrees, move the origin, and flip the sprite horizontally.
			// This will make it so the bottom of the sprite is correctly facing down when shot to the right.
			if (Projectile.direction > 0) {
				rotation -= (float)Math.PI / 2f;
				origin.X += sourceRectangle.Width;
				spriteEffects = SpriteEffects.FlipHorizontally;
			}

			// The position of the sprite. Not subtracting Main.player[Projectile.owner].gfxOffY will cause the sprite to bounce when walking up blocks.
			Vector2 position = new(Projectile.Center.X, Projectile.Center.Y - Main.player[Projectile.owner].gfxOffY);

			// Apply lighting and draw our projectile
			Color drawColor = Projectile.GetAlpha(lightColor);

			Main.EntitySpriteDraw(texture,
				position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
				sourceRectangle, drawColor, rotation, origin, Projectile.scale, spriteEffects, 0);

			 //The following is for debugging the size of the collision rectangle. Set this to the same size as the one you have in Colliding().
			 //Rectangle lanceHitboxBounds = new Rectangle(0, 0, 300, 300);
			 //Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value,
			 //	new Vector2((int)Projectile.Center.X - lanceHitboxBounds.Width / 2, (int)Projectile.Center.Y - lanceHitboxBounds.Height / 2) - Main.screenPosition,
			 //	lanceHitboxBounds, Color.Orange * 0.5f, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

			// It's important to return false, otherwise we also draw the original texture.
			return false;
		}
	}
}