using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using OxyPlot;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;
using SystemOfThermometry3.Services;
using Windows.UI.Popups;
using System.Drawing;
using System.Threading;
using SystemOfThermometry3.DAO;

namespace SystemOfThermometry3.CustomComponent;
public class PresentationLayerClass: IPresentationLayer
{
    private MainWindow window;
    private IBisnesLogicLayer BLL;
    public PresentationLayerClass(Window window)
    {
        this.window = window as MainWindow;
    }

    public void setIBLL(IBisnesLogicLayer bll)
    { this.BLL = bll; }


    public void callSettingComponent(SettingsService settingsService)
    {
    
    }

    public bool setAdminMode()
    {
        return true;
       
    }

    public bool setOfflineMode()
    {
    
    return true; 
    
    }
    public bool setNormalMode()
    {
    
    return true;
    }
    public bool setConnectDB_Mode()
    {
    return true; }
    public void runObservMode()
    {
    
    }
    public void stopObservMode()
    {
    
    }
    public async void callMessageBox(string message)
    {
        var messageBox = new MessageDialog(message);
        await messageBox.ShowAsync();
    }
    public void refreshALL()
    {
    
    }
    public void refreshAllSilos()
    {
    
    }

    public void openConnectDBDialog() => throw new NotImplementedException();
    public void showWindowDownload(bool flag) => throw new NotImplementedException();


    public void setProgressBar(int value)
    {
        window.progressBarSetValue(value);
    }
    bool IPresentationLayer.closeSetting() => throw new NotImplementedException();
    public void openConnectDBDialog(ref Dao dao) => throw new NotImplementedException();
    public void overheatMessageBox() => throw new NotImplementedException();
    public void closeConnectDB() => throw new NotImplementedException();
    public void openFormConnectDBDialog(ref Dao dao) => throw new NotImplementedException();
    public void closeFormConnectDB() => throw new NotImplementedException();
    public void closeWindowDownload() => throw new NotImplementedException();
    public void callSettingComponent(SettingsService settingsService, bool adminSetting) => throw new NotImplementedException();
    public void closeAdminSetting() => throw new NotImplementedException();
    public void openEnterForm(modeCheckPassword checkPassword) => throw new NotImplementedException();
    public bool stopObserv() => throw new NotImplementedException();
    public void setStopStyleForm() => throw new NotImplementedException();
    public void setNormalStyleForm() => throw new NotImplementedException();
    public void sendLogMessage(string message, Color color) => throw new NotImplementedException();
    public void setStatus(string message) => throw new NotImplementedException();
    public void overheatMessageBox(StringBuilder message, bool playSound) => throw new NotImplementedException();
}

