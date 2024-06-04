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
public sealed partial class DBConnectForm : Page
{
    private IBisnesLogicLayer bll;
    public DBConnectForm(IBisnesLogicLayer bll)
    {
        this.InitializeComponent();
        this.bll = bll;
    }

    private string getConnectionString()
    {
        return "server=" + BoxServer.Text.Trim()
            + ";port=" + BoxPort.Text.Trim()
            + ";user=" + BoxUser.Text.Trim()
            + ";database=" + BoxNameDB.Text.Trim()
            + ";password=" + BoxPassword.Password.Trim();
    }

    private void ButConnect_Click(object sender, RoutedEventArgs e)
    {
        bll.connectDB(getConnectionString());
    }

    private void ButAutoConnect_Click(object sender, RoutedEventArgs e)
    {
        bll.successStartWithDB();
    }

    private void ButCancel_Click(object sender, RoutedEventArgs e)
    {
        bll.CustomClose();
    }
}
