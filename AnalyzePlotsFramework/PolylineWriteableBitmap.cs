
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using GDI = System.Drawing;
using Matrix = System.Windows.Media.Matrix;
using Drawing2D = System.Drawing.Drawing2D;


namespace AnalyzePlotsFramework
{
    public class PolylineWriteableBitmap : FrameworkElement, INotifyPropertyChanged
    {
        private readonly List<Visual> visuals = new List<Visual>();
        private DrawingVisual Layer;

        private WriteableBitmap myBitmap;
        private int myBitmapWidth = 0;
        private int myBitmapHeight = 0;


        private float offset_x = 0;//滑动条偏移值
        private float myYScale;
        private float myXScale;

        private float myTotalMaxY = 200;
        private float myTotalMinY = 200;
        private float myMaxY = 200;
        private float myMinY = 0;
        private float myCurrentYRange = 200;
        private float myMinYStep = 50;

        private int myTotalMaxX = 200;
        private int myTotalMinX = 200;
        private int myMaxX = 200;
        private int myMinX = 0;
        private int myCurrentXRange = 100;
        private int myMinXStep = 80;

        private static int Top_Val_Max = 100;//y轴最大值
        private static int Top_Val_Min = 0;//y轴最小值

        private int myMarginLeft = 50;
        private int myMarginRight = 50;
        private int myMarginBottom = 30;

        GDI.Pen primarygrid_pen = new GDI.Pen(GDI.Color.Gray, 1f);

        private Font myFont;

        private List<PointLocation> myPointList = new List<PointLocation>();

        private PointLocation mySelectedPointLocation = new PointLocation();

        public PointLocation SelectedPointLocation
        {
            get { return mySelectedPointLocation; }
            set
            {
                mySelectedPointLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPointLocation)));
            }
        }



        public PolylineWriteableBitmap()
        {
            myFont = new Font("Times New Roman", 12.0f);

            Layer = new DrawingVisual();
            visuals.Add(Layer);

            this.PreviewMouseLeftButtonDown += PolylineWriteableBitmap_PreviewMouseLeftButtonDown;
            this.MouseWheel += PolylineWriteableBitmap_MouseWheel;
        }

        private void PolylineWriteableBitmap_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var position = e.GetPosition(this);
            var point = new PointF((float)position.X, (float)(RenderSize.Height - position.Y));

            //var renderPoint = new PointF((float)(linePoint.X - myMinX) * myXScale + myMarginLeft,
            //    (float)((linePoint.Y - myMinY) * myYScale + myMarginBottom));
            var orgPoint = new PointF((point.X - myMarginLeft) / myXScale + myMinX,
                (point.Y - myMarginBottom) / myYScale + myMinY);

            float scale = 1f;
            int centerX = 0;
            float centerY = 0;
            if (e.Delta > 0)
            {
                //room in
                scale = 1.5f;
                centerX = (int)orgPoint.X;
                centerY = orgPoint.Y;
            }
            else
            {
                centerX = (myMaxX - myMinX) / 2;
                centerY = (myMaxY - myMaxY) / 2;
                scale = 0.7f;
            }
            
            var leftX = (int)((myMaxX - centerX) / scale);
            var rightX = (int)((centerX - myMinX) / scale);
            var rangeX = leftX < rightX ? leftX : rightX;
            var maxX = centerX + rangeX;
            myMaxX = maxX >= myTotalMaxX ? myTotalMaxX : maxX;
            var minX = centerX - rangeX;
            myMinX = minX <= myTotalMinX ? myTotalMinX : minX;
            myCurrentXRange = myMaxX - myMinX;

           
            var topY = (myMaxY - centerY) / scale;
            var bottomY = (centerY - myMinY) / scale;
            var rangeY = topY < bottomY ? topY : bottomY;
            var maxY = centerY + rangeY;
            myMaxY = maxY >= myTotalMaxY ? myTotalMaxY : maxY;
            var minY = centerY - rangeY;
            myMinY = minY <= myTotalMinY ? myTotalMinY : minY;
            myCurrentYRange = myMaxY - myMinY;

            DrawContent();
        }

        private void PolylineWriteableBitmap_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var point = new PointF((float)position.X, (float)(RenderSize.Height - position.Y));


            var nearestPoint = GetSelectedPoint(point);
            SelectedPointLocation = nearestPoint;
            if (SelectedPointLocation.Location != new PointF(0, 0))
            {
                //MessageBox.Show($"Name:{nearestPoint.Line}, X:{nearestPoint.Location.X}, Y:{nearestPoint.Location.Y}");
                InitBitmap();
                DrawContent();
            }

        }

        public PointLocation GetSelectedPoint(PointF point)
        {
            var nearestPoint = myPointList.FirstOrDefault(p => Math.Sqrt(Math.Pow(p.RenderLocation.X - point.X, 2) + Math.Pow(p.RenderLocation.Y - point.Y, 2)) < 10);

            if (nearestPoint == null)
            {
                return new PointLocation();
            }

            return nearestPoint;

        }

        protected override int VisualChildrenCount => visuals.Count;
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            DrawContent();
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            InitBitmap();
            if (this.myBitmap != null)
            {
                drawingContext.DrawImage(myBitmap, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
            }
            base.OnRender(drawingContext);
        }

        private List<PolylineData> myPolyLines = new List<PolylineData>();

        //private int FindClosestDivisibleBy100(int number)
        //{
        //    int quotient = number / 100;
        //    int closestNumber = (quotient + 1) * 100;

        //    return closestNumber;
        //}

        public void GreateLines()
        {
            for (int i = 0; i < 200; i++)
            {
                this.myPolyLines.Add(new PolylineData($"line{i}", GetLineData()));
            }

            myMaxY = (int)myPolyLines.Max(it => it.Points.Max(p => p.Y));
            myTotalMaxY = myMaxY;
            myMinY = 0;
            myTotalMinY = myMinX;
            myCurrentYRange = myMaxY - myMinY;

            myMaxX = (int)myPolyLines.Max(it => it.Points.Max(p => p.X));
            myTotalMaxX = myMaxX;
            myMinX = 0;
            myTotalMinX = myMinX;
            myCurrentXRange = myMaxX - myMinX;

            DrawContent();
        }
        static Random random = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private List<PointF> GetLineData()
        {
            List<PointF> points = new List<PointF>();

            for (int i = 0; i <= 2000; i++)
            {
                var y = random.Next(400, 700);
                var rate = random.Next(1, 10000);
                if (rate < 2)
                {
                    y = y * random.Next(3, 4);
                }
                else if (rate > 9998)
                {
                    y = y * random.Next(0, 1);
                }

                points.Add(new PointF(i, y));
            }

            return points;
        }

        public int GetDecimalShifts(float num)
        {
            int counts = 0;

            if (num < 1 && num > 0)
            {
                while (num < 1 && num > 0)
                {
                    counts--;
                    num *= 10;
                }
            }
            else if (num >= 10)
            {
                while (num >= 10)
                {
                    counts++;
                    num /= 10;
                }
            }

            return counts;
        }

        private int GetBestStep(int distanceBetweenValues)
        {
            var decimalShifts = GetDecimalShifts(distanceBetweenValues);
            var possibleSteps = new int[] { 1, 2, 5 };
            var currStep = distanceBetweenValues * Math.Pow(10, -decimalShifts);

            var bestStep = possibleSteps[0];
            var bestDistance = 1000000;

            for (var i = 0; i < possibleSteps.Length; i++)
            {
                var tmpDistance = (int)Math.Abs(possibleSteps[i] - currStep);
                if (bestDistance > tmpDistance)
                {
                    bestDistance = tmpDistance;
                    bestStep = possibleSteps[i];
                }
            }
            int step = (int)(bestStep * Math.Pow(10, decimalShifts));

            return step;
        }

        private void DrawXAxis(Graphics backBufferGraphics)
        {
            //XAxis
            PointF pointX1 = new PointF(myMarginLeft, myMarginBottom);
            PointF pointX2 = new PointF((float)RenderSize.Width - myMarginRight, myMarginBottom);
            backBufferGraphics.DrawLine(primarygrid_pen, pointX1, pointX2);

            var numberOfScalingX = (int)Math.Floor((RenderSize.Width - myMarginLeft - myMarginRight) / myMinXStep);
            var distance = (int)(Math.Floor((float)(myMaxX - myMinX) / numberOfScalingX));

            var step = GetBestStep(distance);

            //x scale
            for (int x = myMinX; x <= myMaxX; x += step)
            {
                PointF point1 = new PointF((x - myMinX) * myXScale + myMarginLeft, -10 + myMarginBottom);
                PointF point2 = new PointF((x - myMinX) * myXScale + myMarginLeft, 10 + myMarginBottom);

                backBufferGraphics.DrawLine(primarygrid_pen, point1, point2);

                var text = new FormattedText(x + "", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 16, Brushes.White);
                var mat = new Drawing2D.Matrix();
                mat.Scale(1, 1);
                mat.Translate((float)-text.Width / 2, (float)RenderSize.Height - 25);
                backBufferGraphics.Transform = mat;
                backBufferGraphics.DrawString($"{x}", myFont, GDI.Brushes.Red, new PointF((x - myMinX) * myXScale + myMarginLeft, 8));
                backBufferGraphics.Transform = BackToDefaultTransfom();

            }
        }

        private void DrawYAxis(Graphics backBufferGraphics)
        {
            var pointY1 = new PointF(myMarginLeft, myMarginBottom);
            var pointY2 = new PointF(myMarginLeft, myCurrentYRange * myYScale + myMarginBottom);
            backBufferGraphics.DrawLine(primarygrid_pen, pointY1, pointY2);

            var numberOfScalingY = (int)Math.Floor((RenderSize.Height - myMarginBottom) / myMinYStep);
            var distance = (int)Math.Floor(myMaxY / numberOfScalingY);
            var step = GetBestStep(distance);

            for (int y = step; y <= myMaxY; y += step)
            {
                PointF point1 = new PointF(-10 + myMarginLeft, (y - myMinY) * myYScale + myMarginBottom);
                PointF point2 = new PointF(10 + myMarginLeft, (y - myMinY) * myYScale + myMarginBottom);

                backBufferGraphics.DrawLine(primarygrid_pen, point1, point2);

                var text = new FormattedText(y + "", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 12, Brushes.White);

                var mat = new System.Drawing.Drawing2D.Matrix();
                mat.Scale(1, 1);

                backBufferGraphics.Transform = mat;
                backBufferGraphics.DrawString($"{y}", myFont, GDI.Brushes.Red, new PointF(2, (float)(RenderSize.Height - (y - myMinY - Top_Val_Min) * myYScale - myMarginBottom) - (float)text.Height * 2 / 3));
                backBufferGraphics.Transform = BackToDefaultTransfom();
            }

        }

        private Drawing2D.Matrix BackToDefaultTransfom()
        {
            var mat = new Drawing2D.Matrix();
            mat.Scale(1, -1);
            mat.Translate(0, (float)-RenderSize.Height);
            return mat;
        }

        private void DrawContent()
        {
            if (this.myBitmap == null || myPolyLines.Count == 0)
            {
                return;
            }

            myYScale = (float)Math.Round((RenderSize.Height - myMarginBottom) / myCurrentYRange, 2);
            myXScale = (float)Math.Round((RenderSize.Width - myMarginLeft - myMarginRight) / myCurrentXRange, 2);
            this.myBitmap.Lock();

            using (Bitmap backBufferBitmap = new Bitmap(this.myBitmapWidth, this.myBitmapHeight,
                       this.myBitmap.BackBufferStride, GDI.Imaging.PixelFormat.Format24bppRgb,
                       this.myBitmap.BackBuffer))
            {
                using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                {
                    backBufferGraphics.Transform = BackToDefaultTransfom();
                    backBufferGraphics.Clear(GDI.Color.FromArgb(255, 33, 32, 32));
                    DrawXAxis(backBufferGraphics);
                    DrawYAxis(backBufferGraphics);
                    DrawPoints(backBufferGraphics, myPolyLines);
                    DrawSelectPoint(backBufferGraphics);
                }
            }

            this.myBitmap.AddDirtyRect(new Int32Rect(0, 0, this.myBitmapWidth, this.myBitmapHeight));
            this.myBitmap.Unlock();

        }

        private void DrawPoints(Graphics backBufferGraphics, List<PolylineData> lines)
        {
            if (lines != null)
            {
                myPointList.Clear();
                foreach (var line in lines)
                {
                    //var linePoints =
                    //    line.Points.Select(it => new PointF((float)it.X * myXScale + myMarginLeft, (float)(it.Y * myYScale + myMarginBottom)));

                    List<PointF> linePointsList = new List<PointF>();

                    foreach (var linePoint in line.Points)
                    {

                        if (linePoint.X <= myMaxX && linePoint.X >= myMinX && linePoint.Y >= myMinY &&
                            linePoint.Y <= myMaxY)
                        {
                            var renderPoint = new PointF((float)(linePoint.X - myMinX) * myXScale + myMarginLeft,
                                (float)((linePoint.Y - myMinY) * myYScale + myMarginBottom));

                            linePointsList.Add(renderPoint);
                            myPointList.Add(new PointLocation(line.Name, linePoint, renderPoint, new PointF(renderPoint.X + 5, (float)(RenderSize.Height - renderPoint.Y + 5))));
                        }
                    }

                    if (linePointsList.Count > 1)
                    {
                        backBufferGraphics.DrawLines(line.Pen, linePointsList.ToArray());

                    }
                }

            }
        }

        private void DrawSelectPoint(Graphics backBufferGraphics)
        {
            if (SelectedPointLocation.Line == "NaN")
            {
                return;
            }
            var realPoint = new PointF((float)SelectedPointLocation.Location.X * myXScale + myMarginLeft,
                (float)(SelectedPointLocation.Location.Y * myYScale + myMarginBottom));
            backBufferGraphics.FillEllipse(GDI.Brushes.Red, realPoint.X - 3, realPoint.Y - 3, 6, 6);
        }

        private void InitBitmap()
        {
            if (myBitmap == null || this.myBitmap.Width != (int)this.ActualWidth || this.myBitmap.Height != (int)this.ActualHeight)
            {
                if ((int)this.ActualWidth > 0 && (int)this.ActualHeight > 0)
                {
                    this.myBitmapWidth = (int)this.ActualWidth;
                    this.myBitmapHeight = (int)this.ActualHeight;
                    this.myBitmap = new WriteableBitmap(myBitmapWidth, myBitmapHeight, 96, 96, PixelFormats.Bgr24, null);
                    this.myBitmap.Lock();
                    using (Bitmap backBufferBitmap = new Bitmap(myBitmapWidth, myBitmapHeight,
                               this.myBitmap.BackBufferStride, GDI.Imaging.PixelFormat.Format24bppRgb,
                               this.myBitmap.BackBuffer))
                    {
                        using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                        {
                            backBufferGraphics.SmoothingMode = SmoothingMode.HighSpeed;
                            backBufferGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                            backBufferGraphics.Clear(GDI.Color.FromArgb(255, 33, 32, 32));
                            backBufferGraphics.Flush();
                        }
                    }
                    this.myBitmap.AddDirtyRect(new Int32Rect(0, 0, myBitmapWidth, myBitmapHeight));
                    this.myBitmap.Unlock();
                }
            }
        }
    }

    public class PointLocation
    {
        public string Line { get; set; }
        public PointF Location { get; set; }
        public PointF RenderLocation { get; set; }
        public PointF RealLocation { get; set; }

        public bool IsSelected { get; } = true;

        public PointLocation(string line, PointF location, PointF renderLocation, PointF realLocation)
        {
            this.Line = line;
            this.Location = location;
            RenderLocation = renderLocation;
            RealLocation = realLocation;
        }

        public PointLocation()
        {
            this.Line = "NaN";
            IsSelected = false;
            this.Location = new PointF(0, 0);
        }
    }
}
