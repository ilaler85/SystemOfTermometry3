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
using SystemOfThermometry3.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.Setting_components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BoardParameters : Page
{
    private SettingsService settingsService;
    public BoardParameters()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        settingsService = (SettingsService)e.Parameter;
        BoxNameCOMPort.Text = settingsService.PortComPort;
        BoxSpeedRead.Text = settingsService.PortBaudRate.ToString();
        BoxParity.Text = settingsService.PortParity.ToString();
        BoxCountDataBit.Text = settingsService.PortDataBits.ToString();
        BoxCountStopBit.Text= settingsService.PortStopBits.ToString();
        BoxTimeBeforeTimeOut.Text = settingsService.PortTimeOut.ToString();

        BoxSurveyPeriod.Text = settingsService.ObserveIterationPeriod.ToString();
        BoxCountAttempts.Text = settingsService.ObserveTriesToObserveBrokenWire.ToString();

        BoxInterrogateBrokenSuspension.Text = settingsService.ObserveBrokenWireTimeOutMilisec.ToString();
        SwitchTimeOutObservWire.IsOn = settingsService.ObserveIsGiveBrokenWireSecondChanse;
        BoxInterrogateBrokenSuspension.IsReadOnly = !SwitchTimeOutObservWire.IsOn;
        
            


    }

    private void SwitchTimeOutObservWire_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {

    }
}
