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
using System.Windows.Forms;
using Microsoft.UI.Composition;
using System.Diagnostics;
using System.Windows.Forms.Design;

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
    private AllSilosWorker allSilosesWorker; // Компонент, на котором отображаются все силосы. 
    private OneSilosWorker oneSilosInfoWorker; // Компонент с информацией об одном силосе.
    private LogPanelWorker logPanelWorker;


    private int value;
    private Thread splashScreenThread; // Поток, в котором запускается форма загрузки
                                       // private AllFillingComponent allFillingComponent;
    private bool isSettingWindowOpen = false;
    private bool flagStrelka = true;

    #endregion

    public WinUIWorker(IPresentationLayer presentation)
    {
        this.presentation = presentation;

        checkApplyKey();

        connectDB();

        this.presentation.showWindowDownload(true);

        this.presentation.closeFormConnectDB();

        startMainForm();


        
            
        presentation.setStatus("Готов");
    }


    private void checkApplyKey()
    {
        if (!SecurityService.IsActivate())
        {
            if (!this.presentation.openFormApplyKeyForm())
                CustomClose();
        }
    }

    private void connectDB()
    {
        dao = new MySQLDAO();
        string connectionString = FileProcessingService.getConnectionString();


        if (connectionString == "") //Первое подключение
        {
            presentation.openFormConnectDBDialog();
        }
        else
        {
            if (dao.connectToDB(connectionString, SettingsService.IsOnlyReadModeS) && (
                    SettingsService.IsOnlyReadModeS || dao.DBisCorrect())) // попытка подключения
            {
                successStartWithDB(); //удачный старт
            }
            else // не удалось подключиться
            {
                presentation.closeFormConnectDB();
            }
        }
    }

    private void startMainForm()
    {
        
    }

    

    public void successStartWithDB()
    {
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = false;
        initCustomComponent();
        /*if (settingsService.OfflineMode)
            presentation.setOfflineMode();
        else
        {
            if (settingsService.IsAdminMode)
                presentation.setAdminMode();
            else
                presentation.setNormalMode();
        }*/
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
        overheatTrigger = new OverheatTrigger(silosService, settingsService);


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
                presentation.setNormalMode();
            }
            else
            {
                settingsService.IsAdminMode = true;
                presentation.setAdminMode();
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
        presentation.setStatus(message);
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
            presentation.setStatus("Термометрия NIKA. "
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
        presentation.refreshAllSilosTemperatur();
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
        presentation.refreshAllSilos();
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


    public void runStopObserv()
    {
        if (!settingsService.IsObserving)
        {
            if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
                if (!presentation.openEnterForm(checkOperatorPassword))
                    return;
            silosService.deleteLastTemperatur();
            startObserv();
            presentation.runObservMode();
        }
        else
        {

            // Попытка ввести пароль оператора.
            if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
                if (!presentation.openEnterForm(checkOperatorPassword))
                    return;
            presentation.setProgressBar(-2);
            stopObserv();
            presentation.stopObservMode();
        }
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
        presentation.refreshSetting();
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

    public void changeAdminMode()
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
        return SecurityService.checkMainOperPassword(password);
    }

    public bool checkAdminPassword(string password)
    {
        return SecurityService.checkAdminPassword(password);
    }

    public bool checkProviderPassword(string password)
    {
        return SecurityService.checkProviderPassword(password);
    }


    public void changeStatus()
    {
        changeMode();
        showStatus();
    }

    public void openSetting()
    {
        if (observer.IsRunning)
        {
            if (presentation.stopObserv() == true)
            {
                presentation.stopObservMode();
                stopObserv();
                Thread.Sleep(300);
            }
            else
                return;

        }
        try
        {
            isSettingWindowOpen = true;
            if (settingsService.IsAdminMode)
                presentation.openAdminSetting();
            else
                presentation.openNormalSetting();
        }
        catch (Exception ex)
        {
            MyLoger.LogError(ex.Message);
        }
    }

    private void adminEnter()
    {
        presentation.openEnterForm(checkAdminPassword);
    }

    public int getFilling(int silosId)
    {
        Silos silos = silosService.getSilos(silosId);
        Dictionary<int, float[]> temperature = new Dictionary<int, float[]>();
        Dictionary<int, Wire> wires = silos.getSortedByNumberWires();


        foreach (Wire w in wires.Values)
            if (temperature.ContainsKey(w.Id))
                temperature[w.Id] = silosService.getLastTempForWire(w);
            else
                temperature.Add(w.Id, silosService.getLastTempForWire(w));

        int result = HistoryFillingService.getFilling(temperature, wires);
        return result;
    }


    /// <summary>
    /// пока не сделал
    /// </summary>
    /// <returns></returns>
    public bool enterAdminMode()
    {
        return true;
    }

    public bool isAdminMode()
    {
        return settingsService.IsAdminMode;
    }
    public void changeTemperature(DateTime time)
    {
        silosService.refreshTemperature(time);
    }

    public Dictionary<int, string[]> getFillingChart()
    {
        Dictionary<int, string[]> result = new Dictionary<int, string[]>();
        DateTime time = silosService.LastTempTime;
        silosService.getTime(ref time);
        foreach (Silos silos in silosService.getAllSiloses().Values)
        {
            string[] fil = silosService.getFillingAndGrain(silos.Id, time);
            if (fil == null)
            {
                result.Add(silos.Id, new string[2] { "0", "-" });
                continue;
            }
            if (fil[0] != "0")
            {
                result.Add(silos.Id, new string[2] { "0", "-" });
                continue;
            }
            if (fil[1] == "-1")
            {
                result.Add(silos.Id, new string[2] { fil[0], "-" });
                continue;
            }
            result.Add(silos.Id, new string[2] { fil[0], fil[1] });
        }
        return null;
    }
    public Dictionary<int, float[]> getDataGridTemp(int silosId)
    {
        Dictionary<int, float[]> temp = new Dictionary<int, float[]>();
        Dictionary<int, Wire> wires;
        Silos silos = silosService.getSilos(silosId);
        if (settingsService.IsSortWiresX)
            wires = silos.getSortedByXWires();
        else
            wires = silos.getSortedByNumberWires();

        foreach (Wire w in wires.Values)
            if (temp.ContainsKey(w.Id))
                temp[w.Id] = silosService.getLastTempForWire(w);
            else
                temp.Add(w.Id, silosService.getLastTempForWire(w));
        return temp;
    }

    public Dictionary<int, Silos> getTempMaxMidMin()
    {
        Dictionary<int, Silos> result = silosService.getAllSiloses();

        return result;
    }
    public bool attemptChangeImageBackGround(string fileName)
    {
        if (settingsService.IsAdminMode)
        {
            settingsService.SilosBackgroundFilePath = fileName;
            return true;
        }
        return false;
    }
    public bool attemptDragSilos()
    {
        return settingsService.IsAdminMode;
    }
    public int getFilling(int silosId, DateTime time)
    {
        silosService.getFillingAndGrain(silosId, time);

        return 0;
    }
    public void refreshConnect()
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
    public Dictionary<int, Grain> getGrains()
    {

        return grainService.getGrains();
    }
    public Grain addGrain()
    {
        return grainService.addGrain();
    }
    public void updateGrain(Grain g)
    {
        grainService.updateGrains(g);
    }
    public void deleteGrain(Grain g)
    {
        grainService.deleteGrains(g);
    }
    public Dictionary<int, string> getListDivision()
    {
        Dictionary<int, StructureSubdivision> divisions = silosService.getSubdivisions();
        Dictionary<int, string> result = new Dictionary<int, string>();
        foreach (StructureSubdivision item in divisions.Values)
        {
            result.Add(item.Id, item.Name);
        }
        return result;
    }
    public int addDivision(string name)
    {
        silosService.addSubdivision(name);
        return 0;
    }
    public void updateDivision(int id, string name)
    {
        StructureSubdivision subdivision = new StructureSubdivision(id, name);

        silosService.updateSubdivisions(subdivision);
    }
    public void deleteDivision(int id)
    {
        silosService.deleteSubdivision(id);
    }
    public void averaginTemperatureValues()
    {
        settingsService.IsGetMidValueForBrokenSensor = !settingsService.IsGetMidValueForBrokenSensor;
    }
    public bool getModeAveraginTemperatureValues()
    {
        return settingsService.IsGetMidValueForBrokenSensor;
    }
    public string getPathSaveFile()
    {
        return FileProcessingService.ExportDir;
    }
    public void changeProviderPassword(string oldPassword, string newPassword, string newRepeatPassword)
    {

    }
    public string[][] getMailAdress()
    {
        return settingsService.getMailDestinationsWithNotes();
    }
    public void getSettingMail(out string mailSMTPtextBox, out string mailSenderTextBox, out string mailPasswordTextBox, out string mailCaptionTextBox,
        out bool addPlotsCheckBox, out int mailTimeNumericUpDown, out bool mailSEndByTimeCheckBox)
    {
        mailSMTPtextBox = settingsService.MailSmtpServer;
        mailSenderTextBox = settingsService.MailSender;
        mailPasswordTextBox = settingsService.MailSenderPassword;
        mailCaptionTextBox = settingsService.MailCaption;
        addPlotsCheckBox = settingsService.MailIsSendPlot;
        mailTimeNumericUpDown = settingsService.MailSendingHour;
        mailSEndByTimeCheckBox = settingsService.MailSendByTimer;
    }
    public string[][] getErrorMailAdress()
    {
        return settingsService.MailErrorDestination;
    }
    public void getSettingErrorMail(out string MailErrorSmtpServer, out string MailErrorSender, out string MailErrorSenderPassword,
        out string MailErrorCaption, out bool MailErrorSendByTimer, out int MailErrorSendingPeriod)
    {
        MailErrorSmtpServer = settingsService.MailErrorSmtpServer;
        MailErrorSender = settingsService.MailErrorSender;
        MailErrorSenderPassword = settingsService.MailErrorSenderPassword;
        MailErrorCaption = settingsService.MailErrorCaption;
        MailErrorSendByTimer = settingsService.MailErrorSendByTimer;
        MailErrorSendingPeriod = settingsService.MailErrorSendingPeriod;

    }

    public void connectDB(string connectionString)
    {
        if (!dao.connectToDB(getConnectionString(), settingsService.IsOnlyReadMode))
        {
            presentation.callMessageBox("Не удалось подключиться!\n" +
                "проверьте имя пользователя и пароль");
        }
        else
        {
            presentation.callMessageBox("Подключение успешно!");
            FileProcessingService.setConnectionString(getConnectionString());
            silosService.synchronizeWithGettingData();
            //connectedEvent?.Invoke();
            //silosTabControl.tryToGetInformFromDB(); //Обновляем
            //RefreshAll();
        }
        showStatus();
    }

    public string getConnectionString()
    {
        return FileProcessingService.getConnectionString();
    }

    float[][] IBisnesLogicLayer.getDataGridTemp(int silosId) => throw new NotImplementedException();
    public bool attemptChangeImageBackGround()
    {
        return true;
    }


    public int calculateFilling(DateTime start)
    {
        return 0;
    }


    public void saveStructureSubvision()
    {

    }

    public string getSSHKey()
    {
        return SecurityService.GetActivateKeyRequestFromHardware();
    }

    public bool checkSSHKey(string ssh)
    {
        return SecurityService.CheckActivateKey(ssh);
    }

}
