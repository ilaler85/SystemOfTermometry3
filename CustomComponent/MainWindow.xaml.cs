using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SystemOfThermometry3.WinUIWorker;
using Windows.Foundation;
using SystemOfThermometry3.CustomComponent;
using Mysqlx.Notice;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Microsoft.UI;
using Windows.Storage.Pickers;
using Windows.Storage;

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
        this.InitializeComponent();
        this.bll = bll;

    }

    public MainWindow()
    {
        this.InitializeComponent();

        worker = new WinUIWorker.WinUIWorker(this);
        bll = worker;
    }

    public void startBLL()
    {
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
        MethodInfo method = frame.Content.GetType().GetMethod(nameMethod);
        method.Invoke(frame.Content, objects);
    }

    public T getMethod<T>(string nameMethod) where T : struct
    {
        MethodInfo method = frame.Content.GetType().GetMethod(nameMethod);
        return (T)method?.Invoke(frame.Content, parameters: null);
    }

    public void getMethod(string nameMethod)
    {

        MethodInfo method = frame.Content.GetType().GetMethod(nameMethod);
        method?.Invoke(frame.Content, parameters: null);
    }

    public object getReturnMethod(string nameMethod)
    {
        MethodInfo method = frame.Content.GetType().GetMethod(nameMethod);
        return method?.Invoke(frame.Content, parameters: null);
    }

    private ContentDialog dialog;

    public async Task callMessageBox(string message)
    {
        dialog = new ContentDialog
        {
            Title = "Сообщение",
            Content = message,
            CloseButtonText = "OK"
        };

        await dialog.ShowAsync();


    }

    public void setStyle(string title)
    {
        ApplicationView.GetForCurrentView().Title = title;
        var titleBar = ApplicationView.GetForCurrentView().TitleBar;
        titleBar.ForegroundColor = Colors.White;
        titleBar.BackgroundColor = Colors.Green;
        titleBar.ButtonForegroundColor = Colors.White;
        titleBar.ButtonBackgroundColor = Colors.SeaGreen;
        titleBar.ButtonHoverForegroundColor = Colors.White;
        titleBar.ButtonHoverBackgroundColor = Colors.DarkSeaGreen;
        titleBar.ButtonPressedForegroundColor = Colors.Gray;
        titleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

        // Set inactive window colors
        titleBar.InactiveForegroundColor = Colors.Gainsboro;
        titleBar.InactiveBackgroundColor = Colors.SeaGreen;
        titleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
        titleBar.ButtonInactiveBackgroundColor = Colors.SeaGreen;
    }

    public async Task<string> openFilaDialog()
    {
        FileOpenPicker openPicker = new FileOpenPicker();
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        StorageFile file = await openPicker.PickSingleFileAsync();
        if (file != null)
            return file.Path;
        return "";
    }

}
