using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using VoronatorSharp;

namespace mapStuff
{
    internal class mapCreator
    {
        public static int mapWidth = 0;
        public static int mapHeight = 0;
        public static Random rd = new Random(Guid.NewGuid().GetHashCode());

        public static tileEntry[,] coordinateGrid;

        public static void CreateMap(int width, int height, int continents)
        {
            byte[] bytes = CreateGridImage(width, height, continents);
        }

        public static double getDistance(int x, int y, int x2, int y2)
        {
            return Math.Sqrt(((Math.Abs(x2 - x)) ^ 2) + ((Math.Abs(y2 - y)) ^ 2));
        }

        public static void tileCalcs(int xPos, int yPos, int x, int y, int width, int height, double distSteps, Bitmap g, int rowDepth)
        {
            if (xPos < width && yPos < height && xPos >= 0 && yPos >= 0)
            {
                double landW = landCheck(xPos, yPos);
                double dist = getDistance(xPos, yPos, x, y);
                double wChance = (50 - (dist / distSteps)) + (landW * 8);
                double perc = getPercent();
                if (wChance > perc)
                {
                    drawGroundTile(xPos, yPos, g, dist, rowDepth);
                    if (rd.Next(1, 100) > 50 && xPos > 1 && yPos > 1)
                    {
                        drawGroundTile(xPos + rd.Next(-1, 1), yPos + rd.Next(-1, 1), g, dist, rowDepth);
                        if (rd.Next(1, 100) > 25)
                        {
                            drawGroundTile(xPos + rd.Next(-1, 1), yPos + rd.Next(-1, 1), g, dist, rowDepth);
                        }
                    }
                }
            }
        }

        public static int contWidth = 500;

        public static void drawContinent(int x, int y, Bitmap g, int width, int height)
        {
            drawGroundTile(x, y, g, 0, 0);
            int xPos = x;
            int yPos = y;
            int rowDepth = 1;
            double distSteps = contWidth / 100;
            for (int i = 0; i < contWidth * 2; i++)
            {
                for (int h = 0; h < rowDepth; h++)
                {
                    if (xPos < width && yPos < height && xPos >= 0 && yPos >= 0)
                    {
                        tileCalcs(xPos, yPos, x, y, width, height, distSteps, g, rowDepth);
                    }
                    yPos++;
                }
                for (int h = 0; h < rowDepth; h++)
                {
                    if (xPos < width && yPos < height && xPos > 0 && yPos > 0)
                    {
                        tileCalcs(xPos, yPos, x, y, width, height, distSteps, g, rowDepth);
                    }
                    xPos++;
                }
                rowDepth++;
                for (int h = 0; h < rowDepth; h++)
                {
                    if (xPos < width && yPos < height && xPos > 0 && yPos > 0)
                    {
                        tileCalcs(xPos, yPos, x, y, width, height, distSteps, g, rowDepth);
                    }
                    yPos--;
                }
                for (int h = 0; h < rowDepth; h++)
                {
                    if (xPos < width && yPos < height && xPos > 0 && yPos > 0)
                    {
                        tileCalcs(xPos, yPos, x, y, width, height, distSteps, g, rowDepth);
                    }
                    xPos--;
                }
                rowDepth++;
                if (xPos > 1 && yPos > 1)
                {
                    xPos += rd.Next(-1, 1);
                    yPos += rd.Next(-1, 1);
                }
            }
        }

        public static double landCheck(int x, int y)
        {
            double landTiles = 0;
            try
            {
                if (coordinateGrid[x - 1, y - 1].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x, y - 1].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x + 1, y - 1].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x - 1, y].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x, y].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x + 1, y].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x - 1, y + 1].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x, y + 1].tileType == 1)
                {
                    landTiles++;
                }
                if (coordinateGrid[x + 1, y + 1].tileType == 1)
                {
                    landTiles++;
                }
            }
            catch (Exception e)
            {
                return landTiles;
            }
            return landTiles;
        }

        public static void drawContinentLines(int x, int y, Bitmap g, int width, int height)
        {
            double weight = 1.0;
            drawGroundTile(x, y, g, 0, 0);
            int mainlines = rd.Next(3, contWidth / 5);
            landLine[] mainLinesArray = new landLine[mainlines];
            for (int i = 0; i < mainlines; i++)
            {
                weight = 1.0;
                landLine newLine = randomLine(x, y, weight, g);
                mainLinesArray[i] = newLine;
                drawLine(mainLinesArray[i], g);
                while (weight > 0)
                {
                    for (int j = 0; j < newLine.subLines; j++)
                    {
                        int newX = rd.Next(Math.Min(newLine.startX, newLine.endX), Math.Max(newLine.startX, newLine.endX));
                        int newY = GetY(Math.Min(newLine.startX, newLine.endX), Math.Min(newLine.startY, newLine.endY), Math.Max(newLine.startX, newLine.endX), Math.Max(newLine.startY, newLine.endY), newX);
                        newLine = randomLine(newX, newY, weight, g);
                        drawLine(newLine, g);
                        weight = weight - 0.01;
                    }
                    if (newLine.subLines < 1)
                    {
                        weight = weight - 0.01;
                    }
                }
            }
        }

        public static int GetY(int x1, int y1, int x2, int y2, int x)
        {
            var m = 0;
            if (x2 - x1 > 0)
            {
                m = (y2 - y1) / (x2 - x1); ;
            }
            var b = y1 - (m * x1);

            return m * x + b;
        }

        public static landLine randomLine(int x, int y, double weight, Bitmap g)
        {
            double dist = rd.Next(1, contWidth) * weight;
            double angle = rd.Next(1, 360);
            int subLines = (int)(rd.Next(0, (int)dist) * weight);
            double endX = x + dist * Math.Cos(angle);
            double endY = y + dist * Math.Sin(angle);
            double curve = dist * rd.NextDouble() / 2;
            landLine newLine = new landLine((int)endX, (int)endY, x, y, (int)(rd.Next(0, (int)dist / 4) * weight), 0, subLines, (int)dist, (int)angle);
            return newLine;
        }

        public static void drawLine(landLine line, Bitmap bmp)
        {
            if (line.thickness > 0)
            {
                Pen landPen = new Pen(Color.Green, line.thickness);
                int increments = line.dist / line.thickness;
                int steps = line.thickness;
                double startX = line.startX;
                double startY = line.startY;
                double endX = line.startX + increments * Math.Cos(line.angle);
                double endY = line.startY + increments * Math.Sin(line.angle);
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.DrawLine(landPen, (float)startX, (float)startY, (float)endX, (float)endY);
                    for (int i = 0; i < steps; i++)
                    {
                        startX = endX;
                        startY = endY;
                        endX += (increments + rd.Next(0, 5) - rd.Next(0, 5)) * Math.Cos(line.angle + rd.Next(0, 30) - rd.Next(0, 30));
                        endY += (increments + rd.Next(0, 5) - rd.Next(0, 5)) * Math.Sin(line.angle + rd.Next(0, 30) - rd.Next(0, 30));
                        landPen.Width--;
                        graphics.DrawLine(landPen, (float)startX, (float)startY, (float)endX, (float)endY);
                    }
                }
            }
        }

        public class landLine
        {
            public int endX;
            public int endY;
            public int startX;
            public int startY;
            public int thickness;
            public int curve;
            public int subLines;
            public int dist;
            public int angle;

            public landLine(int endX, int endY, int startX, int startY, int thickness, int curve, int subLines, int dist, int angle)
            {
                this.endX = endX;
                this.endY = endY;
                this.startX = startX;
                this.startY = startY;
                this.thickness = thickness;
                this.curve = curve;
                this.subLines = subLines;
                this.dist = dist;
                this.angle = angle;
            }
        }

        public static double getPercent()
        {
            return rd.Next(100);
        }

        public static void drawGroundTile(int x, int y, Bitmap g, double dist, int rowDepth)
        {
            Color color = Color.FromArgb(Math.Min(255, (0 + rowDepth)), Math.Max(0, (255 - rowDepth)), 0);
            g.SetPixel(x, y, Color.Green);
            coordinateGrid[x, y].tileType = 1;
        }

        public static void drawSeaTile(int x, int y, Bitmap g)
        {
            g.SetPixel(x, y, Color.Blue);
            coordinateGrid[x, y].tileType = 1;
            coordinateGrid[x, y].tileType = 0;
        }

        public static void tileTable(int w, int h)
        {
            coordinateGrid = new tileEntry[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    tileEntry tile = new tileEntry();
                    tile.y = j;
                    tile.x = i;
                    tile.tileType = 0;
                    coordinateGrid[i, j] = tile;
                }
            }
        }

        private class coordPoints
        {
            public int x;
            public int y;
        }

        public class tileEntry
        {
            public int x;
            public int y;
            public int tileType;
        }

        public static int regionCount = 199;
        public static Vector2[] points = new Vector2[regionCount];

        public static byte[] CreateGridImage(int maxXCells, int maxYCells, int continents)
        {
            tileTable(maxXCells, maxYCells);
            coordPoints[] continentPoints = new coordPoints[continents];
            Random rd = new Random();
            for (int i = 0; i < continents; i++)
            {
                coordPoints point = new coordPoints();
                point.x = rd.Next(0, maxXCells);
                point.y = rd.Next(0, maxYCells);
                continentPoints[i] = point;
            }
            using (var bmp = new System.Drawing.Bitmap(maxXCells, maxYCells))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Blue);
                    {
                        for (int i = 0; i < continents; i++)
                        {
                            drawContinentLines(continentPoints[i].x, continentPoints[i].y, bmp, maxXCells, maxYCells);
                        }
                    }

                    double[,] kernel = GaussianBlur(blurLength, blurWeight);
                    Bitmap newbmp = Convolve(bmp, kernel);
                    for (int x = maxXCells - 1; x > 0; x--)
                    {
                        for (int y = 9; y >= 0; y--)
                        {
                            newbmp.SetPixel(x, y, newbmp.GetPixel(x, y + 1));
                        }
                    }
                    for (int x = maxXCells - 9; x < maxXCells; x++)
                    {
                        for (int y = 0; y < maxYCells; y++)
                        {
                            newbmp.SetPixel(x, y, newbmp.GetPixel(x - 1, y));
                        }
                    }
                    for (int x = 0; x < maxXCells; x++)
                    {
                        for (int y = maxYCells - 9; y < maxYCells; y++)
                        {
                            newbmp.SetPixel(x, y, newbmp.GetPixel(x, y - 1));
                        }
                    }
                    for (int x = 9; x >= 0; x--)
                    {
                        for (int y = 0; y < maxYCells; y++)
                        {
                            newbmp.SetPixel(x, y, newbmp.GetPixel(x + 1, y));
                        }
                    }
                    for (int i = 0; i < newbmp.Width; i++)
                    {
                        for (int j = 0; j < newbmp.Height; j++)
                        {
                            Color pixel = newbmp.GetPixel(i, j);
                            if (pixel.G > 20)
                            {
                                newbmp.SetPixel(i, j, Color.Green);
                                coordinateGrid[i, j].tileType = 1;
                                coordinateGrid[i, j].x = i;
                                coordinateGrid[i, j].y = j;
                            }
                            else
                            {
                                newbmp.SetPixel(i, j, Color.Blue);
                                coordinateGrid[i, j].tileType = 0;
                                coordinateGrid[i, j].x = i;
                                coordinateGrid[i, j].y = j;
                            }
                        }
                    }
                    var memStream = new MemoryStream();
                    newbmp.Save("myMap.tif", ImageFormat.Tiff);

                    List<tileEntry> landTiles = new List<tileEntry>();

                    foreach (tileEntry tile in coordinateGrid)
                    {
                        if (tile.tileType == 1)
                        {
                            landTiles.Add(tile);
                        }
                    }
                    int n = landTiles.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rd.Next(n + 1);
                        tileEntry value = landTiles[k];
                        landTiles[k] = landTiles[n];
                        landTiles[n] = value;
                    }

                    var platebmp = new System.Drawing.Bitmap(maxXCells, maxYCells);
                    using (Graphics gplate = Graphics.FromImage(platebmp))
                    {
                        gplate.Clear(Color.Black);
                        int platecount = regionCount;
                        points = new Vector2[regionCount];
                        Color[] colors = new Color[platecount];
                        float[] colorhues = new float[platecount];
                        int pixels = maxXCells * maxYCells - 2;
                        for (int i = 0; i < platecount; i++)
                        {
                            do
                            {
                                points[i] = new Vector2(rd.Next(1, maxXCells - 1), rd.Next(1, maxYCells - 1));
                            } while (coordinateGrid[(int)points[i].x, (int)points[i].y].tileType == 0);
                            do
                            {
                                colors[i] = Color.FromArgb(rd.Next(5, 250), rd.Next(5, 255), rd.Next(0, 250));
                            } while (colors[i].GetHue() > 180 && colors[i].GetHue() < 300);
                            platebmp.SetPixel((int)points[i].x + 1, (int)points[i].y + 1, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x + 1, (int)points[i].y, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x + 1, (int)points[i].y - 1, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x, (int)points[i].y + 1, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x, (int)points[i].y, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x, (int)points[i].y - 1, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x - 1, (int)points[i].y + 1, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x - 1, (int)points[i].y, colors[i]);
                            pixels--;
                            platebmp.SetPixel((int)points[i].x - 1, (int)points[i].y - 1, colors[i]);
                            pixels--;
                        }

                        bool gotBlack = false;
                        int printlooper = 0;
                        while (pixels > 0)
                        {
                            gotBlack = false;
                            var i = rd.Next(0, maxXCells);
                            var j = rd.Next(0, maxYCells);
                            Color selfa = Color.Black;
                            selfa = platebmp.GetPixel(i, j);
                            if (selfa.G + selfa.R + selfa.B == 0)
                            {
                                continue;
                            }
                            Color ngbourN = Color.Black;
                            Color ngbourS = Color.Black;
                            Color ngbourE = Color.Black;
                            Color ngbourW = Color.Black;
                            if (j + 1 < platebmp.Height)
                            {
                                ngbourN = platebmp.GetPixel(i, j + 1);
                                if (ngbourN.G + ngbourN.R + ngbourN.B == 0)
                                {
                                    platebmp.SetPixel(i, j + 1, selfa);
                                    pixels--;
                                }
                            }
                            if (j - 1 >= 0)
                            {
                                ngbourS = platebmp.GetPixel(i, j - 1);
                                if (ngbourS.G + ngbourS.R + ngbourS.B == 0)
                                {
                                    platebmp.SetPixel(i, j - 1, selfa);
                                    pixels--;
                                }
                            }
                            if (i + 1 < platebmp.Width)
                            {
                                ngbourE = platebmp.GetPixel(i + 1, j);
                                if (ngbourE.G + ngbourE.R + ngbourE.B == 0)
                                {
                                    platebmp.SetPixel(i + 1, j, selfa);
                                    pixels--;
                                }
                            }
                            if (i - 1 >= 0)
                            {
                                ngbourW = platebmp.GetPixel(i - 1, j);
                                if (ngbourW.G + ngbourW.R + ngbourW.B == 0)
                                {
                                    platebmp.SetPixel(i - 1, j, selfa);
                                    pixels--;
                                }
                            }
                            printlooper++;
                            if (printlooper > maxXCells * 40)
                            {
                                printlooper = 0;
                                try
                                {
                                    platebmp.Save("myMapPlates.tif", ImageFormat.Tiff);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    for (int i = 0; i < maxXCells; i++)
                    {
                        for (int j = 0; j < maxYCells; j++)
                        {
                            if (coordinateGrid[i, j].tileType == 0)
                            {
                                platebmp.SetPixel(i, j, Color.Blue);
                            }
                        }
                    }
                    foreach (Vector2 point in points)
                    {
                        platebmp.SetPixel((int)point.x, (int)point.y, Color.Black);
                    }
                    try
                    {
                        platebmp.Save("myMapPlates.tif", ImageFormat.Tiff);
                    }
                    catch
                    {
                        platebmp.Save("myMapPlates.tif", ImageFormat.Tiff);
                    }

                    return memStream.ToArray();
                }
            }
        }

        public static void Shuffle<T>(Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static int blurLength = 20;
        public static double blurWeight = 50;

        public static double[,] GaussianBlur(int lenght, double weight)
        {
            double[,] kernel = new double[lenght, lenght];
            double kernelSum = 0;
            int foff = (lenght - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * weight * weight);
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                }
            }
            for (int y = 0; y < lenght; y++)
            {
                for (int x = 0; x < lenght; x++)
                {
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }
            return kernel;
        }

        public static Bitmap Convolve(Bitmap srcImage, double[,] kernel)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);
            int colorChannels = 3;
            double[] rgb = new double[colorChannels];
            int foff = (kernel.GetLength(0) - 1) / 2;
            int kcenter = 0;
            int kpixel = 0;
            for (int y = foff; y < height - foff; y++)
            {
                for (int x = foff; x < width - foff; x++)
                {
                    for (int c = 0; c < colorChannels; c++)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < colorChannels; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                            }
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}