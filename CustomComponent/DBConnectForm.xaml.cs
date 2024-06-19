using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NPOI.POIFS.Storage;
using SystemOfThermometry3.WinUIWorker;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DBConnectForm : Window
{
    private IBisnesLogicLayer bll;
    private string error;
    private static AutoResetEvent Locker = new AutoResetEvent(false);

    public DBConnectForm(IBisnesLogicLayer bll)
    {
        this.InitializeComponent();
        this.bll = bll;
    }

    public async Task<string> ShowAsync()
    {
        
        await Task.Run(() => {
            Locker.WaitOne();  //Wait a singal
        });
        return getConnectionString();
    }

    /*protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = (IBisnesLogicLayer)e.Parameter;

    }
    */

    private string getConnectionString()
    {
        return "server=" + BoxServer.Text.Trim()
            + ";port=" + BoxPort.Text.Trim()
            + ";user=" + BoxUser.Text.Trim()
            + ";database=" + BoxNameDB.Text.Trim()
            + ";password=" + BoxPassword.Password.Trim();
    }

    private List<string> checkingForEmptyLines()
    {
        List <string> errorLines = new List<string>();
        if (BoxServer.Text.Trim() == "")
            errorLines.Add("сервера");
        if (BoxPort.Text.Trim() == "")
            errorLines.Add("порта");
        if (BoxUser.Text.Trim() == "")
            errorLines.Add("пользователя");
        if (BoxNameDB.Text.Trim() == "")
            errorLines.Add("имени БД");
        if (BoxPassword.Password.Trim() == "")
            errorLines.Add("пароля");

        return errorLines;
    }

    private async void ButConnect_Click(object sender, RoutedEventArgs e)
    {
        List<string> errorMessage = checkingForEmptyLines();
        if (errorMessage.Count == 0)
        {
            Locker.Set();
            this.Close();
            //await bll.connectDB(connectString);
        }
        else
        {
            error = "Пустое поле ";
            foreach (string line in errorMessage)
            {
                error += line + ", ";
            }
            error = error.Remove(error.Length - 2);
            ToggleThemeTeachingTip1.Subtitle = error;
            ToggleThemeTeachingTip1.IsOpen = true;
        }    
    }

    private void ButAutoConnect_Click(object sender, RoutedEventArgs e)
    {
        bll.successStartOfflineMode();
    }

    private void ButCancel_Click(object sender, RoutedEventArgs e)
    {
        bll.CustomClose();
    }
}
