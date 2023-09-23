using System;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	[Serializable]
	public class Panel
	{
		public int width;
		int x;
		int y;
		int height;
		public int getHeight()
		{
			return height;
		}

		List<Building> uList;
		[NonSerialized]Texture2D mainRect;
		public Panel(Game1 g)
		{ 
			uList = new List<Building>();
			mainRect = g.Content.Load<Texture2D>("black");
		}
		public void updateAfterLoad(Game1 g)
		{
			mainRect=g.Content.Load<Texture2D>("black");
		}
		public void updateSize(int x,int y)
		{
			width = x;
			this.x = 0;
			height = (int)(y * 0.3);
			this.y = y - height;
		}
		public void updateSelection(List<Building> selUnits)
		{
			uList = new List<Building>(selUnits);
		}
		public void draw(SpriteFont font)
		{
			Game1.sprBatch.Begin();
			Game1.sprBatch.Draw(mainRect,new Rectangle(x,y,width,height),Color.Black);
			if (uList.Count == 1)
			{
				uInfo data = UnitCatalog.getInfo(uList[0].getName());
				Game1.sprBatch.Draw(uList[0].getSprite(),
				                   new Rectangle((int)(x + width * 0.3), y + 50, 128, 128),
				new Rectangle(0, 0, data.FrameSize.X,data.FrameSize.Y),Color.White);

				for (int i=0; i < 5; i++)
				{
					if (data.prodOptions[i] != "") Game1.sprBatch.DrawString(font,(i+1).ToString()+"."+ data.prodOptions[i] +"   " + UnitCatalog.getCost(data.prodOptions[i]).ToString(), new Vector2((float)(width*0.75), y + 50 + i * 20),Color.LimeGreen);
				}
				int kx = 0;
				int ky = 0;
				Game1.sprBatch.DrawString(font, uList[0].getName(), new Vector2((float)(width * 0.5 + 50 * kx), y + 40 + ky * 20), Color.LimeGreen);
				ky++;
				Game1.sprBatch.DrawString(font,"Health: "+uList[0].getHP().ToString()+"/"+data.maxhp.ToString() , new Vector2((float)(width * 0.5+50*kx), y+40+ky*20), Color.LimeGreen);
				ky++;
				Game1.sprBatch.DrawString(font, "Armor: " +data.armor.ToString(), new Vector2((float)(width * 0.5 + 50 * kx), y + 40 + ky * 20), Color.LimeGreen);
				ky++;
				if(data.damage!=0)Game1.sprBatch.DrawString(font, "Damage: " + data.damage.ToString(), new Vector2((float)(width * 0.5 + 50 * kx), y + 40 + ky * 20), Color.LimeGreen);
				ky++;
				string a;
				int b, c;
				uList[0].CheckProd(out  a,out  b,out  c);
				if (a!="") Game1.sprBatch.DrawString(font, a+"   " +(b/60).ToString()+"/"+(c/60).ToString(), new Vector2((float)(width * 0.5 + 50 * kx), y + 40 + ky * 20), Color.LimeGreen);

			}
			if (uList.Count>1)
			{
				int col = 0;
				int row = 0;
				foreach (var un in uList)
				{
					
					Game1.sprBatch.Draw(un.getSprite(), new Rectangle(x + width / 4 + 80 * row, y + 50 + 80 * col, 64, 64),new Rectangle(0, 0, un.getFrameSize().X,un.getFrameSize().Y), Color.White);
					row++;
					if (row == 8)
					{
						row = 0;
						col++;
					}
					if (col == 3) break;
				}
			}


			Game1.sprBatch.End();
		}
	}
}
