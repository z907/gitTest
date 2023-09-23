using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	[Serializable]
	public class Order
	{		
		public OrdType type;
		public Building Target;
		public Resource res;
		public int xDest, yDest;
		public int prodUnit;
		public int estTime;
		public bool inited = false;
		public float wayX;
		public float wayY;
		public float koefX;
		public float koefY;
		public Order(Order ord)
		{
			this.type = ord.type;
			this.Target = ord.Target;
			this.res = ord.res;
			this.xDest = ord.xDest;
			this.yDest = ord.yDest;
			this.prodUnit = ord.prodUnit;

			this.estTime = ord.estTime;
		}
		public Order(OrdType t,int x,int y )
		{			
			type = t;
			xDest = x;
			yDest = y;
			Target = null;
			res = null;

			estTime = 0;
		}
		public Order(OrdType t,int time)
		{
			type = t;
			xDest = 0;
			yDest = 0;
			Target = null;
			res = null;

			estTime = time;
		}
		public Order(OrdType t,Building target)
		{
			type = t;
			xDest = 0;
			yDest = 0;
			Target = target;
			res = null;

			estTime = 0;
		}
		public Order(OrdType t,Resource target)
		{

			type = t;
			xDest = 0;
			yDest = 0;
			Target = null;
			res = target;

			estTime = 150;
		}
		public Order(OrdType t, Keys key)
		{
			type = t;
			xDest = 0;
			yDest = 0;
			Target = null;
			res = null;
			prodUnit = (int)key - 49;
			estTime = 0;
		}
		public Order(OrdType t, Keys key, int x, int y)
		{
			type = t;
			xDest = x;
			yDest = y;
			Target = null;
			res = null;
			prodUnit = (int)key - 49;
			estTime = 0;
		}

	}
}
