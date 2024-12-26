using ScottPlot;
using System.Windows;
using System.Windows.Input;
using ScottPlot.DataSources;
using ScottPlot.WPF;
using System.Windows.Media;

namespace ScottPlotDemo;

public partial class PlotDemo : Window
{
    public string DemoTitle => "WPF Multi-Threading";
    public string Description => "Demonstrate how to safely change data while rendering asynchronously.";

    System.Timers.Timer SystemTimer = new() { Interval = 10 };
    private readonly System.Windows.Threading.DispatcherTimer DispatcherTimer = new() { Interval = TimeSpan.FromMilliseconds(10) };

    private readonly List<double> Xs = new List<double>();
    private readonly List<double> Ys = new List<double>();

    public PlotDemo()
    {
        InitializeComponent();
        CreateLines();
    }

    private void CreateLines()
    {
        WpfPlot1.Plot.Clear();
        int newLength = 2000;

        for (int i = 0; i < 200; i++)
        {
            var xs = new List<double>(Generate.Consecutive(newLength));
            var ys = new List<double>(Generate.RandomWalk(newLength));
            ScatterSourceGenericList<double, double> source = new(xs, ys);
            MyScatter scatter = new(source);
            scatter.LineColor = WpfPlot1.Plot.Add.GetNextColor();

            WpfPlot1.Plot.PlottableList.Add(scatter);
        }

    }

    private void ChangeDataLength(int minLength = 10_000, int maxLength = 20_000)
    {
        int newLength = 2000;
        Xs.Clear();
        Ys.Clear();
        Xs.AddRange(Generate.Consecutive(newLength));
        Ys.AddRange(Generate.RandomWalk(newLength));
        WpfPlot1.Plot.Axes.AutoScale(true);
    }

    private void WpfPlot1_OnMouseDown(object? sender, string e)
    {
        this.bdLocation.Visibility = Visibility.Visible;
        this.tbLocation.Text = e;
        this.bdLocation.Margin = new Thickness(Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y, 0, 0);
    }
}