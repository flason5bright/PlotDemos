using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Axes;

namespace OxyPlotDemo
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            this.MyModel = new PlotModel { Title = "Example 1" };
            //this.MyModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));

            for (int j = 0; j < 200; j++)
            {
                var lineSeries = new LineSeries() { MarkerType = MarkerType.None };
                var random = new Random();
                lineSeries.Color = OxyColor.FromArgb((byte)random.Next(0, 255), (byte)random.Next(0, 255),
                    (byte)random.Next(0, 255), (byte)random.Next(0, 255));
                for (int x = 1; x <= 840; x++)
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

                    lineSeries.Points.Add(new DataPoint(x, y));
                }

                MyModel.Series.Add(lineSeries);
            }
         
           // MyModel.Axes.Add(new LinearColorAxis { Position = AxisPosition.Right, Palette = OxyPalettes.Jet(200) });
        }

        public PlotModel MyModel { get; private set; }
    }
}
