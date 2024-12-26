using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using GDI = System.Drawing;

namespace AnalyzePlotsFramework
{
    public class PolylineData : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public GDI.Pen Pen { get; }

        private List<PointF> myPoints;

        public List<PointF> Points
        {
            get { return myPoints; }
            set
            {
                myPoints = value;
                OnPropertyChanged(nameof(Points));
            }
        }

        public PolylineData(string name, List<PointF> points)
        {
            this.Name = name;
            this.Points = points;
            this.Pen = GetPen();
        }

        static Random random = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private GDI.Pen GetPen()
        {
            return new GDI.Pen(GDI.Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)), 1f);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

      
    }
}