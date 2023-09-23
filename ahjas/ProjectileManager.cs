using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ahjas
{
	public delegate void hit(Projectile proj);
	[Serializable]
	public class Projectile
	{
		private int absX, absY;
		private float fX, fY;
		public int Size;
		public int speed;
		public int damage;
		private float rotation = 0;
		private Building Target;
		public Texture2D Sprite;
		public event hit projHit;
		public Projectile(int x,int y,int damage,int speed,Texture2D sprite,Building Target,int size)
		{
			absX = x;
			fX = absX;
			absY = y;
			fY = absY;
			this.Target = Target;
			this.Sprite = sprite;
			this.speed = speed;
			this.damage = damage;
			Size = size;
			 
		}
		public void setEvent(hit OnHit)
		{
			projHit += OnHit;
		}
		public void Move()
		{
			if (Target.mesDist(this.absX, this.absY) < Target.getSize())
			{
				Target.TakeDamage(this.damage);
				projHit(this);
				return;
			}
			float koefX = (Target.absX - absX) / (float)Target.mesDist(absX, absY);
			float koefY = (Target.absY - absY) / (float)Target.mesDist(absX, absY);
			rotation = (float)Math.Atan2(koefX, -koefY);
			fX += koefX * speed / 50f;
			fY += koefY * speed / 50F;
			absX = (int)Math.Round(fX);
			absY = (int)Math.Round(fY);
		}
		public void Draw(int dx, int dy,int scrX,int scrY)
		{
			if (absX > dx + scrX || absX < dx || absY > dy + scrY || absY < dy) return;
			Game1.sprBatch.Draw(Sprite, new Vector2(this.absX - this.Size - dx, this.absY - this.Size - dy), null,
								Color.White, rotation, new Vector2(32,32), new Vector2((float)Size/64,(float)Size/64), SpriteEffects.None, 0);
		}

	}
	[Serializable]
	public static class ProjectileManager
	{
		static List<Projectile> projList;
		static List<Projectile> tempList;
		static Dictionary<string, Projectile> catalog;
		static ProjectileManager()
		{
			projList = new List<Projectile>();
			tempList=new List<Projectile>();
			catalog = new Dictionary<string, Projectile>();
		}
		public static void addProjectileType(string name, Projectile proj)
		{
			catalog.Add(name, proj);
		}
		public static void NewProjectile(string name,int damage,Building Target,int x,int y)
		{
			Projectile proj = new Projectile(x, y, damage, catalog[name].speed, catalog[name].Sprite, Target,catalog[name].Size);
			proj.setEvent(OnHit);
			projList.Add(proj);
		}
		public static void moveProj()
		{
			foreach (var proj in projList)
			{
				proj.Move();
			}
			while (tempList.Count != 0)
			{
				projList.Remove(tempList[0]);
				var a = tempList[0];
				tempList.RemoveAt(0);
				a = null;
			}
		}
		public static void OnHit(Projectile proj)
		{
			
			tempList.Add(proj);
		}
		public static void drawProj(int dx, int dy, int scrX, int scrY)
		{
			Game1.sprBatch.Begin();
			foreach (var proj in projList)
			{
				proj.Draw(dx,dy,scrX,scrY);
			}
			Game1.sprBatch.End();
		}

	}
}
//gg na pare
