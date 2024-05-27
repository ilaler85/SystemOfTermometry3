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
using SystemOfThermometry3.WinUIWorker;
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
    IBisnesLogicLayer bll;
   
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
        if(e != null)
            bll = (IBisnesLogicLayer)e.Parameter;

        string conStr = bll.getConnectionString();
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

            bll.connectDB(getConnectionString());
        }
        else
        {
            ContentDialog messageDialog = new ContentDialog()
            {
                Title = "Внимание",
                Content = "Ошибка ввода порта",
                PrimaryButtonText = "ОК"
            };
            await messageDialog.ShowAsync();
        }
        
    }

    private void ButtonCalculateFilling_Click(object sender, RoutedEventArgs e)
    {
        DateTime date = DateWith.Date.Value.Date;
        bll.calculateFillingSilosesTable(date);
    }

    private void ButtonDropTemperature_Click(object sender, RoutedEventArgs e)
    {
        DateTime date = DateWith.Date.Value.Date;
        bll.dropTemperature(getConnectionString(), date);
    }

    private string getConnectionString()
    {
        return "server=" + BoxServer.Text
            + ";port=" + BoxPort.Text
            + ";user=" + BoxUser.Text
            + ";database=" + BoxNameDB.Text
            + ";password=" + BoxPassword.Text;
    }

    private void ButtonDropDB_Click(object sender, RoutedEventArgs e)
    {
        bll.dropDB(getConnectionString());
    }
}
