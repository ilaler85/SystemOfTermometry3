using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Services;
using System.Drawing;

namespace SystemOfThermometry3.WinUIWorker;

public delegate bool modeCheckPassword(string password);

public interface IPresentationLayer 
{
    /// <summary>
    /// Открытие окна подключения к БД
    /// </summary>
    public Task<string> openFormConnectDBDialog();

    public Task<bool> openFormConnectDBDialog2();

    /// <summary>
    /// закрытие формы подключения к бд
    /// </summary>
    public void closeFormConnectDB();


    /// <summary>
    /// Инициализация Главной формы
    /// </summary>
    public void startMainForm();

    public void setIBLL(IBisnesLogicLayer bll);
    /// <summary>
    /// Открытие окна при загрузки приложения 
    /// </summary>
    public void showWindowDownload(bool flag);

    public void closeWindowDownload();

    /// <summary>
    /// Вызов настроек 
    /// </summary>
    /// <param name="settingsService"></param>
    public void callSettingComponent();

    /// <summary>
    /// Метод закрытия настроек администратора при нажатии на выход из режима администратора
    /// </summary>
    public void closeAdminSetting();

    /// <summary>
    /// Установка режим администратора
    /// </summary>
    /// <returns>true - смена успешная, false - смена неудачная</returns>
    public bool setAdminMode();

    /// <summary>
    /// Открытие окна проверки пароля 
    /// Передается метод для проверки пароля зависит от запроса
    /// </summary>
    public bool openEnterForm(modeCheckPassword modeCheckPassword);

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
    /// открваем обычные настройки
    /// </summary>
    public void openNormalSetting();

    /// <summary>
    /// открываем настройки Администратора
    /// </summary>
    public Task openAdminSetting();

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
    public Task<bool> stopObserv(); 

    /// <summary>
    /// Установка темы приложения при остановке опроса платы
    /// </summary>
    public void setStopStyleForm();

    /// <summary>
    /// Установка темы приложения при запуске опроса платы
    /// </summary>
    public void setNormalStyleForm();

    /// <summary>
    /// Вы
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<bool> askDialogShow(string message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public Task callMessageBox(string message);

    /// <summary>
    /// Сообщение в лог панели 
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="color">Цвет текста</param>
    public void sendLogMessage(string message, Color color);

    /// <summary>
    /// Перезагрузить все компоненты
    /// </summary>
    public void refreshALL();


    /// <summary>
    /// Перезагрузить компонент всех силосов
    /// </summary>
    public void refreshAllSilos();

    /// <summary>
    /// <para>отправка значение progress bar </para> 
    /// <para>-1 активация прогресс бара</para>
    /// <para>-2 ошибка выполнения задачи</para>
    /// <para>100 успешное выполнение задачи запуск таймера на исчезновение прогресс бара </para>
    /// <para>другое значение - прогресс </para>
    /// </summary>
    public void setProgressBar(int value);

    /// <summary>
    /// установка статуса приложения
    /// </summary>
    /// <param name="message"></param>
    public void setStatus(string message);

    /// <summary>
    /// закрытие настроек при старте опроса
    /// </summary>
    /// <returns>возвращает закрыли (true) ли настройки или нет (false)</returns>
    public void closeSetting();

    /// <summary>
    /// Вызов окна с предупреждением о перегреве датчиков
    /// </summary>
    public void overheatMessageBox(StringBuilder message, bool playSound);

    /// <summary>
    /// Открытие форму генерации ключа
    /// </summary>
    public void openFormApplyKeyForm();

    /// <summary>
    /// Запрос на SSH ключ от формы
    /// </summary>
    /// <returns></returns>
    public string returnKeyApplyKeyForm();


    /// <summary>
    /// обновление настроек 
    /// </summary>
    public void refreshSetting();

    /// <summary>
    /// обновление панели управления и настроек силосов
    /// </summary>
    public void refreshSilosTabControl();

    public void refreshAllSilosTemperatur();
}