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
public sealed partial class ApplyKeyForm : Page
{
    private bool passwordCorrect;
    private IBisnesLogicLayer bll;
    string hash = "";

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

    private void ButtonGeneric_Click(object sender, RoutedEventArgs e)
    {
        hash = bll.getSSHKey();
        BoxQuest.Text = hash;
        
    }
    private void ButtonOk_Clic(object sender, RoutedEventArgs e)
    {
        if (bll.checkSSHKey(BoxKey.Text.Trim()))
        {
            bll.successfulActivation();
        }
        else
        {
            ContentDialog dialog = new ContentDialog();
            dialog.PrimaryButtonText = "OK";
            dialog.Content = "Неверный Ключ Активации!";
            dialog.Title = "Неудача";
        }
    }
    private void ButtonExit_Click(object sender, RoutedEventArgs e)
    {
        bll.failedActivation();
    }

    private async  void TextPaste_event(object sender, TextControlPasteEventArgs e)
    {
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        dataPackage.SetText(BoxQuest.Text);
        ToggleThemeTeachingTip2.IsOpen = true;
    }
}
