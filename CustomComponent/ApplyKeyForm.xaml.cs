using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.WinUIWorker;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ApplyKeyForm : Window
{
    private bool passwordCorrect;
    private IBisnesLogicLayer bll;
    string hash = "";
    private static AutoResetEvent Locker = new AutoResetEvent(false);


    public ApplyKeyForm()
    {
        this.InitializeComponent();
        bll = null;
    }


    /* protected override void OnNavigatedTo(NavigationEventArgs e)
     {
         if (e != null)
             bll = (IBisnesLogicLayer)e.Parameter;

     }*/

    public async Task<string> ShowAsync(IBisnesLogicLayer bll)
    {
        this.bll = bll;

        await Task.Run(() => {
            Locker.WaitOne();  //Wait a singal
        });
        
        return hash;
    }

    private void ButtonGeneric_Click(object sender, RoutedEventArgs e)
    {
        hash = bll.getSSHKey();
        BoxQuest.Text = hash;
        
    }
    private void ButtonOk_Clic(object sender, RoutedEventArgs e)
    {
        hash = BoxKey.Text.Trim();
        Locker.Set();

       /* if (bll.checkSSHKey(BoxKey.Text.Trim()))
        {
            
            // ����������
            bll.successfulActivation();
        }
        else
        {
            ContentDialog dialog = new ContentDialog();
            dialog.PrimaryButtonText = "OK";
            dialog.Content = "�������� ���� ���������!";
            dialog.Title = "�������";
        }*/
    }
    private void ButtonExit_Click(object sender, RoutedEventArgs e)
    {
        hash = "exit";
        Locker.Set();
        //bll.failedActivation();
    }

    private async  void TextPaste_event(object sender, TextControlPasteEventArgs e)
    {
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        dataPackage.SetText(BoxQuest.Text);
        ToggleThemeTeachingTip2.IsOpen = true;
    }
}
