using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ahjas
{

	public class Game3 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont Font;
		bool win;
		public Game3(bool win)
		{
			this.win = win;
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		protected override void Initialize()
		{
			base.Initialize();
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 300;
			graphics.PreferredBackBufferHeight = 200;
			graphics.ApplyChanges();
		}

		protected override void LoadContent()
		{

			spriteBatch = new SpriteBatch(GraphicsDevice);
			Font = Content.Load<SpriteFont>("resFont");

		}

		protected override void Update(GameTime gameTime)
		{

#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.LightBlue);
			spriteBatch.Begin();
			string text;
			if (win) text = "Победа";
			else text = "Поражение";
			spriteBatch.DrawString(Font, text,
		 new Vector2(150-text.Length/2*10, 80), Color.Black);
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}

