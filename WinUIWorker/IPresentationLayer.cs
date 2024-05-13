using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public interface IPresentationLayer
{
    /// <summary>
    /// Открытие окна подключения к БД
    /// </summary>
    public void openConnectDBDialog(ref Dao dao);

    /// <summary>
    /// 
    /// </summary>
    public void closeConnectDB();


    /// <summary>
    /// Открытие окна при загрузки приложения 
    /// </summary>
    public void showWindowDownload(bool flag);

    /// <summary>
    /// Вызов настроек 
    /// </summary>
    /// <param name="settingsService"></param>
    public void callSettingComponent(SettingsService settingsService, bool adminSetting);

    /// <summary>
    /// Установка режим администратора
    /// </summary>
    /// <returns>true - смена успешная, false - смена неудачная</returns>
    public bool setAdminMode();

    /// <summary>
    /// Установка автономного режима
    /// </summary>
    /// <returns></returns>
    public bool setOfflineMode();

    /// <summary>
    /// Отключение режима администратора
    /// </summary>
    /// <returns></returns>
    public bool setNormalMode();

    /// <summary>
    /// установка модификации с подключением к БД
    /// </summary>
    /// <returns></returns>
    public bool setConnectDB_Mode();

    /// <summary>
    /// Запуск процесса опроса плат
    /// </summary>
    public void runObservMode();

    /// <summary>
    /// Остановка процесса опроса плат
    /// </summary>
    public void stopObservMode();

    /// <summary>
    /// Вызывает окно с вопросом остановки опроса датчиков
    /// </summary>
    /// <returns>true можно остановить опрос, false не останавливать</returns>
    public bool stopObserv();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void callMessageBox(string message);

    /// <summary>
    /// Перезагрузить все компоненты
    /// </summary>
    public void refreshALL();


    /// <summary>
    /// Перезагрузить компонент всех силосов
    /// </summary>
    public void refreshAllSilos();

    /// <summary>
    /// отправка значение progress bar 
    /// -1 активация прогресс бара
    /// -2 ошибка выполнения задачи
    /// 100 успешное выполнение задачи запуск таймера на исчезновение прогресс бара
    /// другое значение - прогресс 
    /// </summary>
    public void setProgressBar(int value);

    public void setStatus(string message);

    /// <summary>
    /// закрытие настроек при старте опроса
    /// </summary>
    /// <returns>возвращает закрыли (true) ли настройки или нет (false)</returns>
    public bool closeSetting();

    /// <summary>
    /// Вызов окна с предупреждением о перегреве датчиков
    /// </summary>
    public void overheatMessageBox();

    
}