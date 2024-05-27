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
using SystemOfThermometry3.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.GrainComponent;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GrainComponent : Page
{
    Grain grain {get; set; }
    public delegate void DelegateEvent(Grain grain);
    public event DelegateEvent save;
    public delegate void DelegateEventDelete(Grain grain, object sender);
    public event DelegateEventDelete delete;
    public GrainComponent(Grain grain)
    {
        this.InitializeComponent();
        this.grain = grain;
        BoxNameGrain.Text = grain.Name;
        BoxRedTemp.Text = grain.RedTemp.ToString();
        BoxYellowTemp.Text = grain.YellowTemp.ToString();
    }


    private async void message(string mes)
    {
        ContentDialog dialog = new ContentDialog();
        dialog.Content = mes;
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Ошибка";
        dialog.PrimaryButtonText = "Save";


        await dialog.ShowAsync();
    }

    private bool checkTemp()
    {
        int a;
        if ((!int.TryParse(BoxYellowTemp.Text, out a))||(!int.TryParse(BoxRedTemp.Text, out a)))
        {
            message("Введите число!");
            return false;
        }

        if (Convert.ToInt32(BoxYellowTemp.Text) > Convert.ToInt32(BoxRedTemp.Text))
        {
            message("Желтый порог больше красного порога");
            return false;
        }

        return true;
            
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
        delete?.Invoke(grain, this);
        
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        if(!checkTemp()) return; 

        grain.Name = BoxNameGrain.Text;
        grain.RedTemp = Convert.ToInt32(BoxRedTemp.Text.Trim());
        grain.YellowTemp = Convert.ToInt32(BoxYellowTemp.Text.Trim());
        
        save?.Invoke(grain);
    }
}
