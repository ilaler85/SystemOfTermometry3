using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.WinUIWorker;


namespace SystemOfThermometry3.CustomComponent.SettingComponents;


public sealed partial class SettingProvider : Page
{

    IBisnesLogicLayer bll;

    public string newPassword;
    public SettingProvider()
    {
        this.InitializeComponent();
        
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = e.Parameter as IBisnesLogicLayer;

        pathTextBox.Text = "Рабочая папка: " + bll.getPathSaveFile();
        chekBoxAveraginTemperatureValues.IsChecked = bll.getModeAveraginTemperatureValues();

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        bll.changeProviderPassword(BoxOldPassword.Text, BoxNewPassword.Text, BoxRepeatNewPassword.Text);

    }

    private void chekBoxAveraginTemperatureValues_Checked(object sender, RoutedEventArgs e)
    {
        bll.averaginTemperatureValues();
    }
}
