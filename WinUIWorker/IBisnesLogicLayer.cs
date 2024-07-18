using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public interface IBisnesLogicLayer
{


    #region генерецая ключа 
    /// <summary>
    /// генеация ключа 
    /// </summary>
    /// <returns></returns>
    public string getSSHKey();

    /// <summary>
    /// Проверка SSH ключа на подлинность
    /// </summary>
    /// <param name="ssh"></param>
    /// <returns></returns>
    public bool checkSSHKey(string ssh);

    /// <summary>
    /// Удачная активация приложения
    /// </summary>
    public void successfulActivation();

    /// <summary>
    /// Ошибка активации приложения
    /// </summary>
    public void failedActivation();

    #endregion

    #region подкулючение к БД
    /// <summary>
    /// Завершение работы программы
    /// </summary>
    public void CustomClose();

    /// <summary>
    /// Запуск программы в режиме офлайн режима
    /// </summary>
    public void successStartOfflineMode();

    /// <summary>
    /// Запуск программы с подключение к БД
    /// </summary>
    public void successStartWithDB();
    #endregion

    #region MainWindow
    /// <summary>
    /// Смена и вывод температуры в силосах в определенное время
    /// </summary>
    public void changeTemperature(DateTime time);

    /// <summary>
    /// Получить 
    /// </summary>
    /// <returns>Возвращает коллекцию где ключ это id силосаб а значение процентр заполнения зерном</returns>
    public Dictionary<int, string[]> getFillingChart();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="silosId"></param>
    /// <returns></returns>
    public float[][] getDataGridTemp(int silosId);


    /// <summary>
    /// получение информации о минимальных максимальных и среднихтемпературах
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, Silos> getTempMaxMidMin();

    /// <summary>
    /// Попытка смены изображения
    /// </summary>
    /// <returns></returns>
    public bool attemptChangeImageBackGround();

    /// <summary>
    /// Попытка перетащить силос на панеле силосов
    /// </summary>
    /// <returns></returns>
    public bool attemptDragSilos();

    /// <summary>
    /// получение уровня заполненности 
    /// </summary>
    /// <returns></returns>
    public int getFilling(int silosId, DateTime time);

    #endregion

    #region Построение графиков
    /// <summary>
    /// Метод построение графика температур для дня, недели, месяца, года
    /// </summary>
    /// <param name="modeTime">режим графика 
    /// 1 - за день
    /// 2 - за неделю
    /// 3 - за месяц
    /// 4 - за год</param>
    /// <param name="list">Лист подвесок</param>
    /// <returns></returns>
    public PlotModel getChart(int modeTime, IEnumerable<Wire> list);


    /// <summary>
    /// Метод построение графика температур для произвольного отрезка времени 
    /// </summary>
    /// <param name="list">Список сиосов</param>
    /// <param name="start">Время начала</param>
    /// <param name="end">Время конца</param>
    /// <returns></returns>
    public PlotModel getChart(IEnumerable<Wire> list, DateTime start, DateTime end);

    /// <summary>
    /// Метод построение графика заполнения силосов для произвольного отрезка времени
    /// </summary>
    /// <param name="list">Лист силосов</param>
    /// <param name="start">Время начала</param>
    /// <param name="end">Время конца</param>
    /// <returns></returns>
    public PlotModel getChart(IEnumerable<Silos> list, DateTime start, DateTime end);
    #endregion

    #region СтарртСтоп опроса


    /// <summary>
    /// остановка или старт опроса
    /// </summary>
    public void runStopObserv();


    /// <summary>
    /// Остановка опроса плат
    /// </summary>
    public void stopObserv();
    #endregion

    #region Экспорт в эксель
    /// <summary>
    /// Экспорт в эксель температуры
    /// </summary>
    public void exportExcel(DateTime time, string fileName);
    #endregion

    #region Переподключение и смена режима администратора
    /// <summary>
    /// Смена режима администратора
    /// </summary>
    public void changeAdminMode();

    /// <summary>
    /// Попытка войти в режим администратора
    /// </summary>
    public bool enterAdminMode();

    /// <summary>
    /// Проверка режима администратора
    /// </summary>
    /// <returns></returns>
    public bool isAdminMode();

    /// <summary>
    /// переподключение к БД
    /// </summary>
    public void refreshConnect();
    #endregion

    #region настройки 

    /// <summary>
    /// открытие настроек
    /// </summary>
    public Task openSetting();

    /// <summary>
    /// Вычисление заполненности
    /// </summary>
    /// <param name="start">Время начала вычисления</param>
    public int calculateFilling(DateTime start);

    #region Общие настройки
    /// <summary>
    /// Получение значений размера экспортируемого графика
    /// </summary>
    /// <returns>1 значение ШИРИНА, 2 значение ВЫСОТА</returns>
    public List<int> getSizeChart();
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string getNameCompany();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool getOrientationExcelFile();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool getColorCellsExcelFile();


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool getIsHighlightWhenObservStop();
    #endregion


    #region настройка типов зерна

    /// <summary>
    /// Получение списка типов зерна
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, Grain> getGrains();

    /// <summary>
    /// Создание нового типа зерна
    /// </summary>
    /// <returns></returns>
    public Grain addGrain();


    /// <summary>
    /// обновление типа зерна
    /// </summary>
    /// <param name="g"></param>
    public void updateGrain(Grain g);

    /// <summary>
    /// Удаление типа зерна
    /// </summary>
    /// <param name="idGrain"></param>
    public void deleteGrain(Grain g);
    #endregion


    #region автономные ражимы
    /// <summary>
    /// Переход в автономный режим с загрузкой конфигурации из файла 
    /// </summary>
    /// <param name="filepath"></param>
    public void offlineMode(string filepath);

    /// <summary>
    /// Переход в автомный режим
    /// </summary>
    public void setOfflineMode();

    /// <summary>
    /// Сохранить текущую конфигурацию в файл
    /// </summary>
    /// <param name="filepath"></param>
    public void saveToFile(string filepath);

    /// <summary>
    /// Загрузить температуры из файла
    /// </summary>
    /// <param name="filepath"></param>
    public void loadTemperature(string filepath);

    #endregion


    #region структура подразделения



    /// <summary>
    /// Запрашивает список подразделений
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, string> getListDivision();

    /// <summary>
    /// Добавляет подразделение
    /// </summary>
    /// <param name="name"></param>
    public int addDivision(string name);

    /// <summary>
    /// обновление подразделения
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public void updateDivision(int id, string name);

    /// <summary>
    /// Удаление подразделения 
    /// </summary>
    /// <param name="id"></param>
    public void deleteDivision(int id);

    #endregion


    #region настройки разработчика


    public bool attemptOpenSetttingProvider();

    /// <summary>
    /// включение/отключение режима усреднения температур на сломанных датчиках
    /// </summary>
    /// <param name="flag"></param>
    public void averaginTemperatureValues();


    /// <summary>
    /// получение значения режима усреднения температуры
    /// </summary>
    /// <returns></returns>
    public bool getModeAveraginTemperatureValues();

    /// <summary>
    /// Получение пути сохранения файлов
    /// </summary>
    /// <returns></returns>
    public string getPathSaveFile();


    /// <summary>
    /// проверка смены пароля
    /// </summary>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    /// <param name="newRepeatPassword"></param>
    public void changeProviderPassword(string oldPassword, string newPassword, string newRepeatPassword);

    #endregion

    #region опрос плат
    /// <summary>
    /// сохранение настроек опроса плат
    /// </summary>
    /// <param name="newPeriod"></param>
    /// <param name="newTriesCount"></param>
    /// <param name="isGiveSecondChanse"></param>
    /// <param name="newTimeout"></param>
    public void saveObservSetting(uint newPeriod, uint newTriesCount, bool isGiveSecondChanse, uint newTimeout);

    /// <summary>
    /// смена пароля администратора
    /// </summary>
    /// <param name="oldPswd">старый пароль</param>
    /// <param name="newPswd">новый пароль</param>
    /// <param name="newPswd2">повтор нового пароля</param>
    public void saveChangeAdminPassword(string oldPswd, string newPswd, string newPswd2);

    /// <summary>
    /// принудительное вычисление заполненности силосов
    /// </summary>
    /// <param name="time">время с которого начинается вычисление</param>
    public void calculateFillingSilosesTable(DateTime time);

    /// <summary>
    /// открытие формы настроек разработчика
    /// </summary>


    public void saveStructureSubvision(List<StructureSubdivision> subdivisions);


    public void savePortSettings(string portName, int portBoundRate,
        Parity portParity, int portDataBits, StopBits portStopBits, int portTimeOut);
    #endregion

    #region управление БД

    /// <summary>
    /// Получение строки подключени к БД
    /// </summary>
    /// <returns></returns>
    public string getConnectionString();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectString"></param>
    /// <param name="user"></param>
    public void connectDBAndGetData(string connectString, string user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectString"></param>
    public void connectAndSetData(string connectString);

    /// <summary>
    /// подключение к БД
    /// </summary>
    /// <param name="server"></param>
    /// <param name="port"></param>
    /// <param name="user"></param>
    /// <param name="nameDb"></param>
    /// <param name="password"></param>
    public Task connectDB(string connectionString);

    /// <summary>
    /// удаление таблиц
    /// </summary>
    /// <param name="connectString"></param>
    public void dropDB(string connectString);

    /// <summary>
    /// удаление таблиц температур
    /// </summary>
    /// <param name="connectString"></param>
    /// <param name="time"></param>
    public void dropTemperature(string connectString, DateTime time);

    #endregion


    #region почтовая рассылка

    /// <summary>
    /// сохранение настроек стандартной почтовой рассылки
    /// </summary>
    /// <param name="mailSMTPtextBox"></param>
    /// <param name="mailSenderTextBox"></param>
    /// <param name="mailPasswordTextBox"></param>
    /// <param name="mailCaptionTextBox"></param>
    /// <param name="addPlotsCheckBox"></param>
    /// <param name="mailTimeNumericUpDown"></param>
    /// <param name="mailSEndByTimeCheckBox"></param>
    /// <param name="destinations"></param>
    public void saveMail(string mailSMTPtextBox, string mailSenderTextBox, string mailPasswordTextBox,
        string mailCaptionTextBox, bool addPlotsCheckBox, int mailTimeNumericUpDown, bool mailSEndByTimeCheckBox, string[][] destinations);

    /// <summary>
    /// сохранение настроек стандартной почтовой рассылки и отправка пробного письма
    /// </summary>
    /// <param name="mailSMTPtextBox"></param>
    /// <param name="mailSenderTextBox"></param>
    /// <param name="mailPasswordTextBox"></param>
    /// <param name="mailCaptionTextBox"></param>
    /// <param name="addPlotsCheckBox"></param>
    /// <param name="mailTimeNumericUpDown"></param>
    /// <param name="mailSEndByTimeCheckBox"></param>
    /// <param name="destinations"></param>
    public void mailSend(string mailSMTPtextBox, string mailSenderTextBox, string mailPasswordTextBox,
        string mailCaptionTextBox, bool addPlotsCheckBox, int mailTimeNumericUpDown, bool mailSEndByTimeCheckBox, string[][] destinations);

    /// <summary>
    /// сохранение настроек для отправки висем с ошибками
    /// </summary>
    /// <param name="MailErrorSmtpServer"></param>
    /// <param name="MailErrorSender"></param>
    /// <param name="MailErrorSenderPassword"></param>
    /// <param name="MailErrorCaption"></param>
    /// <param name="MailErrorSendByTimer"></param>
    /// <param name="MailErrorSendingPeriod"></param>
    /// <param name="destinations"></param>
    public void saveErrorMail(string MailErrorSmtpServer, string MailErrorSender, string MailErrorSenderPassword,
        string MailErrorCaption, bool MailErrorSendByTimer, int MailErrorSendingPeriod, string[][] destinations);

    /// <summary>
    /// сохранение настроек для отправки висем с ошибками и отправка пробного письма
    /// </summary>
    /// <param name="MailErrorSmtpServer"></param>
    /// <param name="MailErrorSender"></param>
    /// <param name="MailErrorSenderPassword"></param>
    /// <param name="MailErrorCaption"></param>
    /// <param name="MailErrorSendByTimer"></param>
    /// <param name="MailErrorSendingPeriod"></param>
    /// <param name="destinations"></param>
    public void mailErrorSend(string MailErrorSmtpServer, string MailErrorSender, string MailErrorSenderPassword,
        string MailErrorCaption, bool MailErrorSendByTimer, int MailErrorSendingPeriod, string[][] destinations);

    /// <summary>
    /// получение списка получателей рассылки
    /// </summary>
    /// <returns></returns>
    public string[][] getMailAdress();

    /// <summary>
    /// Получение списка настроек обычной почтовой рассылки
    /// </summary>
    /// <param name="MailErrorSmtpServer"></param>
    /// <param name="MailErrorSender"></param>
    /// <param name="MailErrorSenderPassword"></param>
    /// <param name="MailErrorCaption"></param>
    /// <param name="MailErrorSendByTimer"></param>
    /// <param name="MailErrorSendingPeriod"></param>
    public void getSettingMail(out string mailSMTPtextBox, out string mailSenderTextBox, out string mailPasswordTextBox,
        out string mailCaptionTextBox, out bool addPlotsCheckBox, out int mailTimeNumericUpDown, out bool mailSEndByTimeCheckBox);

    /// <summary>
    /// получение списка получателей рассылки ошибок
    /// </summary>
    /// <returns></returns>
    public string[][] getErrorMailAdress();

    public void getSettingErrorMail(out string MailErrorSmtpServer, out string MailErrorSender, out string MailErrorSenderPassword,
        out string MailErrorCaption, out bool MailErrorSendByTimer, out int MailErrorSendingPeriod);


    #endregion


    #endregion
}
