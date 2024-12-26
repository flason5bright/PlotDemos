using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalyzePlotsFramework
{
    /// <summary>
    /// Interaction logic for LiveChart2.xaml
    /// </summary>
    public partial class LiveChart2 : UserControl
    {
        private bool isAdd = true;

        public LiveChart2()
        {
            InitializeComponent();
        }

        private void LiveChart2_OnLoaded(object sender, RoutedEventArgs e)
        {

            canvas.Width = 1500;

            polyLine.GreateLines();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //curve.OffsetX(scroll.HorizontalOffset);
        }

        private void Curve_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
