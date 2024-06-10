using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomComponent;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;
using Windows.UI.Popups;

namespace SystemOfThermometry3.CustomComponent;
public class PresentationLayerClass : IPresentationLayer
{

    //RichTextBox rich = new RichTextBox();
    private MainWindow window;
    private bool isObservMode;
    private bool isAdminMode = false;
    private DBConnectForm dbConnect;
    private bool result = false;
    private IBisnesLogicLayer bll;
    private LoadingForm loading;
    private Thread thread;

    public PresentationLayerClass()
    {

    }


    public PresentationLayerClass(MainWindow mainWindow, IBisnesLogicLayer bll)
    {   
        this.bll = bll;
        this.window = mainWindow;
    }

    public PresentationLayerClass(IBisnesLogicLayer bll)
    {
        this.bll = bll;
    }

    public void setIBLL(IBisnesLogicLayer bll)
    {
        this.bll = bll;
    }


    public void startMainForm()
    {
        window = new MainWindow(bll);
    }

    public bool setAdminMode()
    {
        window.getMethod("setAdminMode");
        return true;

    }

    public bool setOfflineMode()
    {
        window.getMethod("setOfflineMode");
        return true;

    }
    public bool setNormalMode()
    {
        window.getMethod("setNormalMode");
        return true;
    }
    public bool setConnectDB_Mode()
    {
        window.getMethod("setTitleText", new object[] { "Термометрия Nika. Подключено к БД" });
        return true;
    }
    public void runObservMode()
    {
        window.getMethod("setStartObservMode");
    }
    public void stopObservMode()
    {
        window.getMethod("setStopObservMode");
    }

    public async Task<bool> askDialogShow(string message)
    {
        ContentDialog dialog = new ContentDialog();

        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "";
        dialog.PrimaryButtonText = "Да";
        dialog.SecondaryButtonText = "Нет";
        dialog.Content = new ContentDialogPage(message);

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async void callMessageBox(string message)
    {
        ContentDialog dialog = new ContentDialog();

        
        dialog.PrimaryButtonText = "Ок"; 
        dialog.Title = "сообщение";

        dialog.Content = new MessageDialog(message);
        await dialog.ShowAsync();
    }
    public void refreshALL()
    {
        //window.refreshAll();
    }
    public void refreshAllSilos()
    {
        //window.refreshAllSilosComponent();
    }

    public void openFormConnectDBDialog()
    {
        window.setFrame(typeof(DBConnectForm));
        //frame.Navigate(typeof(DBConnectForm), bll);
    }
    public async void showWindowDownload(bool flag)
    {
        window.setFrame(typeof(LoadingForm));
        //frame.Navigate(typeof(LoadingForm));
    }


    public void setProgressBar(int value)
    {
        window.getMethod("progressBarSetValue", new object[] { value });
    }



    private async Task<bool> askCloseSetting()
    {
        ContentDialog dialog = new ContentDialog();
        dialog.Title = "Чтобы начать опрос необходимо закрыть настройки";
        dialog.Content = "Закрыть настройки?";
        dialog.PrimaryButtonText = "Закрыть";
        dialog.SecondaryButtonText = "Отмена";

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            window.getMethod("closeSetting");
            return true;
        }
        else
            return false;
    }

    public bool closeSetting()
    {
        return askCloseSetting().Result;
    }

    public void closeFormConnectDB()
    {
        
    }
    public void closeWindowDownload()
    {window.setFrame(typeof(MainWindow));
       // frame.Navigate(typeof(MainWindow));
    }
    public void callSettingComponent(SettingsService settingsService, bool adminSetting)
    {
        bll.openSetting();
    }
    public void closeAdminSetting()
    {
        isAdminMode = false;
        window.getMethod("setNormalSetting");
    }
    public bool stopObserv()
    {
        DialogShow("Требуется подтверждение", "Остановить опрос плат?");
        return result;
    }
    public void setStopStyleForm()
    {
        window.getMethod("setStopObservMode");
    }
    public void setNormalStyleForm()
    {
        window.getMethod("setStartObservMode");
    }


    private void refreshLogPannel()
    {
        //window.refreshLogPannel(rich);
    }

    public void sendLogMessage(string message, Color color)
    {
        /*
        rich.SelectionStart = 0;
        rich.SelectionLength = 0;
        rich.SelectionColor = color;
        rich.SelectedText = message;
        refreshLogPannel();*/

    }
    public void setStatus(string message)
    {
        //window.Title = "Термометрия NIKA " + message;
    }
    public void overheatMessageBox(StringBuilder message, bool playSound)
    {


    }
    public bool openEnterForm(modeCheckPassword checkPassword)
    {

        return true;
    }
    public void openFormApplyKeyForm()
    {
        window.setFrame(typeof(ApplyKeyForm));
    }

    public void refreshSetting()
    {
        window.getMethod("refreshSetting");
    }
    public void refreshSilosTabControl()
    {

    }
    public void refreshAllSilosTemperatur()
    {

    }

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

    public void openNormalSetting()
    {
        window.getMethod("changeSetting");
    }
    public void openAdminSetting()
    {
        window.getMethod("changeSetting");
    }
}

