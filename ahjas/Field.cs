using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
enum Dir
{
	L,R,U,D,LD,LU,RU,RD
}

namespace ahjas
{
	[Serializable]
	public class PathNode
	{
		public Point pos;
		public int lengthFromStart;
		public PathNode cameFrom;
		public int estLength;
		public int fullLength
		{
			get
			{
				return estLength + lengthFromStart;
			}
		}


	}
	[Serializable]
	public class Field
	{
		private const int TileSize = 32;
		private static Tile[,] TileArray;
		private static bool[,] TileMap;
		private int cols, rows;

		public static bool checkTerrainForBuilding(int x, int y, int size)
		{
			int TileCol = x / TileSize;
			int TileRow = y / TileSize;
			int sizeInTiles = size / TileSize;
			if (size % TileSize != 0) sizeInTiles++;
			if (TileCol - sizeInTiles < 0 || TileCol + sizeInTiles > TileMap.Length - 1 || TileRow - sizeInTiles < 0 || TileRow + sizeInTiles > TileMap.Length - 1)
				return false;
			for (int j = TileCol - sizeInTiles; j <= TileCol + sizeInTiles; j++)
			{
				for (int i = TileRow - sizeInTiles; i <= TileRow + sizeInTiles; i++)
					if (!TileMap[i, j]) return false;
			}
			return true;
		}

		public static int getDist(Point from,Point to)
		{
			return (int)Math.Round(Math.Sqrt(Math.Pow(Math.Abs(from.X - to.X) * 32,2) + Math.Pow(Math.Abs(from.Y - to.Y) * 32,2)));
		}

		public static List<Order> PathFind(int curX, int curY, int destX, int destY)
		{
			int startTileCol = curX / TileSize;
			int startTileRow = curY / TileSize;
			int endTileCol = destX / TileSize;
			int endTileRow = destY / TileSize;
			if (endTileCol < 0 || endTileRow < 0 || endTileRow > 63||endTileCol >63) return null;
			Point startPoint = new Point(startTileCol, startTileRow);
			Point endPoint = new Point(endTileCol, endTileRow);
			List<Point>Path =pathCalc(startPoint, endPoint);
			if (Path == null) return null;
			else
			{
				List<Order> way = new List<Order>();
				Order ord;
				//ord= new Order(OrdType.Move, startTileCol * TileSize + TileSize / 2, startTileRow * TileSize + TileSize / 2);

				//way.Add(ord);
				foreach (var w in Path)
				{
					ord=new Order(OrdType.Move, w.X * TileSize + TileSize / 2, w.Y * TileSize + TileSize / 2);
					way.Add(ord);
				}
				way.RemoveAt(0);
				if(way.Count!=0)way.RemoveAt(way.Count-1);
				ord=new Order(OrdType.Move, destX, destY);

				way.Add(ord);
				return way;
			}


		}
		public static List<Point> pathCalc(Point start, Point end)
		{
			var closedSet = new List<PathNode>();
			var openSet = new List<PathNode>();
			if (!TileMap[end.Y, end.X]) return null;
			PathNode startNode = new PathNode()
			{
				pos = start,
				cameFrom = null,
				lengthFromStart = 0,
				estLength = getDist(start, end)
			};
			openSet.Add(startNode);
			while (openSet.Count > 0)
			{
				var curNode = openSet.OrderBy(node => node.fullLength).First();
				if (curNode.pos == end)
					return GetPathForNode(curNode);
				openSet.Remove(curNode);
				closedSet.Add(curNode);


				foreach (var neighbourNode in GetNeighbours(curNode, end))
				{

					if (closedSet.Count(node => node.pos == neighbourNode.pos) > 0)
						continue;
					var openNode = openSet.FirstOrDefault(node =>
					  node.pos == neighbourNode.pos);

					if (openNode == null)
						openSet.Add(neighbourNode);
					else if (openNode.lengthFromStart > neighbourNode.lengthFromStart)
					{
						openNode.cameFrom = curNode;
						openNode.lengthFromStart = neighbourNode.lengthFromStart;
					}

				}

			  }
			return null;
		}
		public static List<Point> GetPathForNode(PathNode node)
		{
			var result = new List<Point>();
			var currentNode = node;
			while (currentNode != null)
			{
				result.Add(currentNode.pos);
				currentNode = currentNode.cameFrom;
			}
			result.Reverse();
			return result;	
		}
		public static List<PathNode> GetNeighbours(PathNode pathNode,Point end)
		{
			var result = new List<PathNode>();

			// Соседними точками являются соседние по стороне клетки.
			Point[] neighbourPoints = new Point[8];
			neighbourPoints[0] = new Point(pathNode.pos.X + 1, pathNode.pos.Y);
			neighbourPoints[1] = new Point(pathNode.pos.X - 1, pathNode.pos.Y);
			neighbourPoints[2] = new Point(pathNode.pos.X, pathNode.pos.Y + 1);
			neighbourPoints[3] = new Point(pathNode.pos.X, pathNode.pos.Y - 1);
			neighbourPoints[4] = new Point(pathNode.pos.X + 1, pathNode.pos.Y+1);
			neighbourPoints[5] = new Point(pathNode.pos.X - 1, pathNode.pos.Y-1);
			neighbourPoints[6] = new Point(pathNode.pos.X-1, pathNode.pos.Y + 1);
			neighbourPoints[7] = new Point(pathNode.pos.X+1, pathNode.pos.Y - 1);

			foreach (var point in neighbourPoints)
			{
				// Проверяем, что не вышли за границы карты.
				if (point.X < 0 || point.X >= TileMap.GetLength(0))
					continue;
				if (point.Y < 0 || point.Y >= TileMap.GetLength(1))
					continue;
				// Проверяем, что по клетке можно ходить.
				if (!TileMap[point.Y, point.X])
					continue;
				// Заполняем данные для точки маршрута.
				var neighbourNode = new PathNode()
				{
					pos = point,
					cameFrom = pathNode,
					lengthFromStart = pathNode.lengthFromStart +
					getDistBetween(pathNode.pos,point),
					estLength = getDist(point, end)
				};
				result.Add(neighbourNode);
			}
			return result;
		}
		private static int getDistBetween(Point a,Point b)
		{
			if (a.X == b.X || a.Y == b.Y)
				return 32;
			else return 45;
		}




	public static bool checkTerrain(int x, int y)
	{
		return TileMap[y / TileSize, x / TileSize];
	}
	public Field(int col, int row)
	{
		MapInit(col, row);
		cols = col;
		rows = row;
		TileArray = new Tile[col, row];
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				TileArray[i, j] = new Tile(TileMap[i, j]);
			}
		}
	}

	private void MapInit(int colCount, int rowCount)
	{
		TileMap = new bool[rowCount, colCount];
		StreamReader fs = new StreamReader("map.txt", System.Text.Encoding.UTF8);
		string line;
		for (int i = 0; i < rowCount; i++)
		{
			line = fs.ReadLine();
			for (int j = 0; j < colCount; j++)
			{
				if (line[j] == '1') TileMap[i, j] = true;
				else TileMap[i, j] = false;
			}
		}
	}


	public void draw(ref Texture2D brt, ref Texture2D dbrt, int ScreenSizeX, int ScreenSizeY, int dx, int dy)
	{
		Game1.sprBatch.Begin();
		int yn = ScreenSizeY / TileSize + 1;
		int xn = ScreenSizeX / TileSize + 1;
		int tx = dx / TileSize;
		int ty = dy / TileSize;
		if (yn * TileSize + (ty * TileSize - dy) < ScreenSizeY) yn++;
		if (xn * TileSize + (tx * TileSize - dx) < ScreenSizeX) xn++;

		for (int i = 0; i < yn; i++)
		{
			for (int j = 0; j < xn; j++)
			{
				if ((i + ty < 64) && (j + tx < 64)) TileArray[i + ty, j + tx].draw(ref brt, ref dbrt, (j + tx) * TileSize - dx, (i + ty) * TileSize - dy);
			}
		}
		Game1.sprBatch.End();
	}
}
}
