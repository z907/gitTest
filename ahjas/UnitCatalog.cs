using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	[Serializable]
	public class uInfo
	{
		public uInfo(string Name, int Size, int Maxhp, int Armor, int Range, int Attsp, int Movesp, int Damage, int Type, int time, int cost, string option1, string option2, string option3, string option4, string option5, int fsx, int fsy, int fcx, int fcy)
		{
			name = Name;
			size = Size;
			maxhp = Maxhp;
			armor = Armor;
			range = Range;
			attsp = Attsp;
			movesp = Movesp;
			damage = Damage;
			type = Type;
			prodTime = time;
			prodOptions = new string[5];
			Cost = cost;
			prodOptions[0] = option1;
			prodOptions[1] = option2;
			prodOptions[2] = option3;
			prodOptions[3] = option4;
			prodOptions[4] = option5;
			FrameSize = new Point(fsx, fsy);
			FrameCount = new Point(fcx, fcy);
		}
		public string name;
		public int size;
		public int maxhp;
		public int armor;
		public int range;
		public int attsp;
		public int movesp;
		public int damage;
		public int type;
		public int prodTime;
		public int Cost;
		public string[] prodOptions;
		public Texture2D sprite;
		public Point FrameSize;
		public Point FrameCount;
	}
	[Serializable]
	public static class UnitCatalog
	{
		static List<uInfo> Catalog;
		static UnitCatalog()
		{
			Catalog = new List<uInfo>();
		}
		public static void add(uInfo toAdd)
		{
			Catalog.Add(toAdd);
		}
		public static void addSprite(string name, Texture2D img)
		{
			foreach (var inf in Catalog)
			{
				if (name == inf.name)
				{
					inf.sprite = img;
					return;
				}
			}
		}
		public static void init()
		{
			uInfo newUnit = new uInfo("Knight", 32, 150, 3, 5, 60, 40, 25, 2, 1800, 250, "", "", "", "", "", 60, 60, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Archer", 24, 100, 0, 96, 45, 45, 15, 2, 1200, 200, "", "", "", "", "", 56, 56, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Worker", 24, 50, 0, 1, 60, 50, 5, 3, 900, 100, "Citadel", "Barracks", "Workshop", "", "", 40, 40, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Citadel", 96, 1000, 3, 0, 0, 0, 0, 1, 3600, 1000, "Worker", "", "", "", "", 140, 140, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Barracks", 64, 700, 3, 0, 0, 0, 0, 1, 1800, 500, "Knight", "Archer", "", "", "", 105, 105, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Workshop", 64, 750, 3, 0, 0, 0, 0, 1, 2400, 750, "Catapult", "", "", "", "", 105, 105, 16, 4);
			UnitCatalog.add(newUnit);
			newUnit = new uInfo("Catapult", 48, 250, 5, 192, 200, 30, 65, 2, 2100, 800, "", "", "", "", "", 64, 64, 16, 4);
			UnitCatalog.add(newUnit);
		}
		public static Point updateFcount(string name)
		{
			foreach (var inf in Catalog)
			{
				if (name == inf.name) return inf.FrameCount;
			}
			return new Point(0, 0);
		}
		public static Point updateFsize(string name)
		{
			foreach (var inf in Catalog)
			{
				if (name == inf.name) return inf.FrameSize;
			}
			return new Point(0, 0);
		}
		public static Texture2D updateSprite(string name)
		{
			foreach (var inf in Catalog)
			{
				if (name == inf.name) return inf.sprite;
			}
			return null;
		}
		public static uInfo getInfo(string name)
		{
			foreach (var inf in Catalog)
			{
				if (name == inf.name) return inf;
			}
			return null;
		}
		public static int getCost(string name)
		{
			foreach (var x in Catalog)
			{
				if (x.name == name) return x.Cost;
			}
			return 0;
		}
		public static int getTime(string name)
		{
			foreach (var x in Catalog)
			{
				if (x.name == name) return x.prodTime;
			}
			return 0;
		}
	}
}
public class creatingData
{
	public int X, Y;
	public string name;
	public bool preplaced;
	public creatingData(string s, int x, int y, bool prepl)
	{
		preplaced = prepl;
		name = s;
		X = x;
		Y = y;
	}
	public creatingData()
	{
	}
}