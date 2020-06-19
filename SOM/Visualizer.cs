using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOM
{
    class Visualizer
    {
        /*
         source for hexagons: http://csharphelper.com/blog/2015/10/draw-a-hexagonal-grid-in-c/
             */


        private PictureBox picGrid, scaleBar;
        // The height of a hexagon.
        private const float HexHeight = 25;
        //hexagon row and col counts
        private int hexCountR, hexCountC;

        public Visualizer(PictureBox pictureBox, int hexCountR, int hexCountC,PictureBox scalePic)
        {
            picGrid = pictureBox;
            scaleBar = scalePic;
            this.hexCountC = hexCountC;
            this.hexCountR = hexCountR;
            //change picture box size by formula
            picGrid.Size = new Size((int)(hexCountC * HexHeight - (HexHeight * 2)),
                (int)((hexCountR + 1) * HexHeight + 2));
            scaleBar.Size = new Size(picGrid.Size.Width, scaleBar.Size.Height);
            scaleBar.Paint += paintScaleBar;
            picGrid.Paint += picGrid_Paint;
            picGrid.MouseClick += picGrid_MouseClick;
            picGrid.Invalidate();
        }

        // Redraw the grid.
        private void picGrid_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Draw the grid.
            DrawHexGrid(e.Graphics, Pens.Black,
                0, picGrid.ClientSize.Width,
                0, picGrid.ClientSize.Height,
                HexHeight);
        }

        // Draw a hexagonal grid for the indicated area.
        // (You might be able to draw the hexagons without
        // drawing any duplicate edges, but this is a lot easier.)
        private void DrawHexGrid(Graphics gr, Pen pen,
            float xmin, float xmax, float ymin, float ymax,
            float height)
        {
            // Loop until a hexagon won't fit.
            for (int row = 0; ; row++)
            {
                // Get the points for the row's first hexagon.
                PointF[] points = HexToPoints(height, row, 0);

                // If it doesn't fit, we're done.
                if (points[4].Y > ymax) break;

                // Draw the row.
                for (int col = 0; ; col++)
                {
                    // Get the points for the row's next hexagon.
                    points = HexToPoints(height, row, col);

                    // If it doesn't fit horizontally,
                    // we're done with this row.
                    if (points[3].X > xmax) break;

                    // If it fits vertically, draw it.
                    if (points[4].Y <= ymax)
                    {
                        if ((row != 0 || col % 4 != 2) && (row != hexCountR || col % 4 != 0))
                        {
                            gr.DrawPolygon(pen, points);
                        }
                    }
                }
            }
        }

        private void picGrid_Resize(object sender, EventArgs e)
        {
            picGrid.Refresh();
        }
        //Display informations about the clicked hexagon(if hexagon is an output node)
        private void picGrid_MouseClick(object sender, MouseEventArgs e)
        {
            int row, col;
            PointToHex(e.X, e.Y, HexHeight, out row, out col);
            Point clicked = nodeMap.FirstOrDefault(x => x.Value == new Point(row, col)).Key;
            if (!clicked.IsEmpty || (row == 0 && col == 0))
            {
                String temp = "\t Record No (" + SOM.outputs[clicked.X][clicked.Y].records.Count + ")\n";
                int i = 0;
                double weight;
                foreach (int recNo in SOM.outputs[clicked.X][clicked.Y].records)
                {
                    temp += recNo + "\t";
                    i++;
                    if (i % 5 == 0)
                        temp += "\n";
                }
                temp += "\n\t Weights \n";
                for (i = 0; i < SOM.fields.Length; i++)
                {
                    if (SOM.fields[i].type == 1)
                    {
                        weight = (SOM.outputs[clicked.X][clicked.Y].weights[i] * (SOM.fields[i].max - SOM.fields[i].min)) + SOM.fields[i].min;
                        temp += SOM.fields[i].name + " \t" + weight + "\n";
                    }
                    else
                    {
                        temp += SOM.fields[i].name + " \t" + SOM.fields[i].nominalMap.FirstOrDefault(x => x.Value == SOM.outputs[clicked.X][clicked.Y].weights[i]).Key + "\n";
                    }
                }
                MessageBox.Show(temp, "Output Node[" + clicked.X + "][" + clicked.Y + "]");
            }
        }

        // Return the width of a hexagon.
        private float HexWidth(float height)
        {
            return (float)(4 * (height / 2 / Math.Sqrt(3)));
        }

        // Return the row and column of the hexagon at this point.
        private void PointToHex(float x, float y, float height,
            out int row, out int col)
        {
            // Find the test rectangle containing the point.
            float width = HexWidth(height);
            col = (int)(x / (width * 0.75f));

            if (col % 2 == 0)
                row = (int)(y / height);
            else
                row = (int)((y - height / 2) / height);

            // Find the test area.
            float testx = col * width * 0.75f;
            float testy = row * height;
            if (col % 2 == 1) testy += height / 2;

            // See if the point is above or
            // below the test hexagon on the left.
            bool is_above = false, is_below = false;
            float dx = x - testx;
            if (dx < width / 4)
            {
                float dy = y - (testy + height / 2);
                if (dx < 0.001)
                {
                    // The point is on the left edge of the test rectangle.
                    if (dy < 0) is_above = true;
                    if (dy > 0) is_below = true;
                }
                else if (dy < 0)
                {
                    // See if the point is above the test hexagon.
                    if (-dy / dx > Math.Sqrt(3)) is_above = true;
                }
                else
                {
                    // See if the point is below the test hexagon.
                    if (dy / dx > Math.Sqrt(3)) is_below = true;
                }
            }

            // Adjust the row and column if necessary.
            if (is_above)
            {
                if (col % 2 == 0) row--;
                col--;
            }
            else if (is_below)
            {
                if (col % 2 == 1) row++;
                col--;
            }
        }

        // Return the points that define the indicated hexagon.
        private PointF[] HexToPoints(float height, float row, float col)
        {
            // Start with the leftmost corner of the upper left hexagon.
            float width = HexWidth(height);
            float y = height / 2;
            float x = 0;

            // Move down the required number of rows.
            y += row * height;

            // If the column is odd, move down half a hex more.
            if (col % 2 == 1) y += height / 2;

            // Move over for the column number.
            x += col * (width * 0.75f);

            // Generate the points.
            return new PointF[]
                {
                    new PointF(x, y),
                    new PointF(x + width * 0.25f, y - height / 2),
                    new PointF(x + width * 0.75f, y - height / 2),
                    new PointF(x + width, y),
                    new PointF(x + width * 0.75f, y + height / 2),
                    new PointF(x + width * 0.25f, y + height / 2),
                };
        }
        Dictionary<Point, Point> nodeMap = new Dictionary<Point, Point>();
        public void printU_Matrix()
        {
            Graphics graphic = picGrid.CreateGraphics();
            int i = 0, j = 0, mapI;
            double min = double.MaxValue, max = double.MinValue;
            bool flag = false;
            int temp;
            PointF[] temp2;
            //create map for output node index -> hexagon index
            for (i = 0; i < SOM.outputR; i++)
            {
                temp = i * 2;
                mapI = temp;
                for (j = 0; j < SOM.outputC; j++)
                {
                    nodeMap[new Point(i, j)] = new Point(mapI, j * 2);
                    if (!flag)
                    {
                        mapI = temp + 1;
                        flag = true;
                    }
                    else
                    {
                        mapI = temp;
                        flag = false;
                    }
                }
            }
            Point p1Val, p2Val, p1Key, p2Key;
            Dictionary<Point, double> distMap = new Dictionary<Point, double>();
            int hexI, hexJ;
            //find all hexagons between output nodes(hexagons) and calculate distances
            foreach (KeyValuePair<Point, Point> node1 in nodeMap)
            {
                foreach (KeyValuePair<Point, Point> node2 in nodeMap)
                {
                    p1Val = node1.Value;
                    p2Val = node2.Value;
                    temp = Math.Abs(p1Val.X - p2Val.X) + Math.Abs(p1Val.Y - p2Val.Y);
                    if (temp != 3 && temp != 2)
                        continue;
                    p1Key = node1.Key;
                    p2Key = node2.Key;
                    //find hexagon between p1Val-p2Val
                    if (p1Val.X < p2Val.X)
                    {
                        if (p1Val.Y < p2Val.Y)
                        {
                            hexI = p1Val.X;
                            hexJ = p1Val.Y + 1;
                        }
                        else if (p1Val.Y > p2Val.Y)
                        {
                            hexI = p1Val.X;
                            hexJ = p1Val.Y - 1;
                        }
                        else
                        {
                            hexI = p1Val.X + 1;
                            hexJ = p1Val.Y;
                        }
                        temp2 = HexToPoints(HexHeight, hexI, hexJ);
                        distMap[new Point(hexI, hexJ)] = SOM.EuclideanDist(SOM.outputs[p1Key.X][p1Key.Y].weights, SOM.outputs[p2Key.X][p2Key.Y].weights);
                        if (distMap[new Point(hexI, hexJ)] > max)
                            max = distMap[new Point(hexI, hexJ)];
                        if (distMap[new Point(hexI, hexJ)] < min)
                            min = distMap[new Point(hexI, hexJ)];
                    }
                }
            }
            int val;
            int rgbMax = 240, rgbMin = 15;
            Dictionary<Point, int> distColorMap = new Dictionary<Point, int>();
            //color all hexagons between outputs nodes(hexagons)
            foreach (KeyValuePair<Point, double> hex in distMap)
            {
                val = (int)Math.Round((hex.Value - min) / (max - min) * (rgbMax - rgbMin) + rgbMin);
                distColorMap[hex.Key] = val;
                temp2 = HexToPoints(HexHeight, hex.Key.X, hex.Key.Y);
                //val = rgbMax - val+rgbMin;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(val, val, val)))
                    graphic.FillPolygon(brush, temp2);
            }
            double temp3;
            int count;
            //color all hexagons that are output nodes by calculating mean of adjacent ones
            foreach (KeyValuePair<Point, Point> node in nodeMap)
            {
                hexI = node.Value.X;
                hexJ = node.Value.Y;
                temp3 = 0;
                count = 0;
                if (distColorMap.ContainsKey(new Point(hexI - 1, hexJ)))
                {
                    temp3 += distColorMap[new Point(hexI - 1, hexJ)];
                    count++;
                }
                if (distColorMap.ContainsKey(new Point(hexI + 1, hexJ)))
                {
                    temp3 += distColorMap[new Point(hexI + 1, hexJ)];
                    count++;
                }
                if (distColorMap.ContainsKey(new Point(hexI - 1, hexJ - 1)))
                {
                    temp3 += distColorMap[new Point(hexI - 1, hexJ - 1)];
                    count++;
                }
                if (distColorMap.ContainsKey(new Point(hexI - 1, hexJ + 1)))
                {
                    temp3 += distColorMap[new Point(hexI - 1, hexJ + 1)];
                    count++;
                }
                if (distColorMap.ContainsKey(new Point(hexI, hexJ - 1)))
                {
                    temp3 += distColorMap[new Point(hexI, hexJ - 1)];
                    count++;
                }
                if (distColorMap.ContainsKey(new Point(hexI, hexJ + 1)))
                {
                    temp3 += distColorMap[new Point(hexI, hexJ + 1)];
                    count++;
                }
                val = (int)Math.Round(temp3 / count);
                temp2 = HexToPoints(HexHeight, hexI, hexJ);
                //val = rgbMax - val+rgbMin;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(val, val, val)))
                    graphic.FillPolygon(brush, temp2);
            }
            //write record counts to output nodes(hexagons)
            foreach (KeyValuePair<Point, Point> node in nodeMap)
            {
                using (Font myFont = new Font("Arial", 8, FontStyle.Bold))
                {
                    graphic.DrawString(SOM.outputs[node.Key.X][node.Key.Y].records.Count.ToString(), myFont, Brushes.Blue, HexToPoints(HexHeight, node.Value.X, node.Value.Y)[1]);
                }
            }
        }

        /*
            source for scale bar: https://rosettacode.org/wiki/Greyscale_bars/Display#C
             */
        //create grayscale bar
        private static Bitmap ColorBars(Rectangle size)
        {
            int ColorCount = 256;
            var colorBars = new Bitmap(size.Width, size.Height);
            Func<int, int, int> forwardColor = (x, divs) => (int)(x * ((float)divs / size.Width)) * ColorCount / divs;
            Func<int, int, int> reverseColor = (x, divs) => ColorCount - 1 - forwardColor(x, divs);
            Action<int, int, int> setGray = (x, y, gray) => colorBars.SetPixel(x, y, Color.FromArgb(gray,gray,gray));
            Action<int, int, int> setReverse = (x, y, divs) => setGray(x, y, reverseColor(x, divs));
            int verticalStripe = size.Height;
            for (int x = 0; x < size.Width; x++)
                for (int y = 0; y < verticalStripe; y++)
                    setReverse(x, y, 128);
            return colorBars;
        }
        private void paintScaleBar(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(ColorBars(new Rectangle(0, 0, scaleBar.Width, scaleBar.Height)), new Point(0, 0));
        }
    }
}
