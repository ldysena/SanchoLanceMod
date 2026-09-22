using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using SanchoLanceMod.Common.Players;

namespace SanchoLanceMod.Content.Projectiles
{
	public class SanchoTransformProjectile : ModProjectile
	{
		private const float SWINGRANGE = 1.05f * (float)Math.PI; // The angle a swing attack covers (300 deg)
		private const float UNWIND = 0.4f; // When should the sword start disappearing

        // What stage of the attack is being executed, see functions found in AI for description
		private enum AttackStage { Transform, Execute, Unwind, Pose }

		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private AttackStage CurrentStage 
        {
			get => (AttackStage)Projectile.localAI[0];
			set 
            {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
			}
		}

		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		private float transTime => 65f;
		private float execTime => 10f;
		private float slowTime => 4f;
        private float poseTime => 60f;

        // Variables for animating via spritesheet
        private int currentFrame = 0; 
        private float transformProgress = 0;
        private const int numFrames = 18;

		public override string Texture => "SanchoLanceMod/Content/Projectiles/transform_projectile";

		private Player Owner => Main.player[Projectile.owner];

        public SoundStyle transformSFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/transformswing") with { Volume = 0.7f };
        public SoundStyle reverbSFX = new SoundStyle("SanchoLanceMod/Assets/Sounds/transformreverb") with { Volume = 0.5f };

		public override void SetStaticDefaults() 
        {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

		public override void SetDefaults() 
        {
			Projectile.width = 150; // Hitbox width of projectile
			Projectile.height = 150; // Hitbox height of projectile
			Projectile.friendly = true; // Projectile hits enemies
			Projectile.timeLeft = 10000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // Projectile is a melee projectile
            Projectile.scale = 1.0f;
		}

		public override void OnSpawn(IEntitySource source) 
        {
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1; // Sets direction based on mouse
            InitialAngle = (float)Math.Atan2(Main.MouseWorld.Y - Owner.Center.Y, Main.MouseWorld.X - Owner.Center.X);

            SoundEngine.PlaySound(transformSFX);
		}

		public override void SendExtraAI(BinaryWriter writer)
         {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) 
        {
			Projectile.spriteDirection = reader.ReadSByte(); 
		}

		public override void AI() 
        {
			// Extend use animation until projectile is killed
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed) 
            {
				Projectile.Kill();
				return;
			}

			// AI depends on stage
			switch (CurrentStage) 
            {
				case AttackStage.Transform:
					TransformWeapon();
					break;
				case AttackStage.Execute:
					ExecuteStrike();
					break;
				case AttackStage.Unwind:
					UnwindStrike();
					break;
                default:
                    PoseAfterStrike();
                    break;
			}

			SetSwordPosition();
			Timer++;
		}

		public override bool PreDraw(ref Color lightColor) 
        {
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			int handleOffset = 44; // Used to move the handle so we hold it at the right position
            Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

			if (Projectile.spriteDirection > 0) // Right
            { 
                origin = new Vector2(handleOffset, Projectile.height - handleOffset);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.FlipHorizontally;
			}
			else 
            { 
                origin = new Vector2(Projectile.width - handleOffset, Projectile.height - handleOffset);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.None;
			}

			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle sourceRectangle = texture.Frame(numFrames, 1, currentFrame, 0);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);

			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}

		// Find the start and end of the lance and use a line collider to check for collision with enemies
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) 
        {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}

		// Do a similar collision check for tiles
		public override void CutTiles() 
        {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}

        // Avoid multiplayer sync issues by not dealing damage :P
		public override bool? CanDamage() { return false; }

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) 
        {
			// Make knockback go away from player
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;
		}

		// Function to easily set projectile and arm position
		public void SetSwordPosition() 
        {
			Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

			// Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand
			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition; // Set projectile to arm position

			Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
		}

		// Function facilitating the taking out of the sword
		private void TransformWeapon() {
			//Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
			Size = MathHelper.SmoothStep(0, 1, Timer / transTime); // Make sword slowly increase in size as we prepare to strike until it reaches max

            if (Timer > 15 && currentFrame < numFrames - 1) 
            { 
                transformProgress += 0.7f; // Roughly 33 FPS
                currentFrame = (int)transformProgress; 
            }

			if (Timer >= transTime) 
            {
                Projectile.Kill();
				//CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
                //SoundEngine.PlaySound(reverbSFX);
			}
		}

		// Function facilitating the first half of the swing
		private void ExecuteStrike() 
        {
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

			if (Timer >= execTime) 
            {
				CurrentStage = AttackStage.Unwind;
                Owner.direction *= -1;
                Projectile.spriteDirection *= -1;
			}
		}

		// Function facilitating the latter half of the swing where the sword disappears
		private void UnwindStrike() 
        {
			Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / slowTime);
			Size = 1f - MathHelper.SmoothStep(0, 1, Timer / slowTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

			if (Timer >= slowTime) 
            {
				CurrentStage = AttackStage.Pose;
                //Owner.GetModPlayer<SanchoModPlayer>().isPosed = true;
			}
		}

        private void PoseAfterStrike()
        {
            if (Timer >= poseTime) 
            {
				Projectile.Kill();
                //Owner.GetModPlayer<SanchoModPlayer>().isPosed = false;
			}
        }
    }
}