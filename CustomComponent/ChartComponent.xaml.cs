using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OxyPlot;
using OxyPlot.Series;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OxyPlot.Axes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ChartComponent : Page
{
    public ChartComponent()
    {
        this.InitializeComponent();
    }

    public PlotModel Model
    {
        get; private set;
    } = new PlotModel
    {
        Title = "Hello WinUI 3",
        PlotAreaBorderColor = OxyColors.Transparent,
        Axes =
    {
        new LinearAxis { Position = AxisPosition.Bottom },
        new LinearAxis { Position = AxisPosition.Left },
    },
        Series =
    {
        new LineSeries
        {
            Title = "LineSeries",
            MarkerType = MarkerType.Circle,
            Points =
            {
                new DataPoint(0, 0),
                new DataPoint(10, 18),
                new DataPoint(20, 12),
                new DataPoint(30, 8),
                new DataPoint(40, 15),
            }
        }
    }
    };
}


