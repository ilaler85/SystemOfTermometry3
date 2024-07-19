using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfThermometry3.Services.SettingServices;
public class SettingBoard
{
    public uint ObserveIterationPeriod;
    public uint ObserveTriesToObserveBrokenWire;
    public bool ObserveIsGiveBrokenWireSecondChanse;
    public uint ObserveBrokenWireTimeOutMilisec;
    public bool IsStartObservAutomatically;
    public string PortComPort;
    public int PortBaudRate;
    public Parity PortParity;
    public StopBits PortStopBits;
    public int PortTimeOut;
    public int PortDataBits;


    public SettingBoard(SettingsService settingService) 
    { 
        ObserveIterationPeriod = settingService.ObserveIterationPeriod;
        ObserveTriesToObserveBrokenWire = settingService.ObserveTriesToObserveBrokenWire;
        ObserveIsGiveBrokenWireSecondChanse = settingService.ObserveIsGiveBrokenWireSecondChanse;
        ObserveBrokenWireTimeOutMilisec = settingService.ObserveBrokenWireTimeOutMilisec;
        IsStartObservAutomatically = settingService.IsStartObservAutomatically;
        PortComPort = settingService.PortComPort;
        PortBaudRate = settingService.PortBaudRate;
        PortParity = settingService.PortParity;
        PortStopBits = settingService.PortStopBits;
        PortTimeOut = settingService.PortTimeOut;
        PortDataBits = settingService.PortDataBits;
    }

}
