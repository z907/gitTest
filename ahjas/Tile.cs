using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	[Serializable]
	public class Tile
	{
		
		private bool type;
		public Tile(bool TType)
		{
			type = TType;
		}
		public void draw(ref Texture2D brt,ref Texture2D dbrt,int xpos, int ypos)
		{
			
			var xy = new Point(xpos, ypos);
			Point sz = new Point(32, 32);
			if (type) Game1.sprBatch.Draw(brt, new Rectangle(xy, sz), Color.White);
			else Game1.sprBatch.Draw(dbrt, new Rectangle(xy, sz), Color.White);
		}
	}
}
