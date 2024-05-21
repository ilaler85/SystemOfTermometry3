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
using MySqlX.XDevAPI.Common;
using SystemOfThermometry3.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.Setting_components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DBConnect : Page
{
    SettingsService settingsService;
    public delegate void ConnectDelegate(string server, string port, string user, string name, string password);
    public delegate void EmptyDelegate();
    public delegate void DropTempDelegate(DateTime date);
    public event ConnectDelegate connectDB;
    public event EmptyDelegate calculateFilling;
    public event DropTempDelegate dropTemperature;
    public event EmptyDelegate dropDB;
    public DBConnect()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        settingsService = (SettingsService)e.Parameter;

        string conStr = FileProcessingService.getConnectionString();
        if (conStr != null && conStr != "")
        {
            string[] splitedStr = conStr.Split(';', '=');
            if (splitedStr.Length >= 7)
            {
                BoxServer.Text = splitedStr[1];
                BoxPort.Text = splitedStr[3];
                BoxUser.Text = splitedStr[5];
                BoxNameDB.Text = splitedStr[7];
            }
        }
    }
    private bool checkTextBox()
    {
        int n;
        return Int32.TryParse(BoxPort.Text, out n);
    }

    private async void ButtonConnectDB_Click(object sender, RoutedEventArgs e)
    {
        if (checkTextBox())
        {
            connectDB?.Invoke(BoxServer.Text, BoxPort.Text, BoxUser.Text, BoxNameDB.Text, BoxPassword.Text);
        }
        else
        {
            ContentDialog deleteFileDialog = new ContentDialog()
            {
                Title = "Внимание",
                Content = "Ошибка ввода порта",
                PrimaryButtonText = "ОК"
            };
            await deleteFileDialog.ShowAsync();
        }
        
    }

    private void ButtonCalculateFilling_Click(object sender, RoutedEventArgs e)
    {
        calculateFilling?.Invoke();
    }

    private void ButtonDropTemperature_Click(object sender, RoutedEventArgs e)
    {
        DateTime date = DateWith.Date.Value.Date;
        dropTemperature?.Invoke(date);
    }

    private void ButtonDropDB_Click(object sender, RoutedEventArgs e)
    {
        dropDB?.Invoke();
    }
}
