using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SystemOfThermometry3.WinUIWorker;
using Windows.Foundation;
using SystemOfThermometry3.CustomComponent;
using Mysqlx.Notice;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private IBisnesLogicLayer bll;
    private WinUIWorker.WinUIWorker worker;
    public MainWindow(IBisnesLogicLayer bll)
    {
        //frame = new Frame();
        this.InitializeComponent();
        
        //Windows.UI.ViewManagement.ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
        //appView.SetPreferredMinSize(new Size(600, 500));
        this.bll = bll;


    }

    public MainWindow()
    {
        this.InitializeComponent();
        
        worker = new WinUIWorker.WinUIWorker(this);

        bll = worker;
        frame.Navigate(typeof(MainPage), bll);
        //setFrame(typeof(MainPage));
        //_ = frame.Navigate(typeof(MainPage), bll);
    }

    public void startBLL()
    {
        //setFrame(typeof(MainPage));
        worker.loadProgram();
    }

    public void setFrame(Type page)
    {
        frame.Navigate(page, bll);
    }

    public void setBLL(IBisnesLogicLayer bll)
    {
        this.bll = bll;
    }

    public void getMethod(string nameMethod, object[] objects)
    {
        MethodInfo method = frame.GetType().GetMethod(nameMethod);
        method.Invoke(method, objects);
    }

    public void getMethod(string nameMethod)
    {
        MethodInfo method = frame.GetType().GetMethod(nameMethod);
        method?.Invoke(method, parameters: null);
    }

}
