using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SystemOfTermometry2.Services;
using SystemOfThermometry2.Services;

namespace SystemOfTermometry2.WinUIWorker;
internal interface IPresentationLayer
{
    /// <summary>
    /// Вызов информации об одном силосе
    /// </summary>
    public void callOneSilosComponent(SilosService silosService);

    /// <summary>
    /// Вызов информации по всем силосам
    /// </summary>
    /// <param name="silosService"></param>
    public void callAllSilosComponent(SilosService silosService);

    /// <summary>
    /// Вызов настроек 
    /// </summary>
    /// <param name="settingsService"></param>
    public void callSettingComponent(SettingsService settingsService);

    /// <summary>
    /// Вызов графика запоненности силосов
    /// </summary>
    public void callFillingChartMode();

    /// <summary>
    /// Вызов окна графиков функции
    /// </summary>
    public void callChartMode(PlotModel model);

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


      
}