using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace ahjas
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	/// АААААААА НЕГРЫЫЫЫЫЫЫыыы
	public delegate bool CheckForSpec(int x, int y, int dist, string name);
	[Serializable]
	public class Game1 : Game
	{
		bool Loaded;
		public GraphicsDeviceManager graphics;
		public static SpriteBatch sprBatch;

		public delegate void CamMove(int Dir);
		event CamMove CameraMoved;
		public delegate void click(ClickData data);
		event click Clicked;
		public delegate void keyPress(Keys key,int x,int y);
		event keyPress keyPressed;

		public static Texture2D BrownTile;
		public static Texture2D BlueTile;
		public static Texture2D Circle;
		public static Texture2D selRect;

		public bool aFlag;

		public bool LButtonPressed;
		public bool RButtonPressed;
		public int click_duration;
		public ClickData data;
		public Field MainField;
		public int xDelt, yDelt;
		public int ScrollSpeed;
		public Rectangle selectionRect;
		public MouseState ms;

		public static Human player;
		public static Bot bot;
		public static ResourcePlayer neutral;

		public Game1(bool load)
		{
			Loaded = load;
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}


		protected override void Initialize()
		{
			if (!Loaded)
			{
				neutral = new ResourcePlayer();
				player = new Human(this);
				bot = new Bot();
			}
			else
			{
				
				BinaryFormatter formatter = new BinaryFormatter();
				using (FileStream fs = new FileStream("res.dat", FileMode.OpenOrCreate))
				{
					neutral = (ResourcePlayer)formatter.Deserialize(fs);
				}
			   using (FileStream fs = new FileStream("player.dat", FileMode.OpenOrCreate))
				{
					player = (Human)formatter.Deserialize(fs);
				}
			 using (FileStream fs = new FileStream("bot.dat", FileMode.OpenOrCreate))
				{
					bot = (Bot)formatter.Deserialize(fs);

				}

					             	
			}
			MainField = new Field(64, 64);
			xDelt = 0;
			yDelt = 0;
			graphics.IsFullScreen = false;
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += OnResize;
			graphics.ApplyChanges();
			ScrollSpeed = 5;

			this.CameraMoved += this.MoveCam;
			this.Clicked += player.handleClick;
			this.keyPressed += player.handleKey;

			UnitCatalog.init();
			base.Initialize();
			data = new ClickData();
			aFlag = false;

			LButtonPressed = false;
			RButtonPressed = false;
			click_duration = 0;
			player.bot = bot;
			selectionRect = new Rectangle();
			ms = new MouseState();
			if(!Loaded)preplaceUnits();
			if (Loaded) updatePlayersAfterLoad();
			bot.freeRes = ResourcePlayer.findResourcesInRange(1748, 1748, 320);

		}

		void updatePlayersAfterLoad()
		{
			neutral.updateAterLoad();
		}
		protected override void LoadContent()
		{

			sprBatch = new SpriteBatch(GraphicsDevice);

			base.LoadContent();

			Circle = Content.Load<Texture2D>("circle");
			UnitCatalog.addSprite("Citadel",Content.Load<Texture2D>("Citadel"));
			UnitCatalog.addSprite("Workshop", Content.Load<Texture2D>("mach"));
			UnitCatalog.addSprite("Barracks", Content.Load<Texture2D>("Barracks"));
			UnitCatalog.addSprite("Catapult", Content.Load<Texture2D>("catapult"));
			UnitCatalog.addSprite("Archer", Content.Load<Texture2D>("archer"));
			UnitCatalog.addSprite("Knight", Content.Load<Texture2D>("knight"));
			UnitCatalog.addSprite("Worker", Content.Load<Texture2D>("worker"));
			ProjectileManager.addProjectileType("Archer", new Projectile(0, 0, 0, 200, Content.Load<Texture2D>("arch_proj"),null, 24));
			ProjectileManager.addProjectileType("Catapult", new Projectile(0, 0, 0, 150, Content.Load<Texture2D>("cat_proj"), null, 24));
			Resource.resTexture = Content.Load<Texture2D>("minerals1");



			if(Loaded)updateTextures();




			Human.selectionCircle = Content.Load<Texture2D>("selection");
			selRect = Content.Load<Texture2D>("selRect");
			Human.resFont = Content.Load<SpriteFont>("resFont");
			BrownTile = Content.Load<Texture2D>("ground");
			BlueTile = Content.Load<Texture2D>("water");


		}
		void updateTextures()
		{
			player.updateAfterLoad();
			bot.updateAfterLoad();
			player.infPanel.updateAfterLoad(this);
		}
		private void preplaceUnits()
		{
			placeResources();
			creatingData args = new creatingData();
			StreamReader fs = new StreamReader("preplacedUnits.txt", System.Text.Encoding.UTF8);
			string line;
			string[] lines;
			while(!fs.EndOfStream)
			{
				line = fs.ReadLine();
				if (line == "") break;
				lines = line.Split();
				args.name = lines[0];
				args.X = int.Parse(lines[1]);
				args.Y= int.Parse(lines[2]);
				args.preplaced = int.Parse(lines[3]) != 0;
				player.createUnit(null, args);
			}
			fs = new StreamReader("preplacedUnitsBot.txt", System.Text.Encoding.UTF8);
			while (!fs.EndOfStream)
			{
				line = fs.ReadLine();
				if (line == "") break;
				lines = line.Split();
				args.name = lines[0];
				args.X = int.Parse(lines[1]);
				args.Y = int.Parse(lines[2]);
				args.preplaced = int.Parse(lines[3]) != 0;
				bot.createUnit(null, args);
			}

		}
		protected override void UnloadContent()
		{
			base.UnloadContent();
			sprBatch.Dispose();
			BrownTile.Dispose();
			BlueTile.Dispose();
		}


		protected override void Update(GameTime gameTime)
		{
			ms = Mouse.GetState();
			this.CheckScroll();
			CheckKeyboard();
			CheckClicks();
		
			player.ordExec();
			bot.ordExec();
			ProjectileManager.moveProj();
			bot.Think();
			checkWinningConditions();
			base.Update(gameTime);
		}

		protected void checkWinningConditions()
		{
			if (player.BuildingCount == 0)
			{
				Program.win = false;
				Exit();
			}
			else if (bot.BuildingCount == 0)
			{
				Program.win = true;
				Exit();
			}

		}

		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.White);

			MainField.draw(ref BrownTile, ref BlueTile, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, xDelt, yDelt);
			neutral.draw(xDelt, yDelt, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			bot.draw(xDelt,yDelt, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			drawSelRect();

			player.draw(xDelt, yDelt, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			base.Draw(gameTime);
		}

		protected void CheckKeyboard()
		{
			if (Keyboard.GetState().IsKeyDown(Keys.S))
			{
				Save();
			}
			if (Keyboard.GetState().IsKeyDown(Keys.A) && !LButtonPressed) aFlag = true ;
			for (Keys k=Keys.D1; k != Keys.D6;k++)
				if (Keyboard.GetState().IsKeyDown(k)) keyPressed(k,ms.X+xDelt,ms.Y+yDelt);

			if (Keyboard.GetState().IsKeyDown(Keys.Escape)) aFlag = false;
		}
		protected void Save()
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (FileStream fs = new FileStream("player.dat", FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, player);
			}
			using (FileStream fs = new FileStream("bot.dat", FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, bot);
			}
			using (FileStream fs = new FileStream("res.dat", FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs,neutral);
			}

		}
		protected void CheckClicks()
		{
			if (ms.RightButton == ButtonState.Pressed && !LButtonPressed && !aFlag) 
				RButtonPressed = true;
			if (ms.RightButton == ButtonState.Released && RButtonPressed)
			{
				data.leftRight = false;
				data.startX = ms.X+ xDelt;
				data.startY = ms.Y+ yDelt;
				data.endX = 0;
				data.endY = 0;
				data.AFlag = false;
				data.duration = 0;
				RButtonPressed = false;
				click_duration = 0;
				Clicked.Invoke(data);
				aFlag = false;
				return;
			}
			if (ms.LeftButton == ButtonState.Pressed)
			{
				if (LButtonPressed) click_duration++;
				else
				{
					data.startX = ms.X+ xDelt;
					data.startY = ms.Y+ yDelt;
					LButtonPressed = true;
				}
			}
			if (ms.LeftButton == ButtonState.Released && LButtonPressed)
			{
				LButtonPressed = false;
				data.AFlag = aFlag;
				data.endX = ms.X+ xDelt;
				data.endY = ms.Y+ yDelt;
				data.leftRight = true;
				data.duration = click_duration;
				click_duration = 0;
				Clicked.Invoke(data);
				aFlag = false;
				return;
			}
		}

		protected void CheckScroll()
		{
			if (ms.X > GraphicsDevice.Viewport.Width - 40 && ms.X < GraphicsDevice.Viewport.Width && ms.Y < GraphicsDevice.Viewport.Height && ms.Y >0) CameraMoved?.Invoke(1);
			if (ms.X < 40 && ms.X > 0 && ms.Y < GraphicsDevice.Viewport.Height  && ms.Y > 0) CameraMoved?.Invoke(2);
			if (ms.Y > GraphicsDevice.Viewport.Height-40  && ms.Y < GraphicsDevice.Viewport.Height  && ms.X < GraphicsDevice.Viewport.Width && ms.X > 0 ) CameraMoved?.Invoke(3);
			if (ms.Y < 40 && ms.Y > 0 && ms.X < GraphicsDevice.Viewport.Width && ms.X > 0) CameraMoved?.Invoke(4);
		}


		protected void MoveCam(int Direction)
		{
			switch (Direction)
			{
				case 1:
					xDelt += ScrollSpeed;
					break;
				case 2:
					xDelt -= ScrollSpeed;
					break;
				case 3:
					yDelt += ScrollSpeed;
					break;
				case 4:
					yDelt -= ScrollSpeed;
					break;
				default: break;
			}
			checkDelts();
		}
		public void OnResize(Object sender,EventArgs e)
		{
			checkDelts();
		}

		protected void checkDelts()
		{
			if (xDelt >= 64 * 32 - GraphicsDevice.Viewport.Width) xDelt = 64*32 - GraphicsDevice.Viewport.Width;
			if (xDelt <= 0) { xDelt = 0; }
			if (yDelt >= 64 * 32 - (GraphicsDevice.Viewport.Height- player.infPanel.getHeight())) yDelt = 64*32 - (GraphicsDevice.Viewport.Height - player.infPanel.getHeight());
			if (yDelt <= 0) { yDelt = 0; }
		}
		protected void placeResources()
		{
			StreamReader fs = new StreamReader("Resources.txt", System.Text.Encoding.UTF8);
			string line;
			string[] lines;
			while (!fs.EndOfStream)
			{
				line = fs.ReadLine();
				if (line == "") break;
				lines = line.Split();
				neutral.createRes(int.Parse(lines[0]), int.Parse(lines[1]), int.Parse(lines[2]));
				neutral.createRes(2048-int.Parse(lines[0]), 2048-int.Parse(lines[1]), int.Parse(lines[2]));
			}

		}
		protected void drawSelRect()
		{
			if (LButtonPressed && click_duration>20)
			{
				sprBatch.Begin();
				selectionRect.X = Math.Min(data.startX-xDelt, ms.X);
				selectionRect.Y = Math.Min(data.startY-yDelt, ms.Y);
				selectionRect.Height = Math.Abs(data.startY-yDelt - ms.Y);
				selectionRect.Width = Math.Abs(data.startX-xDelt - ms.X);
				sprBatch.Draw(selRect, selectionRect, Color.White);
				sprBatch.End();
			}
		}
		public static Building Scan(owner own,int x,int y,int dist)
		{
			Building targ=null;
			if (own == owner.Bot)
			{
				targ = player.findClosest(x, y, dist);
			}
			else
			{
				targ = bot.findClosest(x, y, dist);
			}
			return targ;
		}
		public static Point checkCollision(Building un,int posX,int posY,int size)
		{
			Point c1 = player.CheckCollision(un,posX, posY, size);
			if (c1.X > 0) return c1;
			Point c2 = bot.CheckCollision(un,posX, posY, size);
			if (c2.X > 0) return c2;
			Point c3=neutral.CheckCollision(un,posX, posY, size);
			if (c3.X > 0) return c3;
			return new Point(-1, -1);
		}
	}
}
