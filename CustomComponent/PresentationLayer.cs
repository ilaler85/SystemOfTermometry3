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
    private ApplyKeyForm applyKeyForm;
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

        ContentDialog deleteFileDialog = new ContentDialog
        {
            Title = "Вопрос",
            Content = message,
            PrimaryButtonText = "Да",
            CloseButtonText = "Нет"
        };

        ContentDialogResult result = await deleteFileDialog.ShowAsync();

        return result == ContentDialogResult.Primary;
    }

    public async void callMessageBox(string message)
    {
        ContentDialog noWifiDialog = new ContentDialog
        {
            Title = "Сообщение",
            Content = message,
            CloseButtonText = "OK"
        };

        ContentDialogResult result = await noWifiDialog.ShowAsync();

        
    }
    public void refreshALL()
    {
        //window.refreshAll();
    }
    public void refreshAllSilos()
    {
        //window.refreshAllSilosComponent();
    }

    public async Task<string> openFormConnectDBDialog()
    {
        DBConnectForm form = new DBConnectForm(bll);
        form.Activate();
        var resultConnect = await form.ShowAsync();
        return resultConnect;
    }

    public void openFormConnectDBDialog2()
    {
        window.setFrame(typeof(DBConnectForm));
        //frame.Navigate(typeof(DBConnectForm), bll);
    }
    public void showWindowDownload(bool flag)
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
    {
        window.setFrame(typeof(MainPage));
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
        applyKeyForm = new ApplyKeyForm();
        applyKeyForm.Activate();
       
        //window.setFrame(typeof(ApplyKeyForm));
    }

    public string returnKeyApplyKeyForm()
    {
        var hash = applyKeyForm.ShowAsync(bll);
        return hash.GetAwaiter().GetResult();
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

