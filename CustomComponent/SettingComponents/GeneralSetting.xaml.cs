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
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.SettingComponents;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GeneralSetting : Page
{
    IBisnesLogicLayer bll;
    public GeneralSetting()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = (IBisnesLogicLayer)e.Parameter;

        List<int> sizeChart = bll.getSizeChart();
        BoxHeigh.Text = sizeChart[1].ToString();
        BoxWidth.Text = sizeChart[0].ToString();
        BoxNameCompany.Text = bll.getNameCompany();
        if (bll.getOrientationExcelFile())
            RadioVertical.IsChecked = true;
        else
            RadioHorizont.IsChecked = true;

        CheckBoxColorExport.IsChecked = bll.getColorCellsExcelFile();
        CheckBoxColorStopObserv.IsChecked = bll.getIsHighlightWhenObservStop();

        SwitchTemp.IsOn = settingsService.OverheatPlaySound;
        BoxCountSensor.Text = settingsService.OverheatMinimumSensorToTrigger.ToString();
        CheckBoxSendMail.IsChecked = settingsService.OverheatMailSend;
        CheckBoxSendChart.IsChecked = settingsService.OverheatMailAddPlot;
        CheckBoxSendExcel.IsChecked = settingsService.OverheatMailAddExcel;
        BoxTimeOut.Text = settingsService.OverheatTriggerMinimumPeriod.ToString();



    }

    private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {

    }

    private void RadioButton_Unchecked(object sebder, RoutedEventArgs e)
    {

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
