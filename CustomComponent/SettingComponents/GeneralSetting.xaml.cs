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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.SettingComponents;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GeneralSetting : Page
{
    SettingsService settingsService;
    public GeneralSetting()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        settingsService = (SettingsService)e.Parameter;
        BoxHeigh.Text = settingsService.PlotHeight.ToString();
        BoxWidth.Text = settingsService.PlotWidth.ToString();
        BoxNameCompany.Text = settingsService.CompanyName;
        if (settingsService.ExcelExportType == SettingsService.EXCEL_EXPORT_STANDARD_TEMPLATE)
            RadioVertical.IsChecked = true;
        else
            RadioHorizont.IsChecked = true;

        if (settingsService.ExportIsSetColorOnCellWithTemp == false)
            CheckBoxColorExport.IsChecked = false;

        if(settingsService.IsHighlightWhenObservStop == false)
            CheckBoxColorStopObserv.IsChecked = false;

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
