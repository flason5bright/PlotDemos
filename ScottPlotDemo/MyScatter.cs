using ScottPlot;
using ScottPlot.Plottables;
using SkiaSharp;

namespace ScottPlotDemo;

public partial class MyScatter : Scatter
{
    public MyScatter(IScatterSource data) : base(data)
    {
        MarkerSize = 15;
        //ConnectStyle = ConnectStyle.StepVertical;
    }

    public DataPoint SelectPoint { get; set; }

    public override void Render(RenderPack rp)
    {
        base.Render(rp);
        using SKPaint paint = new();

        double x = SelectPoint.X * ScaleX + OffsetX;
        double y = SelectPoint.Y * ScaleY + OffsetY;
        var selectPixel = Axes.GetPixel(new(x, y));

        Drawing.DrawCircle(rp.Canvas, selectPixel, 3, new FillStyle() { Color = Color.FromColor(System.Drawing.Color.Red) }, paint);
    }
}
