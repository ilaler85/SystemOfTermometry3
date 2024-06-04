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
using Microsoft.UI.Xaml.Documents;
using System.Drawing;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;
using System.Reflection;
using System.Windows.Forms;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private DispatcherTimer timer = new DispatcherTimer();
    IBisnesLogicLayer bll;
    Type pageType = typeof(AllSilosComponent);
    public MainWindow()
    {
        this.InitializeComponent();

        WinUIWorker.WinUIWorker worker = new WinUIWorker.WinUIWorker(this);
        bll = worker;
        


        timer.Interval = TimeSpan.FromMilliseconds(4000);
        timer.Tick += Timer_Tick;
        timer.Stop();

        setStopObservMode();

        FontIcon iconAllSilos = new FontIcon();
        iconAllSilos.Glyph = "\uF49A";
        itemAllSilos.Icon = iconAllSilos;
        setNormalMode();
        //setAdminMode();

    }




    public void setIBLL(IBisnesLogicLayer bll)
    {
        this.bll = bll;
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

    public bool openConnectDB()
    {
        
        return true;
    }

    public void setStopObservMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE769";
        itemObserv.Icon = icon;
        itemObserv.Content = "Опрос остановлен";
    }

    public void setStartObservMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE768";
        itemObserv.Icon = icon;
        itemObserv.Content = "Опрос запущен";
    }

    public bool openApply()
    {
        return true;
    }

    public void changeSetting()
    {
        MethodInfo info = pageType.GetMethod("changeModeSetting");
        info.Invoke(pageType, parameters: null);
    }


    public void refreshLogPannel(RichTextBox richTextBox )
    {
        _ = contentFrame.Navigate(typeof(AllSilosComponent), bll);
    }

    public void setTitleText(string message)
    {
        this.Title = message;
    }

    public void setAdminSetting()
    {
        _ = contentFrame.Navigate(typeof(SettingComponent), bll);
    }

    public void setNormalSetting()
    {
        _ = contentFrame.Navigate(typeof(SettingComponent), bll);
    }

    public void refreshAll()
    {
        _ = contentFrame.Navigate(pageType, bll);
    }


    public void refreshSetting()
    {
        _ = contentFrame.Navigate(pageType, bll);
    }

    public void refreshAllSilosComponent()
    {
        _ = contentFrame.Navigate(pageType, bll);
    }
    public void closeSetting()
    {
        contentFrame.Navigate(typeof(AllSilosComponent), bll);
    }

    public void setAdminMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE785";
        itemAdminMode.Icon = icon;
        setTitleText("Термометрия Nika. Режим Администратора");
    }

    public void setNormalMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE72E";
        itemAdminMode.Icon = icon;
        itemAdminMode.Content = "";
        setTitleText("Термометрия Nika. Режим оператора");
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

    public void setOfflineMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE822";
        itemAdminMode.Icon = icon;
        itemAdminMode.Content = "Выйти из режима обзора";
        setTitleText("Термометрия Nika. Offline режим");
        
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


        pageType = typeof(AllSilosComponent); //init
        var selectedItem = (NavigationViewItem)args.SelectedItem;

        switch (selectedItem.Name)
        {
            case "itemAllSilos":
                pageType = typeof(AllSilosComponent);
                _ = contentFrame.Navigate(pageType, bll);
                return;

            case "itemObserv":
                bll.runStopObserv();
                return;

            case "itemChart":
                pageType = typeof(ChartComponent);
                contentFrame.Navigate(pageType, bll);
                return;

            case "itemSetting":
                bll.openSetting();
                return;

            case "itemExport":
                pageType = typeof(ExportExcelComponent);
                contentFrame.Navigate(pageType, bll);
                return;

            case "itemAbout":
                pageType = typeof(AboutComponent);
                break;

            case "adminMode":
                bll.changeAdminMode();
                return;

            case "refresh":
                bll.refreshConnect();
                return;
                
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
