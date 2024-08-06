using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.CustomComponent;
using SystemOfThermometry3.WinUIWorker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private DispatcherTimer timer = new DispatcherTimer();
    IBisnesLogicLayer bll;
    Type pageType = typeof(AllSilosComponent);

    public MainPage()
    {
        this.InitializeComponent();
        //WinUIWorker.WinUIWorker worker = new WinUIWorker.WinUIWorker(this);
        //bll = worker;

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
        Frame frame = Window.Current.Content as Frame;
        frame.Navigate(typeof(DBConnectForm), bll);

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
        _ = contentFrame.Navigate(typeof(SettingComponent), bll);
        //MethodInfo info = contentFrame.Content.GetType().GetMethod("changeModeSetting");
        //info?.Invoke(contentFrame.Content, parameters: null); 
    }


    public void refreshLogPannel(RichTextBox richTextBox)
    {
        _ = contentFrame.Navigate(typeof(AllSilosComponent), bll);
    }

    public void setTitleText(string message)
    {
        //this.Title = message;
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
        _ = contentFrame.Navigate(typeof(AllSilosComponent), bll);
    }

    public void setAdminMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE785";
        //iconAdminMode.Symbol = Symbol.Admin;
        itemAdminMode.Icon = icon;
        itemAdminMode.Content = "Выйти из режима администратора";
        setTitleText("Термометрия Nika. Режим Администратора");
    }

    public void setNormalMode()
    {
        FontIcon icon = new FontIcon();
        icon.Glyph = "\uE72E";
        //iconAdminMode.Symbol = Symbol.Emoji;
        itemAdminMode.Icon = icon;
        itemAdminMode.Content = "Включить режим администратора";
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
        //iconAdminMode.Symbol = Symbol.Important;
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
        try
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

                case "itemAdminMode":
                    bll.changeAdminMode();
                    return;

                case "refresh":
                    bll.refreshConnect();
                    return;

                default: pageType = typeof(AllSilosComponent); break;
            }
            _ = contentFrame.Navigate(pageType);
            //_ = contentFrame.NavigateToType(pageType, null, navOptions);
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = (IBisnesLogicLayer)e.Parameter;
    }
}
