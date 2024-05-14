using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.UI.Xaml;
using System.Drawing;
using NPOI.HPSF;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities;
using SystemOfThermometry3.CustomComponent;
using SystemOfThermometry3.DeviceWorking;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.DAO;
using Windows.UI.ApplicationSettings;
using static SystemOfThermometry3.Services.SilosService;
using System.Text;

namespace SystemOfThermometry3.WinUIWorker;
public partial class WinUIWorker : IBisnesLogicLayer
{
    private IPresentationLayer presentation;

    #region переменные

    private DAO.Dao dao; //data access object - интерфейс, через который класс общается с базой данных 
    private SilosService silosService; // сервис для работы с силосами и платами
    private SettingsService settingsService; // сервис для работы с настройками
    private GrainService grainService;
    private WriterReader observer; // Опросчик плат
    private MailSender scheduler; //Отправляет письма
    private OverheatTrigger overheatTrigger; //Класс для обнаружения перегрева.
    private IntPtr hwnd1;
    //private DataBaseSettings dbSetingsDialog; // Форма с настройками подключения к бд, вызывается, если нет файла с строкой подключения

    // объекты для обработки запросов пользователя 
    private ExportExcelWorker exportWorker; //Форма с выбором времени для выгрузки.
    private SettingWorker settingWorker;
    private AllSilosWorker allSilosesWorker; // Компонент, на котором отображаются все силосы. 
    private OneSilosWorker oneSilosInfoWorker; // Компонент с информацией об одном силосе.
    private LogPanelWorker logPanelWorker;


    private int value;
    private Thread splashScreenThread; // Поток, в котором запускается форма загрузки
                                       // private AllFillingComponent allFillingComponent;
    private bool isSettingWindowOpen;
    private bool flagStrelka = true;

    #endregion

    public WinUIWorker(IPresentationLayer presentation)
    {
        if (presentation.openFormApplyKeyForm())
        {
            CustomClose();
            //return;
        }
        this.presentation = presentation;

        //Показывает экран загрузки в отдельном потоке, который после загрузки мейн формы прерывается
        splashScreenThread = presentation.showWindowDownload();


        presentation.showWindowDownload(true);
        dao = new MySQLDAO();
        presentation.openFormConnectDBDialog(ref dao);
        dbSetingsDialog.offlineModeEvent += successStartOfflineMode;

        string connectionString = FileProcessingService.getConnectionString();
        exportProgressBar.BringToFront();
        //isObserverRuning = false;
        isSettingWindowOpen = false;

        if (connectionString == "") //Первое подключение
        {
            presentation.openFormConnectDBDialog();
            //dbSetingsDialog.Focus();
        }
        else
            if (dao.connectToDB(connectionString, SettingsService.IsOnlyReadModeS) && (
                SettingsService.IsOnlyReadModeS || dao.DBisCorrect())) // попытка подключения
        {
            successStartWithDB(); //удачный старт
        }
        else // не удалось подключиться
        {
            dbSetingsDialog.ShowDialog();
        }
       presentation.setStatus("Готов");
    }


    public void successStartWithDB()
    {
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = false;
        initCustomComponent();
        if (settingsService.OfflineMode)
            presentation.setOfflineMode();
        else
        {
            if (settingsService.IsAdminMode)
                presentation.setAdminMode();
            else
                presentation.setNormalMode();
        }
        presentation.closeWindowDownload();
    }

    /// <summary>
    /// Вызывается, если работа начата в автономном режиме, т.е. без базы данных.
    /// </summary>
    public void successStartOfflineMode()
    {
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = true;

        initCustomComponent();
    }

    /// <summary>
    /// Иницилизирует все компоненты, делает привязку событий и тд.
    /// </summary>
    private void initCustomComponent()
    {
        presentation.closeFormConnectDB();
        grainService = new GrainService(dao, settingsService);
        silosService = new SilosService(dao, settingsService, grainService, presentation.setProgressBar);
        observer = new WriterReader(silosService, settingsService);
        observer.errorMessage += messageHandler; //Ошибка, напримет, связанная с открытием порта
        settingsService = new SettingsService(dao);
        //Сообщения из другого потока, обрабатываются специальным образом
        observer.asyncMessage += observerAsyncHandler; //Некоторое сообщение, которое не должно останавливать опрос
        observer.criticalErrorMessage += observerCriticalErrorHandler; //Сообщение об ошибке, после которой останавливается опрос
        observer.beginIterationEvent += observerBeginIterationHandler; //Сообщение о начале итерации
        observer.endIterationEvent += observerEndIterationEventHandler; //Сообщение о конце итерации
        
        observer.progressEvent += presentation.setProgressBar;

        scheduler = new MailSender(silosService, settingsService);
        scheduler.successSendEvent += mailSenderSuccessMessage;
        scheduler.errorSendEvent += mailSenderErrorMessage;
        settingWorker = new SettingWorker(dao, silosService, settingsService, grainService);
        settingWorker.changeReadMode += changeMode;
        overheatTrigger = new OverheatTrigger(silosService, settingsService);

        allSilosesWorker = new AllSilosWorker(this, silosService, settingsService, allFillingComponentInvoke);

        loggingRichTextBox.MinimumSize = new Size(settingsService.LogTextboxWidthHeight, settingsService.LogTextboxWidthHeight);
        hwnd1 = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

        refreshAllSettings();
        refreshSilosesTemperature();

        //sendMailTimer.Enabled = true;
        //changeMode();
    }

    /// <summary>
    /// Вызывается, если подключение неудачно
    /// </summary>
    public void CustomClose()
    {
        //stopObserv();
        //this.Close();
        System.Environment.Exit(0);
        //Application.Exit();
    }

    /// <summary>
    /// Смена модификатора доступа приложения
    /// </summary>
    private void changeMode()
    {

        if (settingsService.IsOnlyReadMode) //Переход в режим обзора
        {
            settingsService.IsAdminMode = true;
            settingsService.OfflineMode = false;
        }
        else
        {

            if (settingsService.IsAdminMode)
            {
                settingsService.IsAdminMode = false;
                allSilosesWorker.refreshButton();
            }
            else
            {
                settingsService.IsAdminMode = true;
                allSilosesWorker.refreshButton();
            }
        }
    }

    private void messageHandler(string message)
    {
        presentation.callMessageBox("Ошибка!\n" + message);
    }


    /// <summary>
    /// Сообщение пришедшее из другого потока, которое не должно останавливать опрос.
    /// </summary>
    /// <param name="message">Сообщение</param>
    private void observerAsyncHandler(string message)
    {
        //var messageBox = new MessageBoxCustomAsync(message);
        //presentation.callMessageBox();
        //может придти сообщение о переходн в автономный режим.
        sendLogMessage(message, Color.Black);
        showStatus();
    }

    /// <summary>
    /// Сообщение о начале новой итерации
    /// </summary>
    /// <param name="message"></param>
    private void observerBeginIterationHandler(string message)
    {
        sendLogMessage(message, Color.Green);
    }

    /// <summary>
    /// Сообщение об окончании итерации
    /// </summary>
    /// <param name="message"></param>
    private void observerEndIterationEventHandler(string message)
    {
        sendLogMessage(message, Color.Green);
        presentation.setStatus( message);
        refreshSilosesTemperature();
        checkOverheat();
    }


    private StringBuilder createTextOverheatMessage(Dictionary<Silos, Dictionary<Wire, int>> overheatSiloses)
    {
        var result = new StringBuilder("");

        result.Append(DateTime.Now.ToString("[HH:mm:ss]") + " Прегретые силосы:\r\n");
        foreach (Silos s in overheatSiloses.Keys)
        {
            result.Append(String.Format("Силос {0}, макс температура {1}\r\n", s.Name, s.Red.ToString()));
            foreach (KeyValuePair<Wire, int> item in overheatSiloses[s])
            {
                Wire w = item.Key;
                int count = item.Value;
                result.Append(String.Format("  Подвеска {0}, перегретых сенсоров - {1}/{2}\r\n",
                    w.Number, count, w.SensorCount));
            }
        }
        return result;
    }

    private void checkOverheat()
    {
        Dictionary<Silos, Dictionary<Wire, int>> overheatSiloses = overheatTrigger.getAndCheckOverheat();
        if (overheatSiloses == null || overheatSiloses.Count == 0)
            return;

        StringBuilder message = createTextOverheatMessage(overheatSiloses);

        presentation.overheatMessageBox(message, settingsService.OverheatPlaySound);
        if (settingsService.OverheatMailSend && !settingsService.IsOnlyReadMode)
            scheduler.sendOverheatMailAsync();
    }

    

    /// <summary>
    /// Сообщение о том, что произошла критическая ошибка и опрос остановлен
    /// </summary>
    /// <param name="message">Сообщение</param>
    private void observerCriticalErrorHandler(string message)
    {
        presentation.callMessageBox(message);
        try
        {

            sendLogMessage(message, Color.Red);
            settingsService.IsObserving = false;

            if (settingsService.IsHighlightWhenObservStop)
            {
                presentation.setStopStyleForm();
            }
        }
        catch
        {
            presentation.callMessageBox("Критическая ошибка!\n Перезагрузите программу!");
        }
    }


    /// <summary>
    /// Добавляет сообщение в журнал.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    private void sendLogMessage(string text, Color color)
    {
        presentation.sendLogMessage(text, color);
    }

    /// <summary>
    /// Изменяет название формы, показывает информацию о режиме работы.
    /// </summary>
    private void showStatus()
    {
        if (settingsService.IsOnlyReadMode)
        {
            presentation.setStatus("Термометрия NIKA. Обзор.");
        }
        else
        {
            presentation.setStatus ("Термометрия NIKA. "
                + (settingsService.OfflineMode ? " Автономный режим." : " Подключено к базе данных. ")
                + (settingsService.IsAdminMode ? " Режим Администратора." : " Режим пользователя."));

            //allSilosesWorker.refreshButton();
            //settingWorker.showStatus();
        }
    }

    /// <summary>
    /// Берет последние значения температур из базы данных
    /// </summary>
    private void refreshSilosesTemperature()
    {
        allSilosesWorker.refreshTemperature();
        //initOneSilosPanel();
        //oneSilosInfoWorker.showChosenSilos();
    }

    /// <summary>
    /// Переводит на главную вкладку.
    /// Делает запрос для получения списка силосов.
    /// Рисует силосы. Причем добавляются они только здесь.
    /// Может присылаться из окна настроек.
    /// </summary>
    private void refreshAllSiloses()
    {
        allSilosesWorker.refreshAll();
    }

    #region Обработка сообщений от отправщика писем

    /// <summary>
    /// Удачная рассылка
    /// </summary>
    /// <param name="message"></param>
    private void mailSenderSuccessMessage(string message)
    {
        sendLogMessage(message, Color.Green);
    }

    /// <summary>
    /// Неудачная рассылка
    /// </summary>
    /// <param name="message"></param>
    private void mailSenderErrorMessage(string message)
    {
        sendLogMessage(message, Color.Red);
    }
    #endregion


    /// <summary>
    /// Обновление настроек
    /// </summary>
    public void refreshAllSettings()
    {
        //changeMode();

        if (settingsService.IsOnlyReadMode) //Обновляем силосы из бд.
        {
            silosService.refreshAllSilosAndDevise();
            silosService.refreshLastTemperatures();
        }

        isSettingWindowOpen = false;
        //settingWorker.refreshAll();
        refreshAllSiloses();

        showStatus();
    }


    private void allFillingComponentInvoke()
    {

        if (allFillingComponent == null)
        {
            allFillingComponent = new AllFillingComponent(silosService, backAllSilosComponentShow);
            allFillingComponent.Show();
        }
        allFillingComponent.refreshComponent();
        allFillingComponent.Visible = true;
        logPan.Visible = false;
        allFillingComponent.Dock = DockStyle.Fill;
        allFillingComponent.Parent = allSilosesPanel;

    }


    /// <summary>
    /// Запуск опроса
    /// </summary>
    public void startObserv()
    {
        if (settingsService.IsOnlyReadMode)
        {
            presentation.callMessageBox("Опрос невозможен в режиме \"Обзор\"");
            return;
        }

        if (isSettingWindowOpen)
        {
            presentation.callMessageBox("Закройте окно настроек!");
            return;
        }

        if (observer.startAsyncObservation())
        {
            settingsService.IsObserving = true;
            presentation.setNormalStyleForm();
        }
    }

    public void stopObserv()
    {
        if (observer != null)
        {
            presentation.sendLogMessage("Ожидание остановки опроса.", Color.Red);
            presentation.setStatus("Ожидание остановки опроса.");
            observer.stopAsyncObservation();
        }

        presentation.sendLogMessage("Опрос остановлен.", Color.Red);
        presentation.setStatus("Опрос остановлен.");
        settingsService.IsObserving = false;

        if (settingsService.IsHighlightWhenObservStop)
        {
            presentation.setStopStyleForm();
        }
    }



    private void refreshSetting()
    {
        if (settingsService.IsAdminMode)

            settingWorker.RefreshAll();
        else
        {
            settingWorker.refreshAll();
        }
    }

    private void reapitConnect()
    {
        try
        {
            if (!dao.connectToDB(FileProcessingService.getConnectionString(), settingsService.IsOnlyReadMode))
            {
                presentation.callMessageBox("Не удалось подключиться!\n" +
                    "проверьте имя пользователя и пароль");
            }
            else
            {
                presentation.callMessageBox("Подключение успешно!");
                silosService.synchronizeWithGettingData();
                refreshAllSettings();

                refreshSetting();
            }
            showStatus();
        }
        catch (Exception ex)
        {
            presentation.callMessageBox("Не удалось подключиться к базе данных");
        }
    }

    private void adminMode_Click()
    {

        if (settingsService.IsOnlyReadMode) //Выхоидм из режима обзор
            adminEnter();

        else
        {
            if (settingsService.IsAdminMode) // Выходим из режиема администратора
            {
                //changeMode();
                changeStatus();
            }
            else
                adminEnter();
        }
    }


    public bool checkOperatorPassword(string password)
    {
        new checkPswdHandler((pswd) =>
        {
            return SecurityService.checkMainOperPassword(pswd);
        });
        return true;
    }

    public bool checkAdminPassword(string password)
    {
        return false;
    }

    public bool checkProviderPassword(string password)
    {
        return false;
    }

    private bool enterOperator()
    {
        presentation.openEnterForm(checkOperatorPassword);
        return true;
    }

    private void simpleSet_Click(object sender, EventArgs e)
    {
        if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
            if (enterOperator())
                return;

        if (observer.IsRunning)
        {
            if (presentation.stopObserv() == true)
            {
                presentation.stopObservMode();
                stopObserv();
                presentation.setStatus("Опрос остановлен");

            }
        }
    }

    public void changeStatus()
    {
        changeMode();
        showStatus();
    }

    private void adminSet_Click()
    {

        if (!settingsService.IsAdminMode) // пытаемся войти как администратор
        {
            adminEnter();
        }

        if (!settingsService.IsAdminMode) // пытаемся войти как администратор
        {
            return;
        }

        if (observer.IsRunning)
        {
            if (presentation.closeSetting() == true)
            {
                presentation.stopObservMode();
                stopObserv();
                Thread.Sleep(300);
            }
        }
        if (!observer.IsRunning)
        {
            isSettingWindowOpen = true;
            try
            {
                presentation.callSettingComponent(settingsService, true);
            }
            catch (Exception ex)
            {
                MyLoger.LogError(ex.Message);
            }

        }
    }

    private void adminEnter()
    {
        presentation.openEnterForm(checkAdminPassword);
    }

    public SilosService getAllSilos() => throw new NotImplementedException();
    public GrainService getGrainService() => throw new NotImplementedException();
    public void runObserv() => throw new NotImplementedException();
    public void exportExcel() => throw new NotImplementedException();
    public void painthart() => throw new NotImplementedException();
    public void getFilling() => throw new NotImplementedException();
    public void calculateFilling() => throw new NotImplementedException();
    public void enterAdminMode() => throw new NotImplementedException();
    public void changeTemperature() => throw new NotImplementedException();
}
