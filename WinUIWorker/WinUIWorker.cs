using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.UI.Xaml;
using System.Drawing;
using NPOI.HPSF;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities;
using SystemOfTermometry2.CustomComponent;
using SystemOfTermometry2.DeviceWorking;
using SystemOfTermometry2.Model;
using SystemOfTermometry2.Services;
using SystemOfThermometry2.DAO;
using SystemOfThermometry2.Services;
using SystemOfTermometry2.WinUIWorker;
using Windows.UI.ApplicationSettings;
using static SystemOfTermometry2.Services.SilosService;

namespace SystemOfTermometry2.WinUIWorker;
public class WinUIWorker: IBisnesLogicLayer
{
    private Window mainForm;

    #region переменные

    private DAO.Dao dao; //data access object - интерфейс, через который класс общается с базой данных 
    private SilosService silosService; // сервис для работы с силосами и платами
    private SettingsService settingsService; // сервис для работы с настройками
    private GrainService grainService;
    private WriterReader observer; // Опросчик плат
    private MailSender scheduler; //Отправляет письма
    private OverheatTrigger overheatTrigger; //Класс для обнаружения перегрева.
    public delegate void Progress(int value);
    private Progress progressDel;
    private IntPtr hwnd1;
    //private DataBaseSettings dbSetingsDialog; // Форма с настройками подключения к бд, вызывается, если нет файла с строкой подключения


    private ExportToExcelForm exportForm; //Форма с выбором времени для выгрузки.
    private OverheatMessageBox overheatMessageBox; //Форма с сообщением о перегреве.
    private SimpleSettingPan simpleSettingsPan;
    private AllSilosWorker allSilosesComponent; // Компонент, на котором отображаются все силосы. 
    private OneSilosFullinformation oneSilosInfoComponent; // Компонент с информацией об одном силосе.
    private PlotBuildingComponent plotBuildingComponent; // Компонент, который строит графики.
    private NewSettingPann newSettingPann;
    private ToolStripLog toolStripLogHost; // Содержит внутри журнал событий

    private int value;
    private Thread splashScreenThread; // Поток, в котором запускается форма загрузки
                                       // private AllFillingComponent allFillingComponent;
    private bool isSettingWindowOpen;
    private bool flagStrelka = true;

    #endregion

    public WinUIWorker(Window window)
    {
        if (ApplyKeyForm.EnterKey() != DialogResult.Yes)
        {
            CustomClose();
            //return;
        }
        mainForm = window;
        //Показывает экран загрузки в отдельном потоке, который после загрузки мейн формы прерывается
        splashScreenThread = SplashScreenForm.ShowAsync();
        InitializeComponent();
        this.BackColor = Color.BlueViolet;



        //workPanel.Height = winSize.Height;
        //allSilosesPanel.Height = workPanel.Height - logPan.Height;
        this.SetStyle(ControlStyles.ResizeRedraw, true);
        dao = new MySQLDAO();
        dbSetingsDialog = new DataBaseSettings(ref dao);
        dbSetingsDialog.closeEvent += CustomClose;
        dbSetingsDialog.connectedEvent += successStartWithDB;
        dbSetingsDialog.offlineModeEvent += successStartOfflineMode;

        string connectionString = FileProcessingService.getConnectionString();
        exportProgressBar.BringToFront();
        //isObserverRuning = false;
        isSettingWindowOpen = false;

        if (connectionString == "") //Первое подключение
        {
            dbSetingsDialog.ShowDialog();
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
        observationStatusToolStripStatusLabel.Text = "Готов";
    }


    private void successStartWithDB()
    {
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = false;
        initCustomComponent();
        if (settingsService.OfflineMode)
            adminMode.Image = Properties.Resources.readModeGreen;
        else
        {
            if (settingsService.IsAdminMode)
                adminMode.Image = Properties.Resources.adminModeGreen;
            else
                adminMode.Image = Properties.Resources.adminModeGray1;
        }
        splashScreenThread.Abort();
    }


    private bool flagSetting = false;
    private void butSet_Click(object sender, EventArgs e)
    {
        if (!flagSetting)
        {
            menuSetting.Show();
            flagSetting = true;
        }
        else
        {
            menuSetting.Hide();
            flagSetting = false;
        }
    }

    /// <summary>
    /// Изменяем выбранный силос
    /// </summary>
    /// <param name="sender">силос</param>
    public void changeLastSilos(Silos sender)
    {
        //tabControl1.SelectedIndex = 1;
        openSelectSilos();
        oneSilosInfoComponent.showSilos(sender);
    }

    /// <summary>
    /// Вызывается, если работа начата в автономном режиме, т.е. без базы данных.
    /// </summary>
    private void successStartOfflineMode()
    {
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = true;

        initCustomComponent();
    }
    #region инициализация форм при первом обращении

    public void initAdminSetting()
    {
        if (newSettingPann == null)
            newSettingPann = new NewSettingPann(dao, silosService, settingsService, grainService);
        if (simpleSettingsPan != null)
            simpleSettingsPan.Hide();
    }

    private void initSimpleSetting()
    {
        if (simpleSettingsPan == null)
            simpleSettingsPan = new SimpleSettingPan(settingsService, silosService, grainService);
        if (newSettingPann != null)
            newSettingPann.Hide();
    }


    public void initSetting()
    {
        if (settingsService == null)
            settingsService = new SettingsService(dao);

    }

    public void initExportToExcel()
    {

    }

    public void initStatistic()
    {
        if (plotBuildingComponent == null)
        {
            plotBuildingComponent = new PlotBuildingComponent(silosService, settingsService);
            plotBuildingComponent.Parent = staticPan;
            plotBuildingComponent.Dock = DockStyle.Fill;
        }

    }

    private void initOneSilosPanel()
    {
        if (oneSilosInfoComponent == null)
        {
            oneSilosInfoComponent = new OneSilosFullinformation(silosService, settingsService, allSilShow);
            oneSilosInfoComponent.Dock = DockStyle.Fill;
            oneSilosInfoComponent.Parent = oneSilosPanel;
            return;
        }
        oneSilosInfoComponent.Show();
    }

    #endregion
    /// <summary>
    /// Иницилизирует все компоненты, делает привязку событий и тд.
    /// </summary>
    private void initCustomComponent()
    {
        dbSetingsDialog.Hide();
        grainService = new GrainService(dao, settingsService);
        silosService = new SilosService(dao, settingsService, grainService, progressBarWorking, progressBarOn);
        observer = new WriterReader(silosService, settingsService);
        observer.errorMessage += messageHandler; //Ошибка, напримет, связанная с открытием порта

        //Сообщения из другого потока, обрабатываются специальным образом
        observer.asyncMessage += observerAsyncHandler; //Некоторое сообщение, которое не должно останавливать опрос
        observer.criticalErrorMessage += observerCriticalErrorHandler; //Сообщение об ошибке, после которой останавливается опрос
        observer.beginIterationEvent += observerBeginIterationHandler; //Сообщение о начале итерации
        observer.endIterationEvent += observerEndIterationEventHandler; //Сообщение о конце итерации
        observer.sendDataEvent += observerSendDataEventHandler; // Сообщение, которое несет в себе информацию о последнем запросе
                                                                // ***

        observer.progressEvent += progressBarWorking;

        scheduler = new MailSender(silosService, settingsService);
        scheduler.successSendEvent += mailSenderSuccessMessage;
        scheduler.errorSendEvent += mailSenderErrorMessage;
        newSettingPann = new NewSettingPann(dao, silosService, settingsService, grainService);
        newSettingPann.changeReadMode += changeMode;
        overheatTrigger = new OverheatTrigger(silosService, settingsService);
        overheatMessageBox = new OverheatMessageBox();

        allSilosesComponent = new AllSilosesComponent(this, silosService, settingsService, allFillingComponentInvoke);
        allSilosesComponent.Parent = allSilosesPanel;
        allSilosesComponent.Dock = DockStyle.Fill;
        allSilosesComponent.AutoSize = true;

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
                allSilosesComponent.refreshButton();
            }
            else
            {
                settingsService.IsAdminMode = true;
                allSilosesComponent.refreshButton();
            }
        }
    }

    private void messageHandler(string message)
    {
        MessageBox.Show("Ошибка!\n" + message);
    }


    /// <summary>
    /// Сообщение пришедшее из другого потока, которое не должно останавливать опрос.
    /// </summary>
    /// <param name="message">Сообщение</param>
    private void observerAsyncHandler(string message)
    {
        //var messageBox = new MessageBoxCustomAsync(message);
        //messageBox.Show();
        //может придти сообщение о переходн в автономный режим.
        addMessageToTextBoxLoger(message, Color.Black);
        showStatus();
    }

    /// <summary>
    /// Сообщение о начале новой итерации
    /// </summary>
    /// <param name="message"></param>
    private void observerBeginIterationHandler(string message)
    {
        addMessageToTextBoxLoger(message, Color.Green);
    }

    /// <summary>
    /// Сообщение об окончании итерации
    /// </summary>
    /// <param name="message"></param>
    private void observerEndIterationEventHandler(string message)
    {
        addMessageToTextBoxLoger(message, Color.Green);
        observationStatusToolStripStatusLabel.Text = message;
        refreshSilosesTemperature();
        checkOverheat();
    }

    private void checkOverheat()
    {
        Dictionary<Silos, Dictionary<Wire, int>> overheatSiloses = overheatTrigger.getAndCheckOverheat();
        if (overheatSiloses == null || overheatSiloses.Count == 0)
            return;

        overheatMessageBox.Show(overheatSiloses, settingsService.OverheatPlaySound);
        if (settingsService.OverheatMailSend && !settingsService.IsOnlyReadMode)
            scheduler.sendOverheatMailAsync();
    }

    /// <summary>
    /// НЕ ИСПОЛЬЗУЕТСЯ
    /// Сообщение, которое присылает данные, считанные в ходе итерации. Не используется
    /// </summary>
    /// <param name="siloses">Силосы</param>
    /// <param name="tempertures">Температуры</param>
    private void observerSendDataEventHandler(Dictionary<int, Silos> siloses, Dictionary<int, Dictionary<int, float[]>> tempertures)
    {
        //Таким образом обрабатываются асинхронные сообщения

        {
            //refreshSilosesTemperatureFromMessage(siloses, tempertures);
        }
        ));
    }

    /// <summary>
    /// Сообщение о том, что произошла критическая ошибка и опрос остановлен
    /// </summary>
    /// <param name="message">Сообщение</param>
    private void observerCriticalErrorHandler(string message)
    {
        MessageBox.Show(message);
        try
        {

            addMessageToTextBoxLoger(message, Color.Red);
            //observationStatusToolStripStatusLabel.Text = message;
            //startObservToolStripMenuItem.Enabled = true;
            //stopObservToolStripMenuItem.Enabled = false;
            settingsService.IsObserving = false;

            if (settingsService.IsHighlightWhenObservStop)
            {
                //MainMenuStrip.BackColor = Color.FromArgb(255, 90, 80);
                loggingRichTextBox.BackColor = Color.FromArgb(255, 90, 80);
            }
            //isObserverRuning = false;
        }
        catch
        {
            MessageBox.Show("Критическая ошибка!\n Перезагрузите программу!");
        }
    }


    /// <summary>
    /// Добавляет сообщение в журнал.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    private void addMessageToTextBoxLoger(string text, Color color)
    {
        loggingRichTextBox.SelectionColor = color;
        if (loggingRichTextBox.Text.Length > 20000)
        {
            loggingRichTextBox.Text = loggingRichTextBox.Text.Substring(10000);
            int new_lent = loggingRichTextBox.Text.Length;
        }
        loggingRichTextBox.AppendText(DateTime.Now.ToString("[HH:mm:s] ") + text + "\r\n");

        loggingRichTextBox.ScrollToCaret();
    }

    /// <summary>
    /// Изменяет название формы, показывает информацию о режиме работы.
    /// </summary>
    private void showStatus()
    {
        if (settingsService.IsOnlyReadMode)
        {
            mainForm.Title = "Термометрия NIKA. Обзор.";
        }
        else
        {
            mainForm.Title = "Термометрия NIKA. "
                + (settingsService.OfflineMode ? " Автономный режим." : " Подключено к базе данных. ")
                + (settingsService.IsAdminMode ? " Режим Администратора." : " Режим пользователя.");

            //allSilosesComponent.refreshButton();
            //simpleSettingsPan.showStatus();
        }
    }

    /// <summary>
    /// Берет последние значения температур из базы данных
    /// </summary>
    private void refreshSilosesTemperature()
    {
        allSilosesComponent.refreshTemperature();
        //initOneSilosPanel();
        //oneSilosInfoComponent.showChosenSilos();
    }

    /// <summary>
    /// Переводит на главную вкладку.
    /// Делает запрос для получения списка силосов.
    /// Рисует силосы. Причем добавляются они только здесь.
    /// Может присылаться из окна настроек.
    /// </summary>
    private void refreshAllSiloses()
    {
        //      oneSilosInfoComponent.refreshAll(); //Обнавляем панель с одним силосом
        allSilosesComponent.refreshAll();

        //Обновляем журнал здесь, т.к. при закрытии окна настроек вызывается эта функция
        //toolStripLogHost.
        loggingRichTextBox.MinimumSize = new Size(settingsService.LogTextboxWidthHeight, settingsService.LogTextboxWidthHeight);
    }

    #region Обработка сообщений от отправщика писем

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    private void mailSenderSuccessMessage(string message)
    {


        addMessageToTextBoxLoger(message, Color.Green);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    private void mailSenderErrorMessage(string message)
    {


        addMessageToTextBoxLoger(message, Color.Red);

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
        //plotBuildingComponent.refreshAll();
        //simpleSettingsPan.refreshAll();
        refreshAllSiloses();

        showStatus();
    }


    private void progressBarOn()
    {
        exportProgressBar.Parent = this;
        exportProgressBar.BringToFront();
        exportProgressBar.Dock = DockStyle.Bottom;
        exportProgressBar.Visible = true;
        exportProgressBar.Enabled = true;
        exportProgressBar.Value = 0;
        exportProgressBar.ProgressColor = Color.FromArgb(44, 255, 137);
        exportProgressBar.ProgressColor2 = Color.FromArgb(139, 42, 227);
    }

    public void backAllSilosComponentShow()
    {

        if (allFillingComponent != null)
            allFillingComponent.Hide();
        allSilosesComponent.Visible = true;
        logPan.Visible = true;

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

    private void progressBarWorking(int value)
    {

        switch (value)
        {
            case -1:
                progressBarOn();
                break;
            case -2:

                exportProgressBar.ProgressColor = Color.Red;
                exportProgressBar.ProgressColor2 = Color.DarkRed;
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                TaskbarProgress.SetValue(Handle, value, 100);
                redTimer.Start();
                animationTimer.Enabled = true;
                break;

            case 100:
                TaskbarProgress.SetValue(Handle, value, 100);
                exportProgressBar.Value = value;
                animationTimer.Enabled = true;
                break;

            default:
                TaskbarProgress.SetValue(Handle, value, 100);
                exportProgressBar.Value = value;
                break;
        }

    }

    /// <summary>
    /// Событие окончания выгрузки
    /// </summary>
    /// <param name="message">сообщение</param>
    /// <param name="success">удачность</param>
    private void exportEndEvent(string message, bool success)
    {


        if (success)
        {
            addMessageToTextBoxLoger("Выгрузка сделана успешно!", Color.Green);
            MessageBox.Show("Выгрузка сделана успешно!");
        }
        else
        {
            MessageBox.Show("Не удалось сделать выгрузку в Файл. \n" +
                "Возможно он открыт в другой программе");
            addMessageToTextBoxLoger("Неудалось сделать выгрузку!", Color.Red);
        }

    }

    /// <summary>
    /// Запуск опроса
    /// </summary>
    public void startObserv()
    {
        if (settingsService.IsOnlyReadMode)
        {
            MessageBox.Show("Опрос невозможен в режиме \"Обзор\"");
            return;
        }

        if (isSettingWindowOpen)
        {
            MessageBox.Show("Закройте окно настроек!");
            return;
        }

        if (observer.startAsyncObservation())
        {
            //isObserverRuning = true;
            //startObservToolStripMenuItem.Enabled = false;
            //stopObservToolStripMenuItem.Enabled = true;
            settingsService.IsObserving = true;
            //MainMenuStrip.BackColor = SystemColors.Control;
            loggingRichTextBox.BackColor = Color.White;
        }
    }

    public void stopObserv()
    {
        if (observer != null)
        {
            addMessageToTextBoxLoger("Ожидание остановки опроса.", Color.Red);
            observationStatusToolStripStatusLabel.Text = "Ожидание остановки опроса.";
            observer.stopAsyncObservation();
        }

        addMessageToTextBoxLoger("Опрос остановлен.", Color.Red);
        observationStatusToolStripStatusLabel.Text = "Опрос остановлен.";
        //isObserverRuning = false;
        //startObservToolStripMenuItem.Enabled = true;
        //stopObservToolStripMenuItem.Enabled = false;
        settingsService.IsObserving = false;

        if (settingsService.IsHighlightWhenObservStop)
        {
            //MainMenuStrip.BackColor = Color.FromArgb(255, 90, 80);
            loggingRichTextBox.BackColor = Color.FromArgb(255, 90, 80);
        }
    }

    public void allSilShow()
    {

        {
            logPan.Visible = true;
            allSilosesPanel.Visible = true;
            allSilosesComponent.Parent = allSilosesPanel;
            allSilosesComponent.Dock = DockStyle.Fill;
            allSilosesComponent.AutoSize = true;
            //allSilosesComponent.refreshAll();

        }



        private void butSil_Click(object sender, EventArgs e)
        {
            //if (exportProgressBar.Enabled == true)
            //    exportProgressBar.Visible = true;
            logPan.Visible = true;
            allSilosesPanel.Visible = true;
            allSilosesComponent.Parent = allSilosesPanel;
            allSilosesComponent.Dock = DockStyle.Fill;
            allSilosesComponent.AutoSize = true;
            //allSilosesComponent.refreshAll();
            backAllSilosComponentShow();
        }


    private bool flagObserv = false;
    private void butRunStop_Click(object sender, EventArgs e)
    {
        if (!flagObserv)
        {
            butRunStop.Image = Properties.Resources.runGreen;
            butRunStop.Text = "Остановить опрос";
            flagObserv = true;
            if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
                if (EnterPasswordForm.enterOperator() != DialogResult.Yes)
                    return;
            silosService.deleteLastTemperatur();
            startObserv();
        }
        else
        {
            flagObserv = false;
            butRunStop.Text = "Опрос датчиков";
            butRunStop.Image = Properties.Resources.runGrey1;
            // Попытка ввести пароль оператора.
            if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
                if (EnterPasswordForm.enterOperator() != DialogResult.Yes)
                    return;
            progressBarWorking(-2);
            stopObserv();
        }
    }

    private bool flagVis = false;
    private void butEx_Click(object sender, EventArgs e)
    {
        if (flagVis)
        {
            filePanel.Show();
            flagVis = false;

        }
        else
        {
            flagVis = true;
            filePanel.Hide();
        }
    }

    public void openSelectSilos()
    {
        initOneSilosPanel();
        allSilosesPanel.Visible = false;
        logPan.Visible = false;
        oneSilosPanel.Show();
        oneSilosPanel.Parent = workPanel;
        oneSilosPanel.Dock = DockStyle.Fill;
    }


    private void butStat_Click(object sender, EventArgs e)
    {
        claerWorkPan();
        initStatistic();
        staticPan.Show();
        staticPan.Dock = DockStyle.Fill;
    }


    /// <summary>
    /// Отчищает workPanel от других панелей
    /// </summary>
    private void claerWorkPan()
    {
        allSilosesPanel.Visible = false;
        oneSilosPanel.Hide();
        logPan.Hide();
        settingPan.Hide();
        staticPan.Hide();
        isSettingWindowOpen = false;
        filePanel.Visible = false;

    }

    private void imageButton_Click(object sender, EventArgs e)
    {
        string fileName;
        if (backgroudFileDialog.ShowDialog() == DialogResult.OK)
        {
            //fileName = backgroudFileDialog.FileName;

            //allSilosesComponent = Image.FromFile(fileName);
            //allSilosesComponent.BackgroundImageLayout = ImageLayout.Zoom;
        }

    }

    private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
    {

    }

    private void refreshSetting()
    {
        if (settingsService.IsAdminMode)

            newSettingPann.RefreshAll();
        else
        {
            if (simpleSettingsPan == null)
                initSimpleSetting();

            simpleSettingsPan.refreshAll();
        }
    }

    private void reapitConnect_Click(object sender, EventArgs e)
    {
        filePanel.Hide();
        try
        {
            if (!dao.connectToDB(FileProcessingService.getConnectionString(), settingsService.IsOnlyReadMode))
            {
                MessageBox.Show("Не удалось подключиться!\n" +
                    "проверьте имя пользователя и пароль");
            }
            else
            {
                MessageBox.Show("Подключение успешно!");
                silosService.synchronizeWithGettingData();
                refreshAllSettings();
                if (newSettingPann == null)
                    initSetting();

                refreshSetting();
            }
            showStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось подключиться к базе данных");
        }
    }

    private void adminMode_Click(object sender, EventArgs e)
    {

        if (settingsService.IsOnlyReadMode) //Выхоидм из режима обзор
            adminEnter();

        else
        {
            if (settingsService.IsAdminMode) // Выходим из режиема администратора
            {
                newSettingPann.Hide();
                butSil_Click(sender, e);
                //changeMode();
                changeStatus();
            }
            else
                adminEnter();
        }
    }

    private void exportToExcelTime_Click(object sender, EventArgs e)
    {
        filePanel.Hide();
        loggingRichTextBox.Height -= exportProgressBar.Height;

        if (exportForm == null)
        {
            exportForm = new ExportToExcelForm(silosService, settingsService, exportEndEvent, progressBarWorking);

        }
        exportForm.ShowDialog();
    }

    private void exportToExcel_Click(object sender, EventArgs e)
    {
        filePanel.Hide();
        if (saveExcelFileDialog.ShowDialog() == DialogResult.OK)
        {
            exportProgressBar.Visible = true;
            if (saveExcelFileDialog.FileName != "")
            {
                if (ExportService.exportToExcelLastTemperatureAsync(saveExcelFileDialog.FileName, silosService, settingsService, exportEndEvent, progressBarWorking))
                    addMessageToTextBoxLoger("Выгрузка началась!", Color.Black);
                else
                    MessageBox.Show("Выгрузка в процессе!");
                /*
                if (!ExportService.exportToExcel(saveExcelFileDialog.FileName,
                    silosService, settingsService))
                {
                    MessageBox.Show("Не удалось сделать выгрузку в Файл. \n" +
                        "Возможно он открыт в другой программе");
                    addMessageToTextBoxLoger("Неудалось сделать выгрузку!", Color.Red);
                }
                else
                {
                    addMessageToTextBoxLoger("Выгрузка сделана успешно!", Color.Green);
                    MessageBox.Show("Выгрузка сделана успешно!");
                }*/
            }
        }

    }

    private void simpleSet_Click(object sender, EventArgs e)
    {
        menuSetting.Hide();
        if (settingsService.IsBlockUserSettingsAndObservWithPSWD)
            if (EnterPasswordForm.enterOperator() != DialogResult.Yes)
                return;

        if (observer.IsRunning)
        {
            DialogResult dialogResult = MessageBox.Show("Остановить опрос и открыть настройки?",
                "Остановка опроса", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                butRunStop_Click(sender, e);
                stopObserv();
                observationStatusToolStripStatusLabel.Text = "Опрос остановлен";

            }
        }
        claerWorkPan();
        if (newSettingPann != null)
            newSettingPann.Hide();

        initSimpleSetting();
        settingPan.Show();
        simpleSettingsPan.Show();
        settingPan.Dock = DockStyle.Fill;
        simpleSettingsPan.Parent = settingPan;
        simpleSettingsPan.Dock = DockStyle.Fill;
        //simpleSettingsForm.ShowDialog();
        menuSetting.Hide();
        flagSetting = false;
    }

    public void changeStatus()
    {



        changeMode();
        showStatus();

    }

    private void adminSet_Click(object sender, EventArgs e)
    {
        menuSetting.Hide();
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
            DialogResult dialogResult = MessageBox.Show("Остановить опрос и открыть настройки?",
                "Остановка опроса", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                butRunStop_Click(sender, e);
                stopObserv();
                Thread.Sleep(300);
                observationStatusToolStripStatusLabel.Text = "Опрос остановлен";
            }
        }
        if (!observer.IsRunning)
        {
            isSettingWindowOpen = true;
            try
            {
                claerWorkPan();
                if (simpleSettingsPan != null)
                    simpleSettingsPan.Hide();
                initAdminSetting();
                newSettingPann.Show();
                settingPan.Show();
                newSettingPann.Parent = settingPan;
                settingPan.Dock = DockStyle.Fill;
                newSettingPann.Dock = DockStyle.Fill;
            }
            catch (Exception ex)
            {
                MyLoger.LogError(ex.Message);
                Application.Restart();
            }

        }
        menuSetting.Hide();
        flagSetting = false;
    }

    private void adminEnter()
    {
        EnterPasswordForm enterPasswordForm = new EnterPasswordForm(settingsService, changeStatus);
        enterPasswordForm.ShowDialog();
    }




    private void redTimer_Tick(object sender, EventArgs e)
    {
        if (exportProgressBar.Value != 100)
        {
            exportProgressBar.Value += 10;
            TaskbarProgress.SetValue(hwnd1, exportProgressBar.Value, 100);
        }
        else
        {
            redTimer.Stop();
            TaskbarProgress.Reset(hwnd1);
        }
    }
}
