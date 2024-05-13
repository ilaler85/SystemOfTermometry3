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
using SystemOfThermometry3.CustomComponent;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    DispatcherTimer timer;
    public MainWindow()
    {
        this.InitializeComponent();
        timer.Interval = TimeSpan.FromMilliseconds(4000);
        timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object sender, object e) 
    {
        progressBarOff();
        timer.Stop();
    }

    private void progressBarError()
    {
        timer.Start();
    }

    public void progressBarSetValue(int value)
    {
        switch (value)
        {
            case -1:
                progressBarOn();
                break;
            case -2:
                progressBarError();
                break;
            case 100:
                progressBar.Value = value;
                timer.Start();
                break;
            default:
                progressBar.Value = value;
                break;
        }
    }


    private void progressBarOn()
    {
        progressBar.Value = 0;
        progressBar.Visibility = Visibility.Visible;
    }

    private void progressBarOff()
    {
        progressBar.Visibility = Visibility.Collapsed;
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        FrameNavigationOptions navOptions = new FrameNavigationOptions();
        navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

        if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
        {
            navOptions.IsNavigationStackEnabled = false;
        }

        Type pageType; //init
        var selectedItem = (NavigationViewItem)args.SelectedItem;

        switch (selectedItem.Name)
        {
            case "itemAllSilos":
                pageType = typeof(AllSilosComponent);
                break;

            case "itemObserv":

                return;

            case "itemChart":
                pageType = typeof(ChartComponent);
                break;

            case "itemSetting":
                pageType = typeof(SettingComponent);
                break;

            case "itemExport":
                pageType = typeof(ExportExcelComponent);
                break;
            case "itemAbout":
                pageType = typeof(AboutComponent);
                break;
            case "adminMode":
                changeMode();
                return;
            case "refresh":
                pageType = typeof(AboutComponent);
                //метод перезагрузки
                break;
                
            default:  pageType= typeof(AllSilosComponent); break;
        }
        _ = contentFrame.Navigate(pageType);
        //_ = contentFrame.NavigateToType(pageType, null, navOptions);
    }


    private bool adminEnter()
    {
        var enterForm = new EnterPasswordForm();
        enterForm.Activate();

        return true;
    }


    private void changeMode()
    {
        adminEnter();
    }

    private void aboutSelfButton_Click(object sender, RoutedEventArgs e)
    {
        
    }


}
