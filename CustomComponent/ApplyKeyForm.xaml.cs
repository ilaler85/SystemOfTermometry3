using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using SystemOfThermometry3.WinUIWorker;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CustomComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ApplyKeyForm : Window
{
    private bool passwordCorrect = false;
    private IBisnesLogicLayer bll;
    string hash = "";
    private ApplyKeyForm(IBisnesLogicLayer bll)
    {
        this.InitializeComponent();
        this.bll = bll;
    }


    public static bool EnterKey(IBisnesLogicLayer bll)
    {
        ApplyKeyForm form = new ApplyKeyForm(bll);
        form.Activate();
        return form.passwordCorrect;
                
    }
    private void ButtonGeneric_Click(object sender, RoutedEventArgs e)
    {
        hash = bll.getSSHKey();
        BoxQuest.Text = hash;

    }
    private void ButtonOk_Clic(object sender, RoutedEventArgs e)
    {
        if (bll.checkSSHKey(hash))
        {
            passwordCorrect = true;
            Close();
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
        passwordCorrect = false;
        Close();
    }

    private async  void TextPaste_event(object sender, TextControlPasteEventArgs e)
    {
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        dataPackage.SetText(BoxQuest.Text);
        ToggleThemeTeachingTip2.IsOpen = true;
    }
}
