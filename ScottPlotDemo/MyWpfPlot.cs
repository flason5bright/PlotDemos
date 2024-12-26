using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ScottPlot.WPF;
using SkiaSharp.Views.WPF;

namespace ScottPlotDemo;


public class MyWpfPlot : WpfPlot
{
    public SkiaSharp.Views.WPF.SKElement Element => base.PlotFrameworkElement as SkiaSharp.Views.WPF.SKElement;
    public event EventHandler<string> OnPointSelected;

    Popup myPopup = new Popup();
    Border myBorder = new Border();

    public MyWpfPlot()
    {

    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.Element.MouseLeftButtonDown += Element_MouseLeftButtonDown;


    }

    private void Element_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var interAction = (ScottPlot.Control.Interaction)Interaction;
        var mouseLocation = interAction.GetMouseCoordinates(Plot.Axes.Bottom, Plot.Axes.Left);
        myPopup.IsOpen = false;
        foreach (var scatter in Plot.PlottableList)
        {
            var myScatter = scatter as MyScatter;
            if (myScatter != null)
            {
                myScatter.MarkerSize = 20;

                var nearest = myScatter.GetNearest(mouseLocation, Plot.LastRender,5);
                if (nearest.Index != -1)
                {
                    myScatter.SelectPoint = nearest;

                    OnPointSelected?.Invoke(this, $"X:{nearest.X} Y:{nearest.Y}");

                    break;
                }

            }
        }
    }
}

