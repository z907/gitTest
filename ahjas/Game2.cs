using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	public delegate void OnClick(string butName);
	public class Button
	{
		OnClick clicked;
		string name;
		int x, y;
		int sizeX, sizeY;
		public Button(string name,int sizex,int sizey,OnClick click)
		{
			this.name = name;
			sizeX = sizex;
			sizeY = sizey;
			clicked = click;

		}
		public void draw(int x,int y,int n)
		{
			this.x = x/2-sizeX/2;
			this.y = 50+n*100;
			Game2.spriteBatch.Begin();
			Game2.spriteBatch.Draw(Game2.mainTexture,new Rectangle(this.x,this.y,sizeX,sizeY),new Rectangle(512,512, 1, 1),Color.LimeGreen);
			Game2.spriteBatch.DrawString(Game2.Font, name,
			 new Vector2(this.x+sizeX/4, this.y+sizeY/3), Color.Black);
			Game2.spriteBatch.End();

		}
		public void checkClick(int cx, int cy)
		{
			if (this.x < cx && (this.x + sizeX) > cx && this.y < cy && this.y + sizeY > cy)
			{
				clicked(name);
			}
		}
	}
	public class Game2:Game
	{
		public static SpriteFont Font;
		GraphicsDeviceManager graphics;
		public static SpriteBatch spriteBatch;
		public bool newGame, loadGame, showHelp, showAbout;
		public static Texture2D mainTexture;
		Button CloseButton;
		List<Button> bList;
		bool lClick = false;
		public Game2()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			bList = new List<Button>();
			Button bt;
			bt= new Button("New game", 200, 50, clicked);
			bList.Add(bt);
			bt = new Button("Load game", 200, 50, clicked);
			bList.Add(bt);
			bt = new Button("Help", 200, 50, clicked);
			bList.Add(bt);
			bt = new Button("About", 200, 50, clicked);
			bList.Add(bt);
			CloseButton = new Button("Close", 200, 50, clicked);

			//buttons
			newGame = false;
			loadGame= false;
			showHelp= false;
			showAbout= false;
			base.Initialize();
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
			Window.AllowUserResizing = true;
		}

		public void clicked(string name)
		{
			
			switch (name)
			{
				case "New game":
					Program.NewGame = true;
					Exit();
					break;
				case "Load game":
					Program.NewGame = false;
					Exit();
					break;
				case "Help": showHelp = true;
					break;
				case "About":showAbout = true;
					break;
				case "Close":showHelp = false;
					showAbout = false;
					break;
			}
		}
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Font=Content.Load<SpriteFont>("resFont");
			mainTexture = Content.Load<Texture2D>("circle");
		}

		void checklClick()
		{
			MouseState ms = Mouse.GetState();
			if (ms.LeftButton == ButtonState.Pressed && !lClick)
			{
				lClick = true;
				return;
			}
			if (ms.LeftButton == ButtonState.Released && lClick)
			{
				lClick = false;
				if (newGame || loadGame || showHelp || showAbout) CloseButton.checkClick(ms.X, ms.Y);
				else
				{
					foreach (var b in bList)
					{
						b.checkClick(ms.X, ms.Y);
					}
				}
			}
		}
		protected override void Update(GameTime gameTime)
		{
			
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			checklClick();
			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			

			graphics.GraphicsDevice.Clear(Color.LightBlue);
			if (showHelp)
			{
				spriteBatch.Begin();
				spriteBatch.DrawString(Font, "Чтобы отдать приказ юнитам,выделите его, кликнув на него\n или зажмите и проведите мышью чтобы выбрать несколько\n" +
				                       "Правая кнопка по пустому месту - двигаться\nПравая кнопка по противнику - атаковать\n" +
				                       "Английская А(Русская Ф) + левая кнопка мыши - двигаться\nв точку и атаковать встреченных противников\n" +
				                       "Правая кнопка по ресурсу выделив рабочего - добывать\nЦифра из списка на панели - производить или строить\n" +
				                       "В правом верхнем углу экрана - кол-во ресурсов\nЗдания буду строиться в месте где находится мышка\n" +
				                       "в момент отдачи приказа\n" +
				                       "Тот, у кого не осталось ни одного здания,проигрывает\n" +
				                       "Синие клетки непроходимы\n" +
				                       "S-сохранение\n" +
				                       "Чтобы переместить камеру, наведитес мышкой в область у соответствующего края экрана", new Vector2(300,50), Color.Black);
				spriteBatch.End();
				CloseButton.draw(300, 0, 1);

				return;
			}
			if (showAbout)
			{
				spriteBatch.Begin();
				spriteBatch.DrawString(Font, "Лучшая стратегия в реальном времени,\nкоторая совсем не похожа на Warcraft II\n\n" +
			     "Программа представляет из себя игру в жанре RTS\n\nАвтор: Игнатенко И.С. 2-42В", 
		     new Vector2(300, 50), Color.Black);
				spriteBatch.End();
				CloseButton.draw(300, 0, 1);
			
				return;
			}
			int i = 0;
			foreach (var b in bList)
			{
				b.draw(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, i);
				 i++;
			}
			base.Draw(gameTime);
		}

	}
}
