using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using SanchoLanceMod.Content.Weapons;
using SanchoLanceMod.Common;
using Humanizer;
using System.Numerics;
using ReLogic.Content.Sources;

using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SanchoLanceMod.Common
{
	/// <summary>
    /// UIElement to display a hardblood resource bar when La Sangre is being held
    /// </summary>
	internal class SanchoHardbloodBar : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		public UIElement area; // Temporarily make public for the sake of fixing this
		private UIImage barFrame;
		private Color gradientA;
		private Color gradientB;

		public override void OnInitialize() 
        {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.
			area = new UIElement(); 
            area.Left.Set(500, 0f); // Dummy values. We set the position under the player when we load the UIsystem
            area.Top.Set(40, 0f); 
			area.Width.Set(40, 0f); 
			area.Height.Set(20, 0f);

			barFrame = new UIImage(ModContent.Request<Texture2D>("SanchoLanceMod/Common/temphardbloodbar")); // Frame of our resource bar
			barFrame.Left.Set(22, 0f);
			barFrame.Top.Set(0, 0f);
			barFrame.Width.Set(36, 0f);
			barFrame.Height.Set(16, 0f);

			gradientA = new Color(131, 0, 70); // A dark purple
			gradientB = new Color(255, 73, 73); // Bright red

			area.Append(barFrame);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) 
		{
			if (Main.LocalPlayer.HeldItem.ModItem is not SanchoLance 
                && Main.LocalPlayer.HeldItem.ModItem is not SanchoLanceEnhanced)
			{  
                return; 
            }

			base.Draw(spriteBatch);
		}

		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) 
        {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<SanchoModPlayer>();
			// Calculate quotient
			float quotient = (float)modPlayer.hardblood / SanchoModPlayer.HARDBLOOD_MAX; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.

			// Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
			Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 6;
			hitbox.Width -= 12;
			hitbox.Y += 6;
			hitbox.Height -= 12;

			// Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps = (int)((right - left) * quotient);
			for (int i = 0; i < steps; i += 1) 
            {
				// float percent = (float)i / steps; // Alternate Gradient Approach
				float percent = (float)i / (right - left);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
			}
		}
        
		// Helper method to place the bar under the player
		public void SetPosition(Vector2 v)
		{
            Vector2 playerScreenPosition = Main.LocalPlayer.Center - Main.screenPosition;
            Main.NewText("Screen Position " + playerScreenPosition.X + ", " + playerScreenPosition.Y);
			area.Left.Set(playerScreenPosition.X - 6f, 0f); // Align the bar with the player
			area.Top.Set(playerScreenPosition.Y - 20f, 0f); // Placing it just a bit below the player
		}
	}

	// This class will only be autoloaded/registered if we're not loading on a server
	[Autoload(Side = ModSide.Client)]
	internal class ExampleResourceUISystem : ModSystem
	{
		private UserInterface HardbloodBarUserInterface;
		internal SanchoHardbloodBar HardbloodBar;
		public static LocalizedText ExampleResourceText { get; private set; }

		public override void Load() 
        {
			HardbloodBar = new();
			HardbloodBarUserInterface = new();
			HardbloodBarUserInterface.SetState(HardbloodBar);
		}

        /*public override void OnWorldLoad()
        {
            //HardbloodBar.SetPosition(new Vector2()); //TODO: I have no idea why this is so finnicky
            base.OnWorldLoad();
        }

        // Event handling to keep the bar under the player when changing the screen size
        public override void OnModLoad()
        {
			Main.OnResolutionChanged += HardbloodBar.SetPosition;
            
            base.OnModLoad();
        }

		public override void OnModUnload()
        {
			Main.OnResolutionChanged -= HardbloodBar.SetPosition;
            base.OnModUnload();
        }*/
 
		public override void UpdateUI(GameTime gameTime) 
        {
			HardbloodBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) 
        {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) 
            {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer
                (
					"SanchoLanceMod: Hardblood Bar",
					delegate 
                    {
						HardbloodBarUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}