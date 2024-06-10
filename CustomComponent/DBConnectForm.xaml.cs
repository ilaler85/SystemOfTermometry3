using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
public sealed partial class DBConnectForm : Page
{
    private IBisnesLogicLayer bll;
    private string error;
    public DBConnectForm()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = (IBisnesLogicLayer)e.Parameter;

    }

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
            errorLines.Add("�������");
        if (BoxPort.Text.Trim() == "")
            errorLines.Add("�����");
        if (BoxUser.Text.Trim() == "")
            errorLines.Add("������������");
        if (BoxNameDB.Text.Trim() == "")
            errorLines.Add("����� ��");
        if (BoxPassword.Password.Trim() == "")
            errorLines.Add("������");

        return errorLines;
    }

    private void ButConnect_Click(object sender, RoutedEventArgs e)
    {
        List<string> errorMessage = checkingForEmptyLines();
        if (errorMessage.Count == 0)
            bll.connectDB(getConnectionString());
        else
        {
            error = "������ ���� ";
            foreach (string line in errorMessage)
            {
                error += line + ", ";   
            }
            error = error.Remove(error.Length-2);
            ToggleThemeTeachingTip1.Subtitle = error;
            ToggleThemeTeachingTip1.IsOpen = true;
        }    
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
