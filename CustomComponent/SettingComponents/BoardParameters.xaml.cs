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
using SystemOfThermometry3.Services.SettingServices;
using SystemOfThermometry3.WinUIWorker;
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
    private IBisnesLogicLayer bll;
    public BoardParameters()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if(e!=null)
            bll = (IBisnesLogicLayer)e.Parameter;
        SettingBoard board = bll.getParametrsBoard();
        BoxNameCOMPort.Text = board.PortComPort;
        BoxSpeedRead.Text = board.PortBaudRate.ToString();
        BoxParity.Text = board.PortParity.ToString();
        BoxCountDataBit.Text = board.PortDataBits.ToString();
        BoxCountStopBit.Text= board.PortStopBits.ToString();
        BoxTimeBeforeTimeOut.Text = board.PortTimeOut.ToString();

        BoxSurveyPeriod.Text = board.ObserveIterationPeriod.ToString();
        BoxCountAttempts.Text = board.ObserveTriesToObserveBrokenWire.ToString();

        BoxInterrogateBrokenSuspension.Text = board.ObserveBrokenWireTimeOutMilisec.ToString();
        SwitchTimeOutObservWire.IsOn = board.ObserveIsGiveBrokenWireSecondChanse;
        BoxInterrogateBrokenSuspension.IsReadOnly = !SwitchTimeOutObservWire.IsOn;
        
            


    }

    private void SwitchTimeOutObservWire_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {

    }
}
