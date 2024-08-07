﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    private ContentDialog dialog;
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
        try
        {
            return await window.callAskDialog(message);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }

    }

    public async Task callMessageBox(string message)
    {
        try
        {
            await window.callMessageBox(message);
        }
        catch 
        {
            //Debug.WriteLine(ex);
        }
        /*dialog = new ContentDialog
        {
            Title = "Сообщение",
            Content = message,
            CloseButtonText = "OK"
        };

        ContentDialogResult result = await dialog.ShowAsync();*/


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
        //form.Activate();
        var resultConnect = await form.ShowAsync();
        return resultConnect;
    }

    public async Task<bool> openFormConnectDBDialog2()
    {
        window.setFrame(typeof(DBConnectForm));
        return true;
    }
    public void showWindowDownload(bool flag)
    {
        window.setFrame(typeof(LoadingForm));
    }


    public void setProgressBar(int value)
    {
        window.getMethod("progressBarSetValue", new object[] { value });
    }



    private async Task<bool> askCloseSetting()
    {
        dialog = new ContentDialog();
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

    public void closeSetting()
    {
        window.setFrame(typeof(AllSilosComponent));
    }

    public void closeFormConnectDB()
    {

    }
    public void closeWindowDownload()
    {
        window.setFrame(typeof(MainPage));
        // frame.Navigate(typeof(MainWindow));
    }
    public void callSettingComponent()
    {
        bll.openSetting();
    }
    public void closeAdminSetting()
    {
        isAdminMode = false;
        window.getMethod("setNormalSetting");
    }

    public Task<bool> stopObserv()
    {
        try
        {
            return DialogShow("Требуется подтверждение", "Остановить опрос плат?");
        }
        catch 
        {
            throw new Exception("Error forms");
        }
    }

    public void setStopStyleForm()
    {
        window.getMethod("setStopObservMode");
        window.setStyle("Опрос остановлен");
    }

    public void setNormalStyleForm()
    {
        window.getMethod("setStartObservMode");
        window.setStyle("Обычные настройки");
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
        window.setStyle("Термометрия NIKA " + message);
        //window.Title = "Термометрия NIKA " + message;
    }
    public void overheatMessageBox(StringBuilder message, bool playSound)
    {


    }
    public bool openEnterForm(modeCheckPassword checkPassword)
    {
        //window
        return true;
    }


    public void openFormApplyKeyForm()
    {
        /*
        applyKeyForm = new ApplyKeyForm();
        applyKeyForm.Activate();*/

        window.setFrame(typeof(ApplyKeyForm));
        // window.getMethod<bool>("ShowAsync");
        //return await ApplyKeyForm.ShowAsync();
    }

    public string returnKeyApplyKeyForm()
    {
        //var hash = applyKeyForm.ShowAsync(bll);
        var hash = window.getReturnMethod("ShowAsync");
        //return hash.GetAwaiter().GetResult();
        return hash.ToString();
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

    private async Task<bool> DialogShow(string title, string content)
    {
        return await window.callAskDialog(content, title);
    }

    public void openNormalSetting()
    {
        window.getMethod("changeSetting");
    }

    public async bool enterPasswordAsync(modeCheckPassword checkPassword)
    {
        try
        {
            string result ="";
            while (true)
            {
                await window.enterPasswordAsync();
                result = window.costil;
                await window.callMessageBox(result);
                if (result == "")
                    return false;
                else
                    if (checkPassword.Invoke(result))
                        return true;
            }
        }
        catch
        {
            Debug.WriteLine("Error enterPasswordAsync"); 
            return false;
        }
    }

    public async Task openAdminSetting()
    {
        await Task.Run(() => window.getMethod("changeSetting"));
    }
}

