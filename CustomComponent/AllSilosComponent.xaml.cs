using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using SystemOfTermometry2.CustomComponent;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfTermometry2.CustomComponent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AllSilosComponent: Page
{
    public AllSilosComponent()
    {
        this.InitializeComponent();
    }

    public void navigationTop_selectChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        FrameNavigationOptions navOptions = new FrameNavigationOptions();
        navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

        Type pageType;

        var selectedItem = (NavigationViewItem)args.SelectedItem;

        switch (selectedItem.Name)
        {
            case "allSilosItem":
                pageType = typeof(AllSilosView);
                break;

            case "oneSilosItem":
                pageType = typeof(OneSilosComponent);
                break;

            case "chartFillingItem":
                pageType = typeof(ChartFillingComponent);
                break;

            default: pageType = typeof(AllSilosView); break;
        }

        _ = contentPanel.Navigate(pageType, null, new DrillInNavigationTransitionInfo());
    }

}