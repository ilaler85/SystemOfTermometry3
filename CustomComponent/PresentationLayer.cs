using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;
using Windows.UI.Popups;

namespace SystemOfThermometry3.CustomComponent;
public class PresentationLayerClass : IPresentationLayer
{
    private MainWindow window;
    private IBisnesLogicLayer BLL;
    private bool isObservMode;
    private DBConnect dbConnect;
    private bool result = false;
    public PresentationLayerClass(Window window)
    {
        this.window = window as MainWindow;
    }

    public void setIBLL(IBisnesLogicLayer bll)
    {
        this.BLL = bll;
    }

    #region вызов Бизес логики
    public void changeMode()
    {
        BLL.changeAdminMode();
    }

    public void paintChart()
    {
    
    
    }

    public void runStopObserv()
    {
        BLL.runStopObserv();
    }

    public void saveExcelFile(DateTime time, string filename)
    {
        BLL.exportExcel(time, filename);
    }

    public void openSetting()
    { BLL.openSetting(); }

    #endregion

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
        return true;
    }
    public void runObservMode()
    {
        window.setStartObservMode();
    }
    public void stopObservMode()
    {
        window.setStopObservMode();
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

    public void openFormConnectDBDialog()
    {
        dbConnect = new DBConnect();
        dbConnect.Activate();

    }
    public void showWindowDownload(bool flag) => throw new NotImplementedException();


    public void setProgressBar(int value)
    {
        window.progressBarSetValue(value);
    }
    public bool closeSetting()
    {
       return true;
    }

    public void openConnectDBDialog(ref Dao dao) => throw new NotImplementedException();
    public void overheatMessageBox() => throw new NotImplementedException();
    public void openFormConnectDBDialog(ref Dao dao) => throw new NotImplementedException();
    public void closeFormConnectDB()
    {
        dbConnect.Close();
    }
    public void closeWindowDownload() => throw new NotImplementedException();
    public void callSettingComponent(SettingsService settingsService, bool adminSetting) => throw new NotImplementedException();
    public void closeAdminSetting() => throw new NotImplementedException();
    public void openEnterForm(modeCheckPassword checkPassword) => throw new NotImplementedException();
    public bool stopObserv()
    {
        DialogShow("Требуется подтверждение", "Остановить опрос плат?");
        return result;
    }
    public void setStopStyleForm() => throw new NotImplementedException();
    public void setNormalStyleForm() => throw new NotImplementedException();
    public void sendLogMessage(string message, Color color) => throw new NotImplementedException();
    public void setStatus(string message)
    {
        window.Title = "Термометрия NIKA " + message;
    }
    public void overheatMessageBox(StringBuilder message, bool playSound) => throw new NotImplementedException();
    bool IPresentationLayer.openEnterForm(modeCheckPassword checkPassword) => throw new NotImplementedException();
    public bool openFormApplyKeyForm()
    {
        return true;
    }

    public void openProviderSetting() => throw new NotImplementedException();
    public void refreshSetting() => throw new NotImplementedException();
    public void refreshSilosTabControl() => throw new NotImplementedException();
    public void refreshAllSilosTemperatur() => throw new NotImplementedException();

    private async void DialogShow(string title, string content)
    {
        ContentDialog deleteFileDialog = new ContentDialog()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "ОК",
            SecondaryButtonText = "Отмена"
        };
        ContentDialogResult dialogResult = await deleteFileDialog.ShowAsync();
        result = dialogResult == ContentDialogResult.Primary;

    }
}

