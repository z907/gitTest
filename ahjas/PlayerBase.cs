using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{

	[Serializable]
	public class PlayerBase
	{
		protected owner Owner;
		public List<Building> UnitList;
		public List<Building> SelectedList;
		public List<Building> tempList;
		public List<Building> tempDelList;
		[NonSerialized] protected Random randomizer;
		public int BuildingCount{get
			{
				int n=0;;
				foreach (var un in UnitList)
					if (!(un is Worker) && !(un is Unit)) n++;
				return n;
			}
		}
		public int resCount;
		public CheckForSpec checkForUnit;
		[NonSerialized]protected   Point[] deltMatrix;
		protected bool checkForSpecUnit(int x, int y, int range, string name)
		{
			foreach (var u in UnitList)
			{
				if (u.getName() == name) return true;
			}
			return false;
		}
		public PlayerBase()
		{
			UnitList = new List<Building>();
			SelectedList = new List<Building>();
			tempList=new List<Building>();
			tempDelList = new List<Building>();
			checkForUnit = checkForSpecUnit;
			 randomizer=new Random();
			deltMatrix = new Point[9];
			deltMatrix[0] = new Point(0, 0);
			deltMatrix[1] = new Point(1, 0);
			deltMatrix[2] = new Point(0, 1);
			deltMatrix[3] = new Point(-1, 0);
			deltMatrix[4] = new Point(0, -1);
			deltMatrix[5] = new Point(1, 1);
			deltMatrix[6] = new Point(1, -1);
			deltMatrix[7] = new Point(-1, 1);
			deltMatrix[8] = new Point(-1, -1);
		}
		public virtual void updateAfterLoad()
		{
			randomizer = new Random();
			deltMatrix = new Point[9];
			deltMatrix[0] = new Point(0, 0);
			deltMatrix[1] = new Point(1, 0);
			deltMatrix[2] = new Point(0, 1);
			deltMatrix[3] = new Point(-1, 0);
			deltMatrix[4] = new Point(0, -1);
			deltMatrix[5] = new Point(1, 1);
			deltMatrix[6] = new Point(1, -1);
			deltMatrix[7] = new Point(-1, 1);
			deltMatrix[8] = new Point(-1, -1);
			foreach (var un in UnitList)
			{
				un.UpdateAfterLoad();
			}
		}
		public Point CheckCollision(Building unit,int posX,int posY, int size)
		{
			foreach (var un in UnitList)
			{
				double dist = un.mesDist(posX, posY);
				if (dist < size + un.getSize() && unit != un)
					return new Point(un.absX, un.absY);
			}
			return new Point(-1,-1);
		}
		public virtual bool resChange(int n, bool IncDec)
		{
			if (IncDec) resCount += n;
			else
			{				
				if (resCount - n < 0)
				{
					return false;
				}
				else
				{
					resCount -= n;
					return true;
				}
			}
			return true;
		}

		public Building findUnit(int x, int y)
		{
			foreach (var un in UnitList)
			{
				if (un.mesDist(x, y) < un.getSize()) return un;
			}
			return null;
		}

		public virtual void draw(int dx,int dy,int scrX,int scrY)
		{
			foreach (var unit in UnitList)
			{
				Game1.sprBatch.Begin();
				unit.draw(dx,dy,scrX,scrY);
				Game1.sprBatch.End();
			}
		}
		public virtual void delUnit(Building destroyedUnit)
		{
			tempDelList.Add(destroyedUnit);
		}

		protected void giveOrder(Order ord, bool onlyFirst)
		{
			Order nOrd;
			int i = 0;
			foreach (var unit in SelectedList)
			{
				nOrd = new Order(ord);
				if (ord.type == OrdType.Find || ord.type == OrdType.AttackMove)
				{
					nOrd.xDest += (deltMatrix[i].X * (unit.getSize() + 96));
					nOrd.yDest += (deltMatrix[i].Y * (unit.getSize() + 96));
					i++;
				}
				unit.getOrder(nOrd, false);
				if(onlyFirst) return;
			}
		}

		protected void select(Building selUnit)
		{
			if (SelectedList.Count == 9) return;
			SelectedList.Add(selUnit);
			selUnit.select();

		}
		protected void unselect(Building selUnit)
		{
			SelectedList.Remove(selUnit);
			selUnit.unselect();
		}

		public virtual void createUnit(Building creator,creatingData args)
		{
			uInfo data;
			data = UnitCatalog.getInfo(args.name);
			Building createdUnit;

			switch (data.type)
			{
				case 1:
					createdUnit = new Building(Owner,createUnit,delUnit,resChange, data, args.X, args.Y,args.preplaced);
					break;
				case 2:
					createdUnit = new Unit(Owner,createUnit, delUnit, resChange, data, args.X, args.Y, args.preplaced);
					break;
				case 3:
					createdUnit = new Worker(Owner,checkForSpecUnit,createUnit, delUnit, resChange, data, args.X, args.Y, args.preplaced);
					break;
				default:
					return;
			}
			if (!args.preplaced) 
				creator.startBuild(createdUnit);
			tempList.Add(createdUnit);
		}
		public void ordExec()
		{
			foreach (var unit in UnitList)
			{
				unit.OrderExec();
			}
			UnitList.AddRange(tempList);
			tempList.Clear();
			foreach (var unit in tempDelList)
			{
				UnitList.Remove(unit);
				SelectedList.Remove(unit);
				unit.endAttack(null);
			}
			tempDelList.Clear();
		}
		public Building findClosest(int x, int y, int dist)
		{
			double mindist = dist;
			Building foundUnit=null;
			foreach (var unit in UnitList)
			{
				if (unit.mesDist(x, y) < dist)
				{
					mindist = unit.mesDist(x, y);
					foundUnit = unit;
				}
			}
			return foundUnit;
		}

	}
	[Serializable]
	public class ResourcePlayer 
	{
		public  List<Resource> resList;
		private static List<Resource> stResList;
		public void updateAterLoad()
		{
			stResList = resList;
		}
		public ResourcePlayer()
		{
			resList = new List<Resource>();
			stResList = resList;
		}
		public void draw(int dx,int dy, int scrX, int scrY)
		{
			Game1.sprBatch.Begin();
			foreach (var res in resList)
			{
				res.draw(dx,dy,scrX,scrY);
			}
			Game1.sprBatch.End();
		}
		public void delRes(Resource destroyedUnit)
		{
			resList.Remove(destroyedUnit);
		}

		public void createRes(int x,int y,int dur)
		{
			Resource newRes = new Resource(x,y,dur,this);
			resList.Add(newRes);

		}
		public static Resource findResource(int x, int y)
		{
			foreach (var rs in stResList)
			{
				if (rs.mesDist(x, y) < 16) return rs;
			}
			return null;
		}
		public static List<Resource> findResourcesInRange(int x, int y,int range)
		{
			List<Resource> rsList = new List<Resource>();
			foreach (var rs in stResList)
			{
				if (rs.mesDist(x, y) < range && !rs.busy) rsList.Add(rs);
			}
			return rsList;
		}
		public Point CheckCollision(Building unit,int posX,int posY, int size)
		{
			foreach (var un in stResList)
				if (un.mesDist(posX,posY) < size + un.size) return new Point(un.x, un.y);
			return new Point(-1, -1);
		}
	}


	[Serializable]
	public class Human : PlayerBase
	{
		[NonSerialized]public static Texture2D selectionCircle;
		public Panel infPanel;
		[NonSerialized]public static SpriteFont resFont;
		public PlayerBase bot;
		public Human(Game1 g):base()
		{
			Owner = owner.Player;
			resCount = 11250;
			infPanel = new Panel(g);
		}
		public override void draw(int dx, int dy, int scrX, int scrY)
		{
			infPanel.updateSize(scrX, scrY);
			infPanel.updateSelection(SelectedList);
			base.draw(dx, dy, scrX, scrY);
			drawRes(resCount,scrX,scrY);
			ProjectileManager.drawProj(dx, dy,scrX, scrY);
			infPanel.draw(resFont);
		}

		private void drawRes(int x,int scrX,int scrY)
		{
			Game1.sprBatch.Begin();
			Game1.sprBatch.DrawString(resFont, resCount.ToString(),new Vector2(scrX-100,30), Color.SkyBlue);
			Game1.sprBatch.End();
		}

		public void handleClick(ClickData data)
		{
			if (data.leftRight)
			{
				if (data.AFlag)
				{
					Order ord = new Order(OrdType.AttackMove, data.startX, data.startY);

						giveOrder(ord, false);

				}
				else
				{
					if (data.duration > 20)
					{
						Queue<Building> ls = findUnits(data.startX, data.startY, data.endX, data.endY);
						if (ls != null)
						{
							foreach (var un in SelectedList) un.unselect();
							SelectedList.Clear();

							foreach (var un in UnitList)
							{
								if (ls.Contains(un)) select(un);
							}
						}
					}
					else
					{
						Building un = findUnit(data.startX, data.startY);
						if (un != null)
						{
							foreach (var un1 in SelectedList) un1.unselect();
							SelectedList.Clear();
							select(un);
						}
					}

				}
			}
			else
			{
				Resource res =ResourcePlayer.findResource(data.startX, data.startY);
				if (res != null)
				{
					Order ord = new Order(OrdType.Gather, res);
					giveOrder(ord, true);
				}
				else
				{
					Building unit =bot.findUnit(data.startX,data.startY);
					if (unit != null)
					{
						Order ord = new Order(OrdType.Attack,unit);
						giveOrder(ord, false);
					}
					else
					{
						Order ord = new Order(OrdType.Find, data.startX, data.startY);
						giveOrder(ord, false);
					}
					}

			}
			//order and selection giving logic
		}

		public void handleKey(Keys key,int x,int y)
		{
			for (Keys k = Keys.D1; k != Keys.D6; k++)
			{
				if (key==k)
				{
					if (SelectedList.Count == 1)
					{
						
						Order ord = new Order(OrdType.Produce, key);
						giveOrder(ord, true);
						ord = new Order(OrdType.Construct, key,x,y);
						giveOrder(ord, true);
					}
				}
			}
		}

		public Queue<Building> findUnits(int sx,int sy,int ex,int ey)
		{
			Queue<Building> ls = new Queue<Building>();
			foreach (var un in UnitList)
			{
				if (un.absX > Math.Min(sx,ex) && un.absX < Math.Max(sx, ex) && un.absY > Math.Min(sy, ey) && un.absY < Math.Max(sy, ey)) ls.Enqueue(un);
			}
			if (ls.Count == 0) return null;
			return ls;
		}

	}
	[Serializable]
	public class Bot : PlayerBase
	{
		private int thinkCounter =0;
		private int phase = 0;
		List<Resource> busyRes;
		List<Building> attackForce;
		Building Builder;
		public List<Resource> freeRes;
		private Building findSpecInList(string name)
		{
			foreach (var un in UnitList)
			{
				if (un.getName() == name) return un;
			}
			return null;
		}
		public override void createUnit(Building creator, creatingData args)
		{
			base.createUnit(creator, args);
			if (phase == 2)
			{
				foreach (var un in tempList)
					if (!attackForce.Contains(un)) attackForce.Add(un);
			}
		}
		public override void delUnit(Building unit)
		{
			base.delUnit(unit);
			attackForce.Remove(unit);

		}
		private void updateNumbers()
		{
			workerCount = 0;
			unitCount = 0;
			haveCitadel = false;
			haveBarracks = false;
			haveWorkshop = false;
			foreach (var un in UnitList)
			{
				switch (un.getName())
				{
					case "Citadel":haveCitadel = true;
					break;
					case "Barracks":haveBarracks = true;
						break;
					case "Workshop":haveWorkshop = true;
						break;
					case "Worker":workerCount++;
						break;
				}
			}
			unitCount = attackForce.Count;

		}
		public Bot():base()
		{
			randomizer = new Random();
			Owner = owner.Bot;
			resCount = 250;
			busyRes = new List<Resource>();
			Builder = null;
			attackForce = new List<Building>();
		}
		private int nextRandomUnit = 1;
		private int workerCount = 4;
		private int unitCount = 0;
		private bool haveCitadel = true;
		private bool haveBarracks = false;
		private bool haveWorkshop = false;

		private void selectFreeWorker()
		{
			SelectedList.Clear();
			foreach (var un in UnitList)
			{
				if (un.getName() == "Worker"&&un.Orders.Count==0)
				{
					SelectedList.Add(un);
					return;
				}
			}


		}
		private void selectAttackForce()
		{
			SelectedList.Clear();
			foreach (var un in attackForce)
				SelectedList.Add(un);
		}
		public void Think()
		{
			updateNumbers();
			thinkCounter++;
			if (thinkCounter < 120) return;
			thinkCounter = 0;
			switch (phase)
			{
				case 0:
					if (workerCount >= 8)
					{
						SelectedList.Clear();
						selectFreeWorker();
						Builder = SelectedList[0];
						phase++;
						break;
					}
					foreach (var un in UnitList)
					{
						if (un.getName() == "Worker" && un.Orders.Count == 0)
						{
							if (freeRes.Count != 0)
							{
								selectFreeWorker();
								if (SelectedList.Count != 0)
								{
									Order ord = new Order(OrdType.Gather, freeRes[0]);
									busyRes.Add(freeRes[0]);
									freeRes.RemoveAt(0);
									giveOrder(ord, true);
								}
							}
						}
					}
					if (resCount >= UnitCatalog.getCost("Worker"))
					{
						SelectedList.Clear();
						SelectedList.Add(findSpecInList("Citadel"));
						Order ord = new Order(OrdType.Produce, Keys.D1);
						giveOrder(ord, true);
					}
					break;
				case 1:
					if (!haveBarracks) giveOrder(new Order(OrdType.Construct,Keys.D2, 1448, 1748), true);
					if (!haveWorkshop) giveOrder(new Order(OrdType.Construct,Keys.D3, 1748, 1448), true);
					if (haveBarracks && haveWorkshop && Builder.Orders.Count == 0)
					{
						giveOrder(new Order(OrdType.Find, 1448, 2000), true);
						phase++;
					}
					break;
				case 2:
					updateNumbers();
					if (!haveBarracks || !haveWorkshop) return;
					if (unitCount < 9)
					{
						if (findSpecInList("Barracks").Orders.Count == 0 && findSpecInList("Workshop").Orders.Count == 0)
						{
							switch (nextRandomUnit)
							{
								case 1:
									if (UnitCatalog.getCost("Knight") <= resCount)
									{
										findSpecInList("Barracks").getOrder(new Order(OrdType.Produce, Keys.D1), false);
										nextRandomUnit = randomizer.Next(1, 4);
									}
									break;
									
								case 2:
									if (UnitCatalog.getCost("Archer") <= resCount)
									{
										findSpecInList("Barracks").getOrder(new Order(OrdType.Produce, Keys.D2), false);
										nextRandomUnit = randomizer.Next(1, 4);
									}
									break;
								case 3:
									if (UnitCatalog.getCost("Catapult") <= resCount)
									{
										findSpecInList("Workshop").getOrder(new Order(OrdType.Produce, Keys.D1), false);
										nextRandomUnit = randomizer.Next(1, 4);
									}
									break;
							}
						}
						selectAttackForce();
						giveOrder(new Order(OrdType.AttackMove, 1448, 1448), false);
					}
					else
					{
						selectAttackForce();
						giveOrder(new Order(OrdType.AttackMove, 300, 300), false);
						attackForce.Clear();
					}
					break;
			}

			
		}
	}

}
