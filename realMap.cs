using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VoronatorSharp;
using MedRandomizer;
using static System.Net.Mime.MediaTypeNames;
using static mapStuff.mapCreator;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Realistic
{
    class RealMap
    {
        public static int landplateCount = 3;
        public static int seaplateCount = 2;
        public static int plateCount = landplateCount + seaplateCount;
        public static Vector2[] platePoints = new Vector2[plateCount];
        public static Random rd = new Random(Guid.NewGuid().GetHashCode());
        public static Color[] plateColors = new Color[plateCount];

        public static int mapWidth = 510;
        public static int mapHeight = 510;
        public static int xMargin = mapWidth / 20;
        public static int yMargin = mapHeight / 20;

        static public void createMap()
        {
            xMargin = mapWidth / 20; 
            yMargin = mapHeight / 20;
            spawnPlatePoints();
            fillPlates();
        }

        public class mapTile
        {
            public int x;
            public int y;
            public int groundType;
            public int landType;
            public int height;
            public double distToPlateBorder;
            public mapPlate plate;
            public bool border;
            public List<mapPlate> convergence = new List<mapPlate>();
        }
        public class mapPlate
        {
            public Color mapColor;
            public List<Vector2> borderTiles = new List<Vector2>();
            public List<Vector2> allTiles = new List<Vector2>();
            public List<Vector2> convergentTiles = new List<Vector2>();
            public Vector2 originPoint;
            public int plateType;
            public int direction;
            public int landFill;
            public int fillTiles;
            public int tileCount;
            public int speed;
        }

        static public mapPlate[] plateDB = new mapPlate[plateCount];

        public static void createDB()
        {
            tileDB = new mapTile[mapWidth, mapHeight];
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    mapTile tile = new mapTile();
                    tile.y = j;
                    tile.x = i;
                    tile.groundType = 0;
                    tile.landType = 0;
                    tile.height = 0;
                    tileDB[i, j] = tile;
                }
            }
        }

        public static mapTile[ , ] tileDB;

        static public double getDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        public static bool plateSpawnRules(int x, int y)
        {
            if (x > xMargin && x < mapWidth - xMargin)
            {
                if (y > yMargin && y < mapHeight - yMargin)
                {
                    foreach (Vector2 point in platePoints)
                    {
                        if (x == (int)point.x && y == (int)point.y)
                        {
                            continue;
                        }
                        if (getDistance(x, y, (int)point.x, (int)point.y) < (xMargin + yMargin))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        static public int minLandFillCont = 10;
        static public int minLandFillSea = 10;
        public static void spawnPlatePoints()
        {
            plateCount = seaplateCount + landplateCount;
            plateDB = new mapPlate[plateCount];
            platePoints = new Vector2[plateCount];
            plateColors = new Color[plateCount];
            for (int i = 0; i < landplateCount; i++)
            {
                int newX = rd.Next(xMargin, mapWidth - xMargin + 1);
                int newY = rd.Next(yMargin, mapHeight - yMargin + 1);
                do
                {
                    newX = rd.Next(xMargin, mapWidth - xMargin + 1);
                    newY = rd.Next(yMargin, mapHeight - yMargin + 1);
                    platePoints[i] = new Vector2(newX, newY);
                } while (!plateSpawnRules(newX, newY));
                do
                {
                    plateColors[i] = Color.FromArgb(rd.Next(15, 240), rd.Next(15, 240), rd.Next(15, 240));
                } while (plateColors[i].GetHue() > 180 && plateColors[i].GetHue() < 300);
                plateDB[i] = new mapPlate();
                plateDB[i].mapColor = plateColors[i];
                plateDB[i].originPoint = platePoints[i];
                plateDB[i].plateType = 1;
                plateDB[i].landFill = rd.Next(minLandFillCont, 101);
                plateDB[i].direction = rd.Next(0, 361);
                plateDB[i].speed = rd.Next(1, (xMargin/2));
                Console.WriteLine(plateDB[i].direction);
                Console.WriteLine(plateDB[i].mapColor);

            }
            for (int i = landplateCount; i < plateCount; i++)
            {
                int newX = rd.Next(xMargin, mapWidth - xMargin + 1);
                int newY = rd.Next(yMargin, mapHeight - yMargin + 1);
                do
                {
                    newX = rd.Next(xMargin, mapWidth - xMargin + 1);
                    newY = rd.Next(yMargin, mapHeight - yMargin + 1);
                    platePoints[i] = new Vector2(newX, newY);
                } while (!plateSpawnRules(newX, newY));
                do
                {
                    plateColors[i] = Color.FromArgb(rd.Next(15, 240), rd.Next(15, 240), rd.Next(15, 240));
                } while (plateColors[i].GetHue() < 210 || plateColors[i].GetHue() > 270);
                plateDB[i] = new mapPlate();
                plateDB[i].mapColor = plateColors[i];
                plateDB[i].originPoint = platePoints[i];
                plateDB[i].plateType = 0;
                plateDB[i].landFill = rd.Next(0, minLandFillSea);
                plateDB[i].direction = rd.Next(0, 361);
                plateDB[i].speed = rd.Next(1, (xMargin / 2));
                Console.WriteLine(plateDB[i].mapColor);
                Console.WriteLine(plateDB[i].direction);
            }
        }

        public static void colorBorderPixels(int x, int y, Bitmap mapImage, int depth, int type)
        {
            Color setColor = mapImage.GetPixel(x, y);
            for (int i = 0; i < 3 * depth; i++)
            {
                for (int j = 0; j < 3 * depth; j++)
                {
                    int targetX = Math.Min(Math.Max((x - 1) + i, 0), mapWidth - 1);
                    int targetY = Math.Min(Math.Max((y - 1) + j, 0), mapHeight - 1);
                    if (mapImage.GetPixel(targetX , targetY).GetHue() == 0)
                    {
                        mapImage.SetPixel(targetX, targetY, setColor);
                        if (type == 0)
                        {
                            pixelCounter--;
                        }
                        else if (type == 1)
                        {
                            tileDB[targetX, targetY].plate.fillTiles--;
                            tileDB[targetX, targetY].landType = 1;
                        }
                    }
                }
            }
        }

        static public int pixelCounter = mapWidth * mapHeight;
        public static void fillPlates()
        {
            createDB();
            mapImage = new System.Drawing.Bitmap(mapWidth, mapHeight);
            g = Graphics.FromImage(mapImage);
            g.Clear(Color.Black);
            PictureBox box1 = imageWindow.pictureBox2;
            ((System.ComponentModel.ISupportInitialize)(box1)).BeginInit();
            box1.Location = new System.Drawing.Point(73, 62);
            box1.Size = mapImage.Size;
            box1.TabStop = false;
            box1.BackgroundImage = mapImage;
            ((System.ComponentModel.ISupportInitialize)(box1)).EndInit();
            imageWindow.pictureBox2 = box1;
            imageWindow.Refresh();
            imageWindow.Show();
            pixelCounter = mapWidth * mapHeight;
            for (int i = 0; i < platePoints.Length; i++)
            {
                mapImage.SetPixel((int)platePoints[i].x, (int)platePoints[i].y, plateColors[i]);
                pixelCounter--;
                colorBorderPixels((int)platePoints[i].x, (int)platePoints[i].y, mapImage, 2, 0);
            }
            int printlooper = 0;
            while (pixelCounter > 0)
            {
                int x = rd.Next(0, mapWidth);
                int y = rd.Next(0, mapHeight);
                printlooper++;

                if (mapImage.GetPixel(x, y).GetHue() == 0)
                {
                    continue;
                }
                else
                {
                    colorBorderPixels(x, y, mapImage, 1, 0);
                }
                if (printlooper > mapWidth * 1000)
                {
                    printlooper = 0;
                    imageWindow.pictureBox2.BackgroundImage = mapImage;
                    imageWindow.pictureBox2.Size = mapImage.Size;
                    imageWindow.Refresh();
                }

            }
            MessageBox.Show("Done");
            setPlateTiles();
            imageWindow.Refresh();
            imageWindow.pictureBox2.BackgroundImage = mapImage;
            MessageBox.Show("Done");
            try
            {
                mapImage.Save("myMapPlates.tif", ImageFormat.Tiff);
            }
            catch
            {
            }
            imageWindow.Refresh();
            makeLandMass();
            imageWindow.Refresh();
            MessageBox.Show("Done");
            double[,] kernel = GaussianBlur(blurLength, blurWeight);
            mapImage = Convolve(mapImage, kernel);
            for (int i = 0; i < mapImage.Width; i++)
            {
                for (int j = 0; j < mapImage.Height; j++)
                {
                    Color pixel = mapImage.GetPixel(i, j);
                    if (pixel.R > 50)
                    {
                        mapImage.SetPixel(i, j, Color.Brown);
                        tileDB[i, j].landType = 1;
                    }
                    else if (pixel.G > 20)
                    {
                        mapImage.SetPixel(i, j, Color.Green);
                        tileDB[i, j].landType = 1;
                    }
                    else
                    {
                        mapImage.SetPixel(i, j, Color.Blue);
                        tileDB[i, j].landType = 0;
                    }
                }
            }
            imageWindow.pictureBox2.BackgroundImage = mapImage;
            imageWindow.Refresh();
            MessageBox.Show("Done");
            try
            {
                mapImage.Save("myMap.tif", ImageFormat.Tiff);
            }
            catch
            {
            }
            MessageBox.Show("Done");
        }

        static public int blurLength = 10;
        static public double blurWeight = 50;

        static int GetHeadingDifference(int heading1, int heading2)
        {
            var difference = Math.Abs(heading1 - heading2);
            if (difference > 180)
            {
                return 360 - difference;
            }

            return difference;
        }

        static public void getConvergence(Vector2 tile)
        {
            int x = (int)tile.x;
            int y = (int)tile.y;
            int angle = tileDB[x, y].plate.direction;
            double endX = x;
            double endY = y;
            int dist = ((xMargin+ yMargin) / 2) - (xMargin/2) + tileDB[x, y].plate.speed;
            endX = Math.Round(endX + (dist * Math.Cos(angle)));
            endY = Math.Round(endY + (dist * Math.Sin(angle)));
            if (endX < 0 || endX > mapWidth - 1 || endY < 0 || endY > mapHeight - 1)
            {
                return;
            }
            if (tileDB[(int)endX, (int)endY].convergence.Contains(tileDB[x, y].plate))
            {
                return;
            }
            tileDB[(int)endX, (int)endY].convergence.Add(tileDB[x, y].plate);
    }

        static public void makeLandMass()
        {
            g.Clear(Color.Blue);
            imageWindow.Refresh();
            foreach (mapPlate plate in plateDB)
            {
                imageWindow.Refresh();
                double filltyles = (double)plate.tileCount * ((double)plate.landFill / 100);
                plate.fillTiles = (int)filltyles;
                Vector2 contSpawnPoint = new Vector2(0,0);
                do
                {
                    contSpawnPoint = validTile(plate.originPoint + new Vector2(rd.Next(0, xMargin), rd.Next(0, yMargin)));
                } while (tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].plate != plate);

                mapImage.SetPixel((int)contSpawnPoint.x, (int)contSpawnPoint.y, Color.Green);
                tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].landType = 1;
                plate.fillTiles--;

                colorBorderPixels((int)contSpawnPoint.x, (int)contSpawnPoint.y, mapImage, 2, 1);
                do
                {
                    contSpawnPoint = validTile(plate.originPoint + new Vector2(rd.Next(0, xMargin), rd.Next(0, yMargin)));
                } while (tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].plate != plate);

                mapImage.SetPixel((int)contSpawnPoint.x, (int)contSpawnPoint.y, Color.Green);
                tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].landType = 1;
                plate.fillTiles--;

                colorBorderPixels((int)contSpawnPoint.x, (int)contSpawnPoint.y, mapImage, 2, 1);
                do
                {
                    contSpawnPoint = validTile(plate.originPoint + new Vector2(rd.Next(0, xMargin), rd.Next(0, yMargin)));
                } while (tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].plate != plate);

                mapImage.SetPixel((int)contSpawnPoint.x, (int)contSpawnPoint.y, Color.Green);
                tileDB[(int)contSpawnPoint.x, (int)contSpawnPoint.y].landType = 1;
                plate.fillTiles--;

                colorBorderPixels((int)contSpawnPoint.x, (int)contSpawnPoint.y, mapImage, 2, 1);

                while (plate.fillTiles > 0)
                {
                    Vector2 tile = plate.allTiles[rd.Next(plate.allTiles.Count)];
                    checkBorderFill(tile);
                }

                int lakeCount = rd.Next(0, plate.allTiles.Count / 5000);
                for (int i = 0; i < lakeCount; i++)
                {
                    Vector2 newLakePoint = plate.allTiles[rd.Next(0, plate.allTiles.Count)];
                    makeIsland(newLakePoint, 0);
                }

                if (plate.convergentTiles.Count > 10)
                {

                    int rangeCount = rd.Next(1, (int)Math.Ceiling((double)plate.convergentTiles.Count / (double)xMargin));
                    Vector2 newRangePoint;
                    {
                        for (int i = 0; i < rangeCount; i++)
                        {
                            int spawnCounter = plate.convergentTiles.Count * 100;
                            do
                            {
                                newRangePoint = plate.convergentTiles[rd.Next(0, plate.convergentTiles.Count)];
                                spawnCounter--;
                            } while (tileDB[(int)newRangePoint.x, (int)newRangePoint.y].landType == 0 && spawnCounter > 0);
                            if (spawnCounter == 0)
                            {
                                continue;
                            }
                            makeIsland(newRangePoint, 2);
                        }
                    }

                    int islandCount = rd.Next(1, (int)Math.Ceiling((double)plate.convergentTiles.Count / (double)xMargin));
                    Vector2 newIslandPoint;

                    for (int i = 0; i < islandCount; i++)
                    {
                        int spawnCounter = plate.convergentTiles.Count*10;
                        do
                        {
                            newIslandPoint = plate.convergentTiles[rd.Next(0, plate.convergentTiles.Count)];
                            spawnCounter--;
                        } while (tileDB[(int)newIslandPoint.x, (int)newIslandPoint.y].landType == 1 && spawnCounter > 0);
                        if (spawnCounter == 0) 
                        {
                            continue;
                        }
                        makeIsland(newIslandPoint, 1);
                    }
                }

                imageWindow.Refresh();
            }

        }

        public static void makeIsland(Vector2 point, int land)
        {
            int islandSize = rd.Next(0, xMargin*15);
            if (land == 0)
            {
                islandSize *= 5;
            }
            Vector2 newPoint = point;
            for (int i = 0; i < islandSize;i++)
            {
                if (newPoint.x < 0 || newPoint.x > mapWidth - 1 || newPoint.y < 0 || newPoint.y > mapHeight - 1)
                {
                    continue;
                }

                if (land == 1 && tileDB[(int)newPoint.x, (int)newPoint.y].convergence.Count > 1)
                {
                    mapImage.SetPixel((int)newPoint.x, (int)newPoint.y, Color.Green);
                    tileDB[(int)newPoint.x, (int)newPoint.y].landType = 1;
                    newPoint = new Vector2((newPoint.x - 1) + rd.Next(0, 3), (newPoint.y - 1) + rd.Next(0, 3));
                    continue;
                } 
                else if(land == 2 && tileDB[(int)newPoint.x, (int)newPoint.y].convergence.Count > 1 && tileDB[(int)newPoint.x, (int)newPoint.y].landType == 1)
                {
                    mapImage.SetPixel((int)newPoint.x, (int)newPoint.y, Color.Brown);
                    tileDB[(int)newPoint.x, (int)newPoint.y].landType = 1;
                    tileDB[(int)newPoint.x, (int)newPoint.y].groundType = 1;
                    newPoint = new Vector2((newPoint.x - 1) + rd.Next(0, 3), (newPoint.y - 1) + rd.Next(0, 3));
                    continue;
                }
                else if(land == 0)
                {
                    mapImage.SetPixel((int)newPoint.x, (int)newPoint.y, Color.Blue);
                    tileDB[(int)newPoint.x, (int)newPoint.y].landType = 0;
                    newPoint = new Vector2((newPoint.x - 1) + rd.Next(0, 3), (newPoint.y - 1) + rd.Next(0, 3));
                    continue;
                }
                newPoint = point;

            }
        }

        public static Vector2 validTile(Vector2 tile)
        {   
            return new Vector2(Math.Max(Math.Min(tile.x, mapWidth - xMargin), xMargin), Math.Max(Math.Min(tile.y, mapHeight - yMargin), yMargin));
        }


        public static void setPlateTiles()
        {

            for (int x = 0; x < mapImage.Width; x++)
            {
                for (int y = 0; y < mapImage.Height; y++)
                {
                    Color pixColor = mapImage.GetPixel(x, y);
                    foreach (mapPlate plate in plateDB)
                    {
                        if (plate.mapColor == pixColor)
                        {
                            tileDB[x, y].plate = plate;
                            plate.allTiles.Add(new Vector2(x, y));
                            plate.tileCount++;
                            continue;
                        }
                    }
                }
            }

            for (int x = 0; x < mapImage.Width; x++)
            {
                for (int y = 0; y < mapImage.Height; y++)
                {
                    checkBorder(x, y);
                }
            }


            for (int x = 0; x < mapImage.Width; x++)
            {
                for (int y = 0; y < mapImage.Height; y++)
                {
                    getConvergence(new Vector2((float)x, (float)y));
                }
            }

            for (int x = xMargin; x < mapImage.Width - xMargin; x++)
            {
                for (int y = yMargin; y < mapImage.Height - yMargin; y++)
                {
                    if (tileDB[x, y].convergence.Count == 0)
                    {
                        mapImage.SetPixel(x, y, Color.Azure);
                    }
                    else if (tileDB[x, y].convergence.Count >= 2)
                    {
                        mapImage.SetPixel(x, y, Color.DarkRed);
                        tileDB[x, y].plate.convergentTiles.Add(new Vector2((float)x, (float)y));
                    }
                }
            }

            foreach (mapPlate plate in plateDB)
            {
                int angle = plate.direction;
                Point origin = new Point((int)plate.originPoint.x, (int)plate.originPoint.y);
                int dist = (xMargin + yMargin) / 2 + plate.speed;
                double endX = plate.originPoint.x + (dist * Math.Cos(angle));
                double endY = plate.originPoint.y + (dist * Math.Sin(angle));
                Point end = new Point((int)endX, (int)endY);
                DrawLine(g, origin, end);

            }

        }
        public static void DrawLine(Graphics g, Point from, Point to)
        {
            var pen = new Pen(Color.White, 1);

            pen.CustomEndCap = new AdjustableArrowCap(5, 5);

            g.DrawLine(pen, from, to);
        }

        public static void getBorderDist(int x , int y)
        {
            mapPlate locplate = tileDB[x, y].plate;
            double mindist = mapWidth;
            foreach (Vector2 borderTile in locplate.borderTiles)
            {
                double dist = getDistance(x, y, (int)borderTile.x, (int)borderTile.y);
                if (dist < mindist)
                {
                    mindist = dist; 
                }
            }
            tileDB[x, y].distToPlateBorder = mindist;
        }

        public static void checkBorderFill(Vector2 tile)
        {
            int x = (int)tile.x;
            int y = (int)tile.y;
            if (tileDB[x, y].landType == 1)
            {
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int targetX = Math.Min(Math.Max((x - 1) + i, 0), mapWidth - 1);
                    int targetY = Math.Min(Math.Max((y - 1) + j, 0), mapHeight - 1);
                    if (tileDB[targetX, targetY].landType == 1)
                    {
                        tileDB[x, y].landType = 1;
                        mapImage.SetPixel(x, y, Color.Green);
                        tileDB[x, y].plate.fillTiles--;
                        return;
                    }
                    int newTileX = x;
                    int newTileY = x;
                    //var randomNum = getPercent();
                    //while (randomNum > 95)
                    //{
                    //    randomNum = getPercent();
                    //    newTileX = Math.Min(Math.Max((newTileX - 1) + rd.Next(0, 3), 0), mapWidth - 1);
                    //    newTileY = Math.Min(Math.Max((newTileX - 1) + rd.Next(0, 3), 0), mapWidth - 1);
                    //    if (tileDB[newTileX, newTileY].landType == 0 && tileDB[newTileX, newTileY].plate.fillTiles > 0)
                    //    {
                    //        tileDB[newTileX, newTileY].landType = 1;
                    //        mapImage.SetPixel(newTileX, newTileY, Color.Green);
                    //        tileDB[newTileX, newTileY].plate.fillTiles--;
                    //    }
                    //}
                }
            }
        }

        public static void checkBorder(int x, int y)
        {
            Color pixColor = mapImage.GetPixel(x, y);
            mapPlate locplate = tileDB[x, y].plate;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int targetX = Math.Min(Math.Max((x - 1) + i, 0), mapWidth - 1);
                    int targetY = Math.Min(Math.Max((y - 1) + j, 0), mapHeight - 1);
                    if (tileDB[targetX, targetY].plate != locplate)
                    {
                        tileDB[x, y].border = true;
                        locplate.borderTiles.Add(new Vector2(x,y));
                        mapImage.SetPixel(x, y, Color.Black);
                        return;
                    }
                }
            }
        }

        public static Bitmap mapImage = new System.Drawing.Bitmap(mapWidth, mapHeight);
        public static Graphics g = Graphics.FromImage(mapImage);
        public static mapWindow imageWindow = new mapWindow();

        public static void ShowImage()
        {

            for (int x = 0; x < mapImage.Width; x++)
            {
                for (int y = 0; y < mapImage.Height; y++)
                {
                    Color c = mapImage.GetPixel(x, y);
                    if (imageWindow.InvokeRequired)
                    {
                        var x1 = x;
                        var y1 = y;
                        imageWindow.BeginInvoke((Action)(() =>
                        {
                            g.FillRectangle(new SolidBrush(c), x1, y1, 1, 1);
                        }));
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(c), x, y, 1, 1);
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
        }




    }
}
