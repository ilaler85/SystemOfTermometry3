using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OxyPlot;
using SystemOfThermometry3.WinUIWorker;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ApplyKeyForm : Page
{
    private bool passwordCorrect;
    private IBisnesLogicLayer bll;
    static bool result = false;
    string hash = "";
    private static AutoResetEvent Locker = new AutoResetEvent(false);


    public ApplyKeyForm()
    {
        this.InitializeComponent();
        
        bll = null;
    } 


     protected override void OnNavigatedTo(NavigationEventArgs e)
     {
         if (e != null)
             bll = (IBisnesLogicLayer)e.Parameter;

     }

    public async Task<bool> ShowAsync()
    {
        await Task.Run(() => {
            Locker.WaitOne();  //Wait a singal
        });
        
        return result;
    }

    private void ButtonGeneric_Click(object sender, RoutedEventArgs e)
    {
        hash = bll.getSSHKey();
        BoxQuest.Text = hash;
        
    }
    private void ButtonOk_Clic(object sender, RoutedEventArgs e)
    {
        hash = BoxKey.Text.Trim();
        //bll.checkSSHKey(hash);
       // Locker.Set();

        if (bll.checkSSHKey(BoxKey.Text.Trim()))
        {
            result = true;
            //Locker.Set();
            bll.successfulActivation();
        }
        else
        {
            BoxKey.Text = "";
            ToggleThemeTeachingTip1.IsOpen = true;
            /*ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Неудача",
                Content = "Неверный Ключ Активации!",
                CloseButtonText = "ОК"
            };*/
            //var result  = await contentDialog.ShowAsync();
        }
    }
    private void ButtonExit_Click(object sender, RoutedEventArgs e)
    {
        result =false;
        bll.failedActivation();
        //bll.failedActivation();
    }

    private void TextPaste_event(object sender, TextControlPasteEventArgs e)
    {
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        dataPackage.SetText(BoxQuest.Text);
        ToggleThemeTeachingTip2.IsOpen = true;
    }
}
