using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public interface IBisnesLogicLayer
{
    
    public SilosService getAllSilos();

    public GrainService getGrainService();

    /// <summary>
    /// 
    /// </summary>
    public void runObserv();


    public void CustomClose();

    public void successStartOfflineMode();

    public void successStartWithDB();
    /// <summary>
    /// 
    /// </summary>
    public void stopObserv();

    /// <summary>
    /// 
    /// </summary>
    public void exportExcel(DateTime time);

    public void getFilling();
    /// <summary>
    /// Вычисление заполненности
    /// </summary>
    /// <param name="start">Время начала вычисления</param>
    public void calculateFilling(DateTime start);



    /// <summary>
    /// Попытка войти в режим администратора
    /// </summary>
    public bool enterAdminMode();


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


    /// <summary>
    /// Смена и вывод температуры в силосах в определенное время
    /// </summary>
    public void changeTemperature(DateTime time);
}
