using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace ahjas
{
	public delegate bool resCh(int n, bool incDec);
	public delegate void uCr(Building creator, creatingData args);
	public delegate void uDel(Building delunit);
	public delegate void depl(Resource res);
	[Serializable]
	public class Resource 
	{
		
		//public owner Owner = owner.Res;
		//public string name;
		public int ResLeft;
		public int x, y;
		public int size;
		public bool busy;

		public event depl ResourceDepleted;
		[NonSerialized]public static Texture2D resTexture;
		public Resource(int Xpos, int Ypos, int RL,ResourcePlayer respl)
		{
			x = Xpos;
			y = Ypos;
			size = 24;
			ResLeft = RL;
			ResourceDepleted += respl.delRes;
		}

		public void Decrease()
		{			
			ResLeft -= 5;
			if (ResLeft <= 0)
			{
				ResourceDepleted.Invoke(this);
			}

		}

		public bool Occupy(Worker worker )
		{
			if (!busy)
			{
				busy = true;
				ResourceDepleted += worker.endMining;
				return true;
			}

			else return false;
		}

		public void free(Worker worker)
		{
			busy = false;
			ResourceDepleted -= worker.endMining;
		}

		public void draw(int dx,int dy, int scrX, int scrY)
		{
			if (x > dx + scrX || x < dx || y > dy + scrY || y < dy) return;
			float scale = 2 * this.size / 288f;
			Game1.sprBatch.Draw(resTexture, new Vector2(this.x - this.size - dx, this.y - this.size - dy), null, Color.LightBlue, 0, Vector2.Zero, new Vector2(scale, scale), SpriteEffects.None, 0);
		}

		public double mesDist(int x,int y)
		{
			return Math.Sqrt(Math.Pow(x - this.x, 2) + Math.Pow(y - this.y, 2));
		}
	}
	[Serializable]
	public class Building 
	{
		protected owner Owner;
		protected int maxHp, curHp;
		protected int Size;
		protected string Name;
		protected int SpriteX, SpriteY;
		public int absX { get; set; }
		public int absY { get; set; }
		public float fX;
		public float fY;
		private bool isReady;
		protected int armor;
		protected bool selected;
		public Queue<Order> Orders;

		public event uCr uCreated;

		public event uDel uDeleted;
		[NonSerialized]private Texture2D Sprite;
		[NonSerialized]protected Point FrameSize;
		[NonSerialized]protected Point FrameCount;
		protected string[] prodList;
		public resCh ChangeRes;
		protected int animCounter=0;
		public virtual void targetDead(Building u)
		{
		}
		public void UpdateAfterLoad()
		{
			FrameSize = UnitCatalog.updateFsize(Name);
			FrameCount = UnitCatalog.updateFcount(Name);
			Sprite = UnitCatalog.updateSprite(Name);
		}
		public Point getFrameSize()
		{
			return FrameSize;
		}
		public void BuildFinished()
		{
			isReady = true;
		}
		public int getHP()
		{
			return curHp;
		}
		public Texture2D getSprite()
		{
			return Sprite;
		}
		public virtual void startBuild(Building toBuild)
		{
		}
		public string getName()
		{
			return Name;
		}
		public void setName(string s)
		{
			Name = s;
		}
		public Building(owner own,uCr toCreate,uDel toDelete,resCh changeRes, uInfo creatingInfo,int x,int y,bool preplaced)
		{
			setName(creatingInfo.name);
			FrameSize = creatingInfo.FrameSize;
			FrameCount = creatingInfo.FrameCount;
			Owner = own;
			uCreated += toCreate;
			uDeleted +=toDelete;
			Size = creatingInfo.size;
			maxHp = creatingInfo.maxhp;
			curHp = maxHp;
			absX = x;
			absY = y;
			fX = (float)x;
			fY = (float)y;
			armor = creatingInfo.armor;
			Orders = new Queue<Order>();
			selected = false;
			isReady = preplaced;
			prodList = new string[5];
			for (int i = 0; i < 5; i++)
			{
				prodList[i] = creatingInfo.prodOptions[i];
			}
			Sprite = creatingInfo.sprite;
			ChangeRes += changeRes;
			SpriteX = 0;
			SpriteY = 0;

		}
		protected void unitCreated(Building Creator,creatingData args)
		{
			uCreated(Creator,args);
		}
		public void select()
		{
			selected = true;
		}

		public void unselect()
		{
			selected = false;
		}

		public void startAttack(uDel u)
		{
			this.uDeleted += u;
		}
		public void endAttack(Unit u)
		{
			if (u == null) uDeleted = null;
			else this.uDeleted -= u.targetDead;
		}
		public void draw(int dx,int dy,int scrX,int scrY)
		{
			if (absX > dx + scrX || absX < dx || absY > dy + scrY || absY < dy) return;
			float scale = 2*this.Size / 1024f;
			Game1.sprBatch.Draw(Sprite, new Rectangle(this.absX - this.Size - dx-8,
		this.absY - this.Size - dy-8,
		Size*2+16, Size*2+16),
	new Rectangle(SpriteX * FrameSize.X,
		SpriteY * FrameSize.Y,
		FrameSize.X, FrameSize.Y),Color.White);
			if(Owner==owner.Player && selected) Game1.sprBatch.Draw(Human.selectionCircle, new Vector2(this.absX - this.Size - dx-4, this.absY - this.Size - dy-4), null, Color.LimeGreen, 0, Vector2.Zero, new Vector2(scale + 0.008f, scale + 0.008f), SpriteEffects.None, 0);
			if (Owner == owner.Bot) Game1.sprBatch.Draw(Human.selectionCircle, new Vector2(this.absX - this.Size - dx - 4, this.absY - this.Size - dy - 4), null, Color.Red, 0, Vector2.Zero, new Vector2(scale + 0.008f, scale + 0.008f), SpriteEffects.None, 0);
			//ACHTUNG must draw units
		}

		public virtual bool getOrder(Order ord,bool enqueue)
		{
			if (!isReady) return false;
			if(Orders.Count!=0) return false;
			switch (ord.type)
			{
				case OrdType.Produce:
					if (prodList[ord.prodUnit] == "") return false;
					uInfo inf =UnitCatalog.getInfo(prodList[ord.prodUnit]);
					if (!ChangeRes(inf.Cost, false)) return false;
					else
					{
						ord.estTime = inf.prodTime;
						Orders.Enqueue(ord);
					}
				return true;

				case OrdType.Upgrade: Orders.Enqueue(ord);
				return true;
					
				default :return false;
			}
		}

		public virtual void dropOrder()
		{
			if(Orders.Count!=0)Orders.Dequeue();
		}

		public virtual void OrderExec()
		{
			if (Orders.Count == 0) return;
			Order curOrd = Orders.Peek();
			switch (curOrd.type)
			{
				case OrdType.Produce:
					if (curOrd.estTime == 0)
					{
						uInfo inf = UnitCatalog.getInfo(prodList[curOrd.prodUnit]);
						creatingData args = new creatingData(prodList[curOrd.prodUnit], this.absX, this.absY + this.Size + 2 + inf.size, true);
						uCreated(this,args);
						dropOrder();
					}
					else curOrd.estTime -= 1;
					break;
					   default:break;
			}
			//order execution

		}



		public void CheckProd(out string prodOpt, out int estTime, out int totalTime)
		{
			if (Orders.Count == 0)
			{
				prodOpt = "";
				estTime = 0;
				totalTime = 0;
			}
			else
			{
				if (Orders.Peek().type == OrdType.Produce || Orders.Peek().type == OrdType.Construct)
				{
					prodOpt = prodList[(Orders.Peek().prodUnit)];
					estTime = Orders.Peek().estTime;
					totalTime = UnitCatalog.getTime(prodList[(Orders.Peek().prodUnit)]);
				}
				else
				{
					prodOpt = "";
					estTime = 0;
					totalTime = 0;
				}
			}

		}

		public bool TakeDamage(int n)
		{
			curHp -= (n - armor);
			if (curHp <= 0)
			{
				uDeleted?.Invoke(this);
				return true;
			}
			else return false;
		}

		public double mesDist(int x, int y)
		{
			double n = Math.Sqrt(Math.Pow(x - absX, 2) + Math.Pow(y - absY, 2));
			return n;
		}

		public int getSize()
		{
			return Size;
		}
	}
	[Serializable]
	public class Unit : Building
	{
		public int range;
		public int attDamage;
		public int attSpeed;
		public int moveSpeed;
		protected int collisionCounter = 0;
		protected int reScanCounter=-1;
		protected int attackMoveX=-1;
		protected int attackMoveY=-1;
		protected bool currentlyAttacking = false;
		public Unit(owner own, uCr toCreate, uDel toDelete, resCh changeRes, uInfo creatingInfo, int x, int y,bool preplaced):base( own, toCreate,  toDelete, changeRes, creatingInfo,x,y, preplaced)
		{
			range = creatingInfo.range;
			attDamage = creatingInfo.damage;
			attSpeed = creatingInfo.attsp;
			moveSpeed = creatingInfo.movesp;

		}

		protected Vector2 checkPointsAround(int centerX, int centerY, float pointX, float pointY, int radius)
		{
			pointX = pointX - centerX;
			pointY = pointY - centerY;
			double angle = Math.Acos(pointX / radius);
			while (radius > Size)
			{
				for (int i = 0; i < 12; i++)
				{
					angle += i * (Math.PI/6);
					pointX = (float)(radius * Math.Cos(angle));
					pointY = (float)(radius * Math.Sin(angle));
					if (Field.checkTerrain((int)pointX + centerX, (int)pointY + centerY)) return new Vector2(pointX+centerX,pointY+centerY);
				}
				radius--;
			}
			return new Vector2(centerX, centerY);
		}
		protected void moveCloser(int x, int y, int dist)
		{
			Vector2 reqPos = findPointBetween(this.absX, this.absY, x, y, dist);
			if (!Field.checkTerrain((int)reqPos.X, (int)reqPos.Y))
			{
				reqPos=checkPointsAround(x,y,reqPos.X,reqPos.Y,dist);
			}
			if (!Field.checkTerrain((int)reqPos.X, (int)reqPos.Y)) return;
			//check if point is reachable
			Order ord1 = Orders.Dequeue();

			Order ord2 = new Order(OrdType.Find, (int)reqPos.X, (int)reqPos.Y);
			getOrder(ord2, true);

			ord1.inited = false;
			getOrder(ord1, true);
		}

		protected Vector2 findPointBetween(int x1, int y1, int x2, int y2, int dist)
		{
			Vector2 coord = new Vector2();

			float lambda = dist / ((float)(mesDist(x2, y2)) - dist);
			coord.X = (x2 + lambda * x1) / (1 + lambda);
			coord.Y = (y2 + lambda * y1) / (1 + lambda);
			return coord;
		}

		public override void targetDead(Building u)
		{
			Order ord=null;
			foreach (var un in Orders)
			{
				if (un.type == OrdType.Attack)
				{
					ord = un;
					break;
				}
				else ord = null;
			}
			if (ord != null)
			{
				ord.Target.endAttack(this);
				reScanCounter = -1;
			}
			uDeleted -= u.targetDead;
			currentlyAttacking = false;
			Orders.Clear();

		}
		public  override void dropOrder()
		{
			SpriteY = 0;
			if (Orders.Count !=0)
			{
				if (Orders.Peek().type == OrdType.Attack)
				{
					Orders.Peek().Target.endAttack(this);
					currentlyAttacking = false;
					reScanCounter = -1;
				}
			}
			collisionCounter = 0;
			base.dropOrder();

		}


		public override void OrderExec()
		{
			if (Orders.Count == 0) 
			{
				collisionCounter = 0;
				if (attackMoveX != -1)
				{
					if (Math.Abs(attackMoveX - absX) <= 1 && Math.Abs(attackMoveY - absY) <= 1)
					{
						attackMoveX = -1;
						attackMoveY = -1;
					}
					else getOrder(new Order(OrdType.Find, attackMoveX, attackMoveY), true);
					return;
				}
				else
				{
					Building target = Game1.Scan(this.Owner, this.absX, this.absY, 10 * 32);
					if (target == null)
					{
						SpriteY = 0;
						return;
					}
					else
					{
						if(target.getHP()>0)
						getOrder(new Order(OrdType.Attack, target), false);
						return;
					}
				}
			}

			Order curOrd = Orders.Peek();

			switch (curOrd.type)
			{
				case OrdType.Find:
					List<Order> way = Field.PathFind(absX, absY, curOrd.xDest, curOrd.yDest);
					if (way == null) dropOrder();
					else
					{
						dropOrder();
						if (Orders.Count != 0)
						{
							if (Orders.Peek().type == OrdType.Attack || Orders.Peek().type == OrdType.Gather || Orders.Peek().type == OrdType.Construct)
							{
								curOrd = Orders.Dequeue();
							}
						}
						foreach (var w in way)
						{
							getOrder(w, true);
						}
						if (curOrd.type != OrdType.Find) getOrder(curOrd, true);
					}
					break;
				case OrdType.AttackMove:
					if (!curOrd.inited)
					{
						attackMoveX = curOrd.xDest;
						attackMoveY = curOrd.yDest;
						dropOrder();
						getOrder(new Order(OrdType.Find, curOrd.xDest, curOrd.yDest), true);
					}

					break;
				case OrdType.Move:
					
					if (attackMoveX != -1)
					{
						if (!currentlyAttacking)
						{
							Building fUnit = Game1.Scan(this.Owner, this.absX, this.absY, 8 * 32);
							if (fUnit != null)
							{
								Orders.Clear();
								int atkX = attackMoveX;
								int atkY = attackMoveY;
								getOrder(new Order(OrdType.Attack, fUnit), false);
								currentlyAttacking = true;
							}
						}

					}
					if (!currentlyAttacking) reScanCounter = -1;
					if (reScanCounter != -1)
					{
						reScanCounter += 1;
						if (reScanCounter == 30)
						{
							
							while (Orders.Peek().type != OrdType.Attack)
								Orders.Dequeue();
							
							break;
						}

					}
					if (!curOrd.inited)
					{
						curOrd.wayX = curOrd.xDest - this.absX;
						curOrd.wayY = curOrd.yDest - this.absY;
						float dist = (float)mesDist((int)(fX + curOrd.wayX), (int)(fY + curOrd.wayY));
						if (dist == 0) dropOrder();
						curOrd.koefX = curOrd.wayX / dist;
						curOrd.koefY = curOrd.wayY / dist;
						curOrd.inited = true;
					}
					else
					{
						if (SpriteX % 2 != 0) SpriteX--;
						Move(curOrd);
					}

					break;
					case OrdType.Attack:
					
					if (!curOrd.inited)
					{
						curOrd.inited = true;
						reScanCounter = 0;
						currentlyAttacking = true;
					}

					if (mesDist(curOrd.Target.absX, curOrd.Target.absY) > curOrd.Target.getSize()+ this.Size+ this.range+1)
						{
							moveCloser(curOrd.Target.absX, curOrd.Target.absY, curOrd.Target.getSize() + this.Size+this.range);
							break;
						}
					if (curOrd.estTime <= attSpeed / 3)
						SpriteY =3- (int)(((curOrd.estTime) / (float)(attSpeed / 3)) / 0.3);
					else 
						SpriteY = 0;
					if (SpriteX % 2 == 0) SpriteX++;
					if (curOrd.estTime == 0)
					{
						attack();
						curOrd.estTime = attSpeed;
						break;
					}
					else curOrd.estTime -= 1;
					break;
				default: 
				break;
			}
			//order execution

		}

		protected Point GoAround(int arx, int ary, float dx, float dy)
		{
			Point p = new Point();
			if (absX <= arx && absY <= ary)
			{
				if (Math.Abs(dx)<0.707)
				{
					p.X = -1;
					p.Y = 0;
				}
				else
				{
					p.X = 0;
					p.Y = -1;
				}

			}
			else if (absX > arx && absY > ary)
			{
				if (Math.Abs(dx) < 0.707)
				{
					p.X = 1;
					p.Y = 0;
				}
				else
				{
					p.X = 0;
					p.Y = 1;
				}
			}
			else if (absX <= arx && absY > ary)
			{
				if (Math.Abs(dx) < 0.707)
				{
					p.X = -1;
					p.Y = 0;
				}
				else
				{
					p.X = 0;
					p.Y = 1;
				}
			}
			else if (absX > arx && absY <= ary)
			{
				if (Math.Abs(dx) < 0.707)
				{
					p.X = 1;
					p.Y = 0;
				}
				else
				{
					p.X = 0;
					p.Y = -1;
				}
			}
			return p;


		}

		protected void Move(Order ord)
		{
			
			float deltX = ord.koefX * moveSpeed / 50f;
			float deltY = ord.koefY * moveSpeed / 50f;
			Point p = Game1.checkCollision(this, (int)Math.Round(fX + deltX), (int)Math.Round(fY + deltY), this.Size);
			if (p.X>=0)
			{
				collisionCounter++;

				if (collisionCounter > 10)
				{
					Point turn=GoAround(p.X,p.Y,deltX,deltY);
					if (collisionCounter > 15)
					{
						Random ran = new Random(this.absX);
						turn.X = ran.Next(-2,2);
						turn.Y = ran.Next(-2,2);
						collisionCounter = 10;
					}
					if (turn.X==-2) return;
					else
					{
						Order bufOrd;
						while (Orders.Count != 1)
						{
							bufOrd = Orders.Dequeue();
						}
							bufOrd = Orders.Dequeue();
							for (int i = 0; i < 32; i++)
							{
							if (Field.checkTerrain(this.absX + (48 - i) * turn.X, this.absX + (48 - i) * turn.X))
								{
									Order bufOrd2 = new Order(OrdType.Move, this.absX + (48 - i) * turn.X, this.absY + (48 - i) * turn.Y);
									if (bufOrd.type == OrdType.Move)bufOrd = new Order(OrdType.Find, bufOrd.xDest, bufOrd.yDest);
									getOrder(bufOrd2, true);
									getOrder(bufOrd, true);
									break;
								}
							}
					}
				}
			}
			else
			{
				if (Math.Abs(ord.wayX) <= Math.Abs(deltX) && Math.Abs(ord.wayY) <= Math.Abs(deltY))
				{
					animCounter = 0;
					absX = ord.xDest;
					absY = ord.yDest;
					dropOrder();
					return;
				}
				if (Math.Abs(ord.wayX) > Math.Abs(deltX))
				{
					fX += deltX;
					ord.wayX -= deltX;
				}
				if (Math.Abs(ord.wayY) > Math.Abs(deltY))
				{
					fY += deltY;
					ord.wayY -= deltY;
				}
			}

			absX = (int)Math.Round(fX);
			absY = (int)Math.Round(fY);
			


			if (Math.Abs(ord.koefX)>0.4  && Math.Abs(ord.koefY)>0.4 )
			{
				if (ord.koefX > 0)
				{
					if (ord.koefY > 0)
					{
						SpriteX = 12;
					}
					else
					{
						SpriteX = 2;
					}
				}
				else
				{
					if (ord.koefY > 0)
					{
						SpriteX = 14;
					}
					else
					{
						SpriteX = 4;
					}
				}
			}
			else
			{
				if (Math.Abs(ord.koefX) >= Math.Abs(ord.koefY))
				{
					if (ord.koefX > 0)
					{
						SpriteX = 6;
					}
					else
					{
						SpriteX = 8;
					}
				}
				else
				{
					if (ord.koefY > 0)
					{
						SpriteX = 10;
					}
					else
					{
						SpriteX = 0;
					}
				}
			}



			animCounter += 1;
			if (animCounter == 4)
			{
				animCounter = 0;

				SpriteY++;
				if (SpriteY == this.FrameCount.Y) SpriteY = 0;

			}
		}



		public override bool getOrder(Order ord,bool enqueue)
		{
			if (!enqueue)
			{
				currentlyAttacking = false;
				attackMoveX = -1;
				attackMoveY = -1;
			}

			switch (ord.type)
			{
				case OrdType.Find:
					if (!enqueue) Orders.Clear();
					Orders.Enqueue(ord);
					return true;
				case OrdType.Move:if(!enqueue)Orders.Clear();
					Orders.Enqueue(ord);
					return true;

				case OrdType.AttackMove:if (!enqueue) Orders.Clear();
					Orders.Enqueue(ord);
					return true;
				case OrdType.Attack:if (!enqueue) Orders.Clear();
					if (reScanCounter == -1) ord.Target.startAttack(targetDead);
					ord.estTime = attSpeed;
					Orders.Enqueue(ord);
					return true;

				default: return false;
			}
		}

		protected void attack()
		{
			if (range < 10) Orders.Peek().Target.TakeDamage(this.attDamage);
			else ProjectileManager.NewProjectile(Name, attDamage, Orders.Peek().Target, absX, absY);
		}


	}
	[Serializable]
	public class Worker:Unit
	{
		private Building nowBuilding;

		CheckForSpec CheckMainBuilding;
		public Worker(owner own,CheckForSpec checkMB ,uCr toCreate, uDel toDelete, resCh changeRes, uInfo creatingInfo, int x, int y,bool preplaced):base(own, toCreate,  toDelete,  changeRes,creatingInfo,x,y, preplaced)
		{
			CheckMainBuilding += checkMB;
		}

		public void endMining(Resource res)
		{
			dropOrder();
		}

		public override void dropOrder()
		{
			if (Orders.Count != 0)
			{
				var x = Orders.Peek();
				if (x.type == OrdType.Gather) x.res.free(this);
			}
			base.dropOrder();
		}
		public override void OrderExec()
		{
			
			base.OrderExec();
			if (Orders.Count == 0) return;
			Order curOrd = Orders.Peek();
			switch (curOrd.type)
			{
				case OrdType.Gather:
					if (!CheckMainBuilding(curOrd.res.x, curOrd.res.y, 32 * 10, "Citadel")) dropOrder();
					if (mesDist(curOrd.res.x, curOrd.res.y) > curOrd.res.size+2 + this.Size)
					{
						if (curOrd.res.busy) curOrd.res.free(this);
						moveCloser(curOrd.res.x, curOrd.res.y, curOrd.res.size + 1 + this.Size);
						break;
					}
					if (!curOrd.res.busy) curOrd.res.Occupy(this);
					if (SpriteX % 2 == 0) SpriteX++;
					animCounter++;
					if (animCounter == 60) animCounter = 0;

					SpriteY = animCounter / 15;
					curOrd.estTime -= 1;
					if (curOrd.estTime == 0)
					{
						curOrd.res.Decrease();
						ChangeRes(5, true);
						curOrd.estTime = 150;
					}
					break;
					case OrdType.Construct:
					if (!curOrd.inited)
					{
						uInfo inf = UnitCatalog.getInfo(prodList[curOrd.prodUnit]);
						if (!Field.checkTerrainForBuilding(curOrd.xDest, curOrd.yDest, inf.size))
						{
							dropOrder();
							break;
						}
						if (mesDist(curOrd.xDest, curOrd.yDest) > inf.size + this.Size+2)
						{
							moveCloser(curOrd.xDest, curOrd.yDest, inf.size + this.Size + 1);
							break;
						}
						if (mesDist(curOrd.xDest, curOrd.yDest) < inf.size + this.Size)
						{
							dropOrder();
							return;
						}
						if (!ChangeRes(inf.Cost, false))
						{
							dropOrder();
							break;
						}
						curOrd.estTime = inf.prodTime;
						curOrd.inited = true;
						creatingData args = new creatingData(prodList[curOrd.prodUnit], curOrd.xDest, curOrd.yDest, false);
						unitCreated(this,args);
					}
					else
					{
						if (SpriteX % 2 == 0) SpriteX++;
						animCounter++;
						if (animCounter == 60) animCounter = 0;

						SpriteY = animCounter/15;

						curOrd.estTime -= 1;
						if (curOrd.estTime == 0)
						{
							nowBuilding.BuildFinished();
							nowBuilding = null;
							dropOrder();
						}
					}
					break;
				default:
				break;
			}
		}


		public override void startBuild(Building toBuild)
		{
			nowBuilding = toBuild;
		}

		public override bool getOrder(Order ord, bool enqueue)
		{
			if (Orders.Count != 0)
			{
				if (Orders.Peek().type == OrdType.Construct) return false;
				if (Orders.Peek().type == OrdType.Gather) dropOrder();
			}
			if (!enqueue) currentlyAttacking = false;
			switch (ord.type)
			{
				case OrdType.Find:
					if (!enqueue) Orders.Clear();
					Orders.Enqueue(ord);
					return true;
				case OrdType.Move:if (!enqueue)Orders.Clear();
					Orders.Enqueue(ord);
					return true;
				case OrdType.AttackMove:if (!enqueue)Orders.Clear();
					Orders.Enqueue(ord);
					return true;
				case OrdType.Attack: if (!enqueue)Orders.Clear();
					if (reScanCounter == -1) ord.Target.startAttack(targetDead);
					ord.estTime = attSpeed;
					Orders.Enqueue(ord);
					return true;
				case OrdType.Gather:if (!enqueue) Orders.Clear();
					if (ord.res.busy) return false;
					Orders.Enqueue(ord);
					return true;
				case OrdType.Construct:if (!enqueue)Orders.Clear();
					Orders.Enqueue(ord);
					return true;
					
				default: return false;
			}
		}
	}
}
