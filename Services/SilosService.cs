using MySqlX.XDevAPI.Common;
using NuGet;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.Properties;
using static SystemOfThermometry3.Services.GrainService;

namespace SystemOfThermometry3.Services;

/// <summary>
/// Класс-посредник между пользовательским интерфейсом и логикой и базой данных
/// Отвечает за конфигурацию силосов и подвесок.
/// А также дает дополнительную информаци по поводу силосов
/// </summary>
public class SilosService
{
    //data access object - интерфейс, через который класс общается с базой данных        
    private DAO.Dao dao;
    private SettingsService settingsService; // Cервис для получения настроек
    private GrainService grainService;
    private Dictionary<int, Silos> siloses; // Архетектура силосов. Ключ - id силоса
    private Dictionary<int, Dictionary<int, Wire>> devices; // Архитектура плат и подвесок. Первый ключ - адрес платы.
                                                            // Значение - словарь с подвесками, где ключ - id подвески.
                                                            //private bool settingsService.OfflineMode = false; // Режим, в котором база данных не обязательна,
                                                            // конфигурация и считываемые данные будут сохраняться в файл

    private Dictionary<int, float[]> lastTemperatures; //Словарь с последними снятыми температурами.
                                                       // Ключ - id подвески
    private DateTime lastTempTime = DateTime.MinValue; //Время когда были получены температуры.
    public DateTime LastTempTime
    {
        get
        {
            return lastTempTime;
        }
    }
    public GrainService GrainService
    {
        get
        {
            return grainService;
        }
    }

    private Dictionary<int, StructureSubdivision> subdivisions;

    public delegate void ProgressDelegate(int progress);
    private ProgressDelegate progressDelegate;


    private static SilosService instance;
    /// <summary>
    /// Конструктор, который вызывается в случае корректной работы базы данных.
    /// Принимает в качестве аргумента реализацию интерфейса DAO
    /// Таким образом на зависит от конкретной базы данных
    /// </summary>
    /// <param name="dao">реализацию интерфейса DAO</param>
    /// <param name="settingsService">сервис для работы с настройками</param>
    /// <param name="grainService">Сервис для работы с культурами</param>
    /// 
    public SilosService(DAO.Dao dao, SettingsService settingsService, GrainService grainService, ProgressDelegate progressDelegate)
    {
        this.dao = dao ?? throw new ArgumentNullException("dao is null");
        this.settingsService = settingsService ?? throw new ArgumentException("setting service is null");
        this.grainService = grainService ?? throw new ArgumentNullException("grain service is null");
        this.grainService.updateGrain += updateGrains;
        this.progressDelegate = progressDelegate ?? throw new ArgumentNullException("error progress bar");
        if (grainService != null)
            this.grainService = grainService;

        refreshAllSilosAndDevise();
        refreshGraind();

        lastTemperatures = new Dictionary<int, float[]>();

        //Заполняем последние значения температур.
        refreshLastTemperatures();
        /*
        foreach (var dictEntry in devices)
        {
            foreach (var wireEntry in dictEntry.Value)
            {
                DateTime newLastTempTime = new DateTime();
                Wire wire = wireEntry.Value;
                float[] temperatures = this.dao.getLastTempForWire(wire, ref newLastTempTime);
                if (temperatures == null)
                {
                    lastTemperatures.Add(wire.Id, new float[0]);
                }
                else
                {
                    lastTemperatures.Add(wire.Id, temperatures);
                    lastTempTime = newLastTempTime > lastTempTime ? newLastTempTime : lastTempTime;
                }
            }
        }
        */
        logAll("SilosS: start");
        this.grainService = grainService;
    }

    /// <summary>
    /// Конструктор, вызываемый в случае, если база данных не работает.
    /// </summary>
    /// <param name="siloses">Конфигурация силосов</param>
    /// <param name="devices">Конфигурация плат и подвесок</param>
    /// <param name="settingsService">сервис для работы с настройками</param>

    public SilosService(DAO.Dao dao, SettingsService settingsService, Dictionary<int, Silos> siloses, Dictionary<int, Dictionary<int, Wire>> devices)
    {
        this.siloses = siloses ?? throw new ArgumentNullException("Siloses is null");
        this.devices = devices ?? throw new ArgumentNullException("Devices is null");

        //Заполняем последние значения температур.
        foreach (var dictEntry in devices)
        {
            foreach (var wireEntry in dictEntry.Value)
                lastTemperatures.Add(wireEntry.Value.Id, new float[0]);
        }

        settingsService.OfflineMode = true;
    }


    #region получение данных из БД

    public void reInitService(Dictionary<int, Silos> siloses, Dictionary<int, Dictionary<int, Wire>> devices, Dictionary<int, StructureSubdivision> subdivs)
    {
        this.siloses = siloses ?? throw new ArgumentNullException("Siloses is null");
        this.devices = devices ?? throw new ArgumentNullException("Devices is null");
        subdivisions = subdivs ?? throw new ArgumentNullException("Subdivs is null");

        lastTemperatures.Clear();
        foreach (var dictEntry in devices)
        {
            foreach (var wireEntry in dictEntry.Value)
                lastTemperatures.Add(wireEntry.Value.Id, new float[0]);
        }
        logAll("SilosS reinit");
    }

    private void logAll(string str)
    {
        foreach (var s in siloses.Values)
        {
            str += "\nSilos " + s.Id + " " + s.Name;
            foreach (var w in s.Wires.Values)
            {
                str += "\n__" + w.ToString();
            }
        }

        str += "\nSubdivisions:";
        foreach (var subdivision in subdivisions.Values)
        {
            str += "\n" + subdivision.Id + " " + subdivision.Name;
            //subdivisionsHoles.Remove(subdivision.Id);
        }

        MyLoger.Log(str);
    }


    private void updateGrains()
    {
        refreshGraind();
    }

    public void refreshGraind()
    {
        var graines = grainService.getGrains();
        foreach (var s in siloses.Values)
        {
            var grainId = s.GrainId;
            if (grainId != -1)
            {
                if (graines.ContainsKey(grainId))
                {
                    s.Red = graines[grainId].RedTemp;
                    s.Yellow = graines[grainId].YellowTemp;
                    s.GrainName = graines[grainId].Name;
                    updateSilos(s);
                }
            }

        }

    }

    /// <summary>
    /// Заполняет конфигурацию силосов и подвесок из базы данных.
    /// </summary>
    /// <param name="siloses">Силосы</param>
    /// <param name="devices">Платы</param>        
    public void refreshAllSilosAndDevise()//ref Dictionary<int, Silos> siloses, ref Dictionary<int, Dictionary<int, Wire>> devices)
    {
        devices = new Dictionary<int, Dictionary<int, Wire>>();
        siloses = dao.getAllSilos();
        subdivisions = dao.getAllSubdivisions();
        if (subdivisions == null)
            subdivisions = new Dictionary<int, StructureSubdivision>();

        if (siloses == null)
            siloses = new Dictionary<int, Silos>();

        var wires = dao.getAllWires();
        if (wires == null)
            wires = new Dictionary<int, Wire>();

        //Формируем конфигурацию плат и подвесок.
        foreach (var entry in wires)
        {
            var w = entry.Value;
            if (!devices.ContainsKey(w.DeviceAddress))
                devices.Add(w.DeviceAddress, new Dictionary<int, Wire>());

            devices[w.DeviceAddress].Add(w.Id, w);
        }

        // Заполняем силосы подвесками
        foreach (var deviceEntry in devices)
        {
            wires = deviceEntry.Value;
            foreach (var entry in wires)
            {
                var wire = entry.Value;
                if (siloses.ContainsKey(wire.SilosId))
                    siloses[wire.SilosId].Wires.Add(wire.Id, wire);
            }
        }
    }

    /// <summary>
    /// Обновляет значения температур в конкретный момент времени
    /// </summary>
    /// <param name=""></param>
    public void refreshTemperature(DateTime dateTime)
    {
        DateTime specificTime;
        specificTime = dao.getTime(ref dateTime);
        lastTempTime = specificTime;
        foreach (var dictEntry in devices)
        {
            foreach (var wireEntry in dictEntry.Value)
            {

                var wire = wireEntry.Value;
                var temperatures = dao.getUpperBoundTempTempForWire(wire, ref specificTime);
                if (temperatures == null)
                {
                    if (!lastTemperatures.ContainsKey(wire.Id))
                        lastTemperatures.Add(wire.Id, new float[0]);
                    else
                        lastTemperatures[wire.Id] = new float[0];
                }
                else
                {
                    if (!lastTemperatures.ContainsKey(wire.Id))
                        lastTemperatures.Add(wire.Id, temperatures);
                    else
                        lastTemperatures[wire.Id] = temperatures;
                    //lastTempTime = specificTime > lastTempTime ? specificTime : lastTempTime;
                }
            }
        }

        foreach (var s in siloses.Values)
        {
            s.refreshTemperature(lastTemperatures);
        }
    }

    /// <summary>
    /// Обновляет значения температур.
    /// </summary>
    public void refreshLastTemperatures()
    {
        foreach (var dictEntry in devices)
        {
            foreach (var wireEntry in dictEntry.Value)
            {
                var newLastTempTime = new DateTime();
                var wire = wireEntry.Value;
                var temperatures = dao.getLastTempForWire(wire, ref newLastTempTime);
                if (temperatures == null)
                {
                    if (!lastTemperatures.ContainsKey(wire.Id))
                        lastTemperatures.Add(wire.Id, new float[0]);
                    else
                        lastTemperatures[wire.Id] = new float[0];
                }
                else
                {
                    if (!lastTemperatures.ContainsKey(wire.Id))
                        lastTemperatures.Add(wire.Id, temperatures);
                    else
                        lastTemperatures[wire.Id] = temperatures;
                    lastTempTime = newLastTempTime > lastTempTime ? newLastTempTime : lastTempTime;
                }
            }
        }

        foreach (var s in siloses.Values)
        {
            s.refreshTemperature(lastTemperatures);
        }


    }

    /// <summary>
    /// Синхронизируется с базой данных, путем загрузки конфигурации из базы данных
    /// </summary>
    public void synchronizeWithGettingData()
    {
        settingsService.OfflineMode = false;
        settingsService.synchronizeWithGettingData();
        refreshAllSilosAndDevise();
    }

    /// <summary>
    /// Синхронизация с базой данных, путем сохранения текущей конфигурации в базу данных
    /// </summary>
    public void synchronizeWithSettingData()
    {
        dao.dropAndCreateModel();
        settingsService.synchronizeWithSettingData();

        foreach (var s in siloses.Values)
        {
            dao.addSilos(s);
        }

        foreach (var dict in devices.Values)
        {
            foreach (var w in dict.Values)
                dao.addWire(w);
        }

        foreach (var subdiv in subdivisions.Values)
        {
            dao.addSubdivision(subdiv);
        }
        //добавить загрузку культур зерна

        /* old
        settingsService.OfflineMode = false;
        settingsService.synchronizeWithSettingData();
        Dictionary<int, Silos> silosesInDB = dao.getAllSilos(); //Силосы, которые уже есть в бд.
        HashSet<int> syncronizedSiloses = new HashSet<int>(); //Силосы, которые удалось синхронизировать
        if (silosesInDB != null && silosesInDB.Count != 0)
        {
            foreach (Silos s in silosesInDB.Values.ToArray()) //
            {
                if (siloses.ContainsKey(s.Id)) //Такой силос уже есть, обновляем его
                {
                    updateSilos(siloses[s.Id]);
                    syncronizedSiloses.Add(s.Id);
                }
                else // Такого силоса у нас нет, значит, его надо удалить из базы данных
                {
                    deleteSilos(s);
                }
            }
        }

        //теперь добавляем в базу данных все силосы, которые у не синхронизировались
        //при этом их ИД будет изменен
        foreach (int silosIdx in siloses.Keys.ToArray())
        {
            Silos s = siloses[silosIdx];
            if (!syncronizedSiloses.Contains(s.Id))
            {
                int oldId = s.Id;
                siloses.Remove(s.Id);
                addSilos(ref s);

                //Переделываем все ножки, у которых был старый ID Силоса.
                foreach (var dict in devices.Values)
                    foreach (Wire w in dict.Values)
                        if (w.SilosId == oldId)
                            w.SilosId = s.Id;
            }
        }

        //Теперь проделываем то же самое с подвесками
        Dictionary<int, Wire> wiresInDB = dao.getAllWires();
        HashSet<int> syncronizedWires = new HashSet<int>(); //Подвески, которые удалось синхронизировать
        if (wiresInDB != null && wiresInDB.Count != 0)
        {
            foreach (Wire w in wiresInDB.Values.ToArray())
            {
                bool contains = false; // переменная, которая указывает на то, что такая подвеска существует
                foreach (var dict in devices.Values)
                {
                    if (dict.ContainsKey(w.Id))
                    {
                        contains = true;
                        updateWire(dict[w.Id]);//обновляем до текущего состояния
                        syncronizedWires.Add(w.Id);
                    }
                }
                if (!contains)
                    deleteWire(w); //Не нашли - удаляем.
            }
        }

        //Теперь добавляем все не синхронизированные подвески. ид подвесок будет изменен.
        foreach (var dict in devices.Values)
        {
            foreach (int wireIdx in dict.Keys.ToArray())
            {
                Wire w = dict[wireIdx];
                if (!syncronizedWires.Contains(w.Id))
                {
                    dict.Remove(w.Id);
                    addWireToDevice(ref w);
                }
            }
        }



        Dictionary<int, StructureSubdivision> subdivInDB = dao.getAllSubdivisions(); //подразделения, которые уже есть в бд.
        HashSet<int> syncronizedSubdivs = new HashSet<int>(); //подразделения, которые удалось синхронизировать
        if (subdivInDB != null && subdivInDB.Count != 0)
        {
            foreach (StructureSubdivision s in subdivInDB.Values.ToArray()) //
            {
                if (subdivisions.ContainsKey(s.Id)) //Такой силос уже есть, обновляем его
                {
                    updateSubdivisions(subdivisions[s.Id]);
                    syncronizedSubdivs.Add(s.Id);
                }
                else // Такого силоса у нас нет, значит, его надо удалить из базы данных
                {
                    deleteSubdivision(s);
                }
            }
        }

        //теперь добавляем в базу данных все подразделения, которые не синхронизировались
        //при этом их ИД будет изменен
        foreach (int subdivId in subdivisions.Keys.ToArray())
        {
            StructureSubdivision s = subdivisions[subdivId];
            if (!syncronizedSubdivs.Contains(s.Id))
            {
                int oldId = s.Id;
                subdivisions.Remove(s.Id);
                addSubdivision(ref s);
            }
        }
        /**/

    }



    #endregion

    #region принудительное заполнение таблицы заполненности силосов

    public void threadFilling(object parameters)
    {
        var tuple1 = (Tuple<DateTime>)parameters;
        var withDate = tuple1.Item1;
        //List<DateTime> listTimes = dao.getSetTimeBetweenTime(withDate, DateTime.Now);
        var listTimeFromHystory = dao.getTimesBetweenFromFillingTable(withDate);
        /*Dictionary<int, List<Wire>> silosesWire = new Dictionary<int, List<Wire>>();

        foreach (Silos item in siloses.Values)
        {
            silosesWire.Add(item.Id, item.getEnabledWires());
        }*/
        var silosDevice = new Dictionary<int, List<int>>();
        foreach (var idDevice in devices.Keys)
        {
            if (devices[idDevice].Count == 0)
                continue;
            var w = devices[idDevice].First().Value;

            if (!silosDevice.ContainsKey(w.SilosId))
                silosDevice.Add(w.SilosId, new List<int>());

            if (!silosDevice[w.SilosId].Contains(idDevice))
                silosDevice[w.SilosId].Add(idDevice);
        }
        progressDelegate?.Invoke(-1);
        try
        {

            //double progress = 100.0 / (double)listTimes.Count;
            var progress = 100.0 / ((DateTime.Now - withDate).TotalDays - listTimeFromHystory.Count);
            var i = 1;
            var dates = new List<DateTime>();
            for (var dt = new DateTime(withDate.Year, withDate.Month, withDate.Day); dt <= DateTime.Now; dt = dt.AddDays(1))
            {
                dates.Add(dt);
            }


            Parallel.ForEach(dates, dt =>
            //for(DateTime dt = new DateTime(withDate.Year, withDate.Month, withDate.Day); dt <= DateTime.Now;dt = dt.AddDays(1))
            {
                if (!listTimeFromHystory.Contains(dt))
                {
                    progressDelegate?.Invoke(Convert.ToInt32(Math.Round(i * progress, 0)));
                    addFilling(dt, silosDevice);
                    //addFilling(dt, silosesWire);
                    ++i;
                }

            });

            /*foreach (DateTime time in listTimes)
             {

                 progressDelegate?.Invoke(Convert.ToInt32(Math.Round(i * progress, 0)));
                 if (!listTimeFromHystory.Contains(time))
                 {
                     addFilling(time, silosesWire);
                 }
                 ++i;
             }*/
            MessageBox.Show("Вычисление заполненности законченно");
            progressDelegate?.Invoke(100);
        }
        catch
        {
            MessageBox.Show("Ошибка заполнения данных по загрзке");
            progressDelegate?.Invoke(-2);
        }
    }

    /// <summary>
    /// метод заполнения 
    /// </summary>
    /// <param name="withDate"></param>
    /// <returns></returns>
    /// 
    public void fillingInSilosTable(DateTime withDate)
    {
        var thread = new Thread(threadFilling);
        thread.Start(new Tuple<DateTime>(withDate));


    }

    /* private List<DateTime> getTimesCalculateFilling(DateTime time, int silosId)
     {
         List<DateTime> result = new List<DateTime>();
         List<DateTime> times = dao.getSetTimeBetweenTime(Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 0:00:00"), Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 23:59:59"), silosId);
         if (!result.Any()) return result;

         if(time.Month>=3 && time.Month<=5 )

         if(time.Month)

         return result;
     }*/

    /// <summary>
    /// получает набор DateTime, по которым можем провести наиболее точное вычисление заполненности
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private List<DateTime> getTimesCalculateFilling(DateTime time)
    {
        var result = new List<DateTime>();
        result = dao.getSetTimeBetweenTime(Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 0:00:01"), Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 6:00:00"));

        if (!result.Any())
        {
            result = dao.getSetTimeBetweenTime(Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 06:00:00"), Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 9:00:00"));
            var tmp = dao.getSetTimeBetweenTime(Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 18:00:00"), Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 23:59:00"));
            result.AddRange(tmp);
        }

        if (!result.Any())
        {
            result = dao.getSetTimeBetweenTime(Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 09:00:00"), Convert.ToDateTime(time.ToString("yyy-MM-dd") + " 18:00:00"));
        }

        return result;
    }


    /// <summary>
    /// вычисляет уровень заполнения в текущем силосе используя другой метод вычисления
    /// </summary>
    /// <param name="sil"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    private int getFillingSilosDay(List<int> device, List<DateTime> times)
    {
        var result = new List<int>();
        var res = 0;
        foreach (var t in times)
        {
            var tempAVG = dao.getAVGSilosTemp(device, t);
            result.Add(HistoryFillingService.getFilling(tempAVG));
        }
        foreach (var item in result)
        {
            if (item > 0 && item != 100)
            {
                res = item;
                continue;
            }
            if (item == 100 && res == 0)
                res = 100;
        }
        return res;
    }

    /// <summary>
    /// вычисляет уровень заполнения в текущем силосе
    /// </summary>
    /// <param name="sil"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    private int getFillingSilosDay(List<Wire> wires, List<DateTime> times)
    {
        var result = new List<int>();

        foreach (var t in times)
        {

            var silosId = wires[1].SilosId;
            var tmp = new Dictionary<int, Wire>();
            var tempSilos = new Dictionary<int, float[]>();
            var time = t;
            foreach (var w in wires)
            {
                if (w != null)
                {
                    tmp.Add(w.Id, w);
                    var tempWire = dao.getUpperBoundTempTempForWire(w, ref time);
                    if (tempWire != null)
                        tempSilos.Add(w.Id, tempWire);
                }

            }
            result.Add(HistoryFillingService.getFilling(tempSilos, tmp));

        }
        var res = 0;

        foreach (var item in result)
        {
            if (item != 0 && item != 100)
            {
                res = item;
                continue;
            }
            if (item == 100 && res == 0)
                res = 100;
        }
        return res;
    }
    /// <summary>
    /// принудительное вычисление заполнения в определенный момент времени
    /// </summary>
    /// <param name="time"></param>
    /// <param name="silosDevice"></param>
    private void addFilling(DateTime time, Dictionary<int, List<int>> silosDevice)
    {
        var silosFilling = new Dictionary<int, int>();
        var timeCalculateFilling = new List<DateTime>();
        timeCalculateFilling = getTimesCalculateFilling(time);
        if (timeCalculateFilling == null) return;

        if (timeCalculateFilling.Count == 0)
            return;
        foreach (var sil in siloses.Values)
        {
            if (sil.getEnabledWireCount() != 0)
                silosFilling.Add(sil.Id, getFillingSilosDay(silosDevice[sil.Id], timeCalculateFilling));

        }
        //MessageBox.Show(silosFilling.Count.ToString());
        dao.addMoreHistory(time, silosFilling);
    }

    private void addFilling(DateTime time, Dictionary<int, List<Wire>> siloses)
    {
        var silosFilling = new Dictionary<int, int>();
        var timeCalculateFilling = new List<DateTime>();
        timeCalculateFilling = getTimesCalculateFilling(time);
        if (timeCalculateFilling == null) return;

        if (timeCalculateFilling.Count == 0)
            return;
        Parallel.ForEach(siloses, sil =>
        {
            if (sil.Value.Count != 0)
                silosFilling.Add(sil.Key, getFillingSilosDay(sil.Value, timeCalculateFilling));

        });
        //MessageBox.Show(silosFilling.Count.ToString());
        dao.addMoreHistory(time, silosFilling);
    }
    /*private void addFilling(DateTime time, Dictionary<int, List<Wire>> siloses)
    {
        Dictionary<int, int> silosFilling = new Dictionary<int, int>();

        Dictionary<int, Dictionary<int, float[]>> temperature = new Dictionary<int, Dictionary<int, float[]>>();

        foreach (List<Wire> wires in siloses.Values)//идем по силусам 
        {
            if(wires.Count == 0) continue;
            int silosId = wires[1].SilosId;
            Dictionary<int, Wire> tmp = new Dictionary<int, Wire>();
            Dictionary<int, float[]> tempSilos = new Dictionary<int, float[]>();
            Parallel.ForEach(wires, w => 
            {
                //foreach (Wire w in wires) {
                if (w != null)
                {
                    tmp.Add(w.Id, w);
                    float[] tempWire = dao.getUpperBoundTempTempForWire(w, ref time);
                    if (tempWire != null)
                        tempSilos.Add(w.Id, tempWire);
                }

            });
            silosFilling.Add(silosId, HistoryFillingService.getFilling(tempSilos, tmp));
        }

        dao.addMoreHistory(time, silosFilling);
        //getUpperBoundTempTempForWire
        
    }*/
    #endregion

    #region Температуры.
    /// <summary>
    /// Возвращает фактические последние значения температур
    /// </summary>
    /// <param name="wire">подвеска</param>
    /// <returns>значения температур, не возвращает null, если нет данных, возвращает пустой массив</returns>
    public float[] getLastTempForWire(Wire wire)
    {
        if (!lastTemperatures.ContainsKey(wire.Id))
            return new float[0];

        return lastTemperatures[wire.Id];
        /*
        float[] result = dao.getLastTempForWire(wire);
        if (result == null)
            return new float[wire.SensorCount];

        return result;
        */
    }


    public void deleteLastTemperatur()
    {
        dao.deleteLastTemperatur();
    }


    /// <summary>
    /// Возвращает фактические последние значения температур в строковом формате,
    /// при этом, в зависимости от настроек, может усреднять значения не считанных датчиков или выводить "-"
    /// </summary>
    /// <param name="wire">подвеска</param>
    /// <returns>значения температур, не возвращает null, если нет данных, возвращает пустой массив</returns>
    public float[] getLastTempForWireInStringFormat(Wire wire)
    {
        //float[] data = dao.getLastTempForWire(wire);
        var data = lastTemperatures[wire.Id];
        if (data == null)
        {
            var result = new float[wire.SensorCount];
            for (var i = 0; i < result.Length; i++)
                result[i] = -100;

            return result;
        }

        var getMid = settingsService.IsGetMidValueForBrokenSensor;
        return getStringFormatForTemperatuers(data, getMid);

        /*
        float[] data = dao.getLastTempForWire(wire);
        if (data == null)
        {
            string[] result = new string[wire.SensorCount];
            for (int i = 0; i < result.Length; i++)
                result[i] = "-";

            return result;
        }


        bool getMid = settingsService.IsGetMidValueForBrokenSensor;
        return getStringFormatForTemperatuers(data, getMid);
        */
    }



    public List<string> getUpperBoundTempForSilosInListFormat(Silos silos, DateTime time)
    {
        if (settingsService.OfflineMode)
            return new List<string>();

        return null;
    }


    public DateTime getTime(ref DateTime time)
    {
        return dao.getTime(ref time);
    }

    /// <summary>
    /// Возвращает фактические значения температур в строковом формате, записанных раннее заданного времени.
    /// при этом, в зависимости от настроек, может усреднять значения не считанных датчиков или выводить "-"
    /// </summary>
    /// <param name="wire">подвеска</param>
    /// <returns>значения температур, не возвращает null, если нет данных, возвращает пустой массив</returns>
    public float[] getUpperBoundTempForWireInStringFormat(Wire wire, ref DateTime time)
    {
        if (settingsService.OfflineMode)
            return new float[0];
        //string cte = dao.getTime(wire, time);
        //MessageBox.Show(cte);
        var data = dao.getUpperBoundTempTempForWire(wire, ref time);
        if (data == null)
        {
            var result = new float[wire.SensorCount];
            for (var i = 0; i < result.Length; i++)
                result[i] = (float)-100.0;

            return result;
        }

        var getMid = settingsService.IsGetMidValueForBrokenSensor;
        return getStringFormatForTemperatuers(data, getMid);
    }


    /// <summary>
    /// Добавляет значение температур.
    /// </summary>
    /// <param name="wire_id"></param>
    /// <param name="temps"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool addTemperatures(Wire wire, float[] temps, DateTime time)
    {
        lastTempTime = time;
        if (!lastTemperatures.ContainsKey(wire.Id))
            lastTemperatures.Add(wire.Id, temps);
        else
            lastTemperatures[wire.Id] = temps;

        if (temps.Length == 0) //Зануление температуры, когда плата не работает
            return true; //это значение не пишем ни в файл ни в бд.

        if (settingsService.OfflineMode)
        {
            //Пытаемся записать температуру в файл
            FileProcessingService.writeTemperature(settingsService.temperatureFilePath, wire.DeviceAddress, wire.Leg, temps, time);
            return true;
        }

        if (!dao.addTemperatures(wire, temps, time)) //не удалось добавить температуру.
        {
            //переходим в автономный режим.
            settingsService.OfflineMode = true;
            FileProcessingService.writeTemperature(settingsService.temperatureFilePath, wire.DeviceAddress, wire.Leg, temps, time);
            return false;
        }

        return true;
    }

    public Dictionary<Wire, SortedDictionary<DateTime, float[]>> getTemperatureBetweenTime(IEnumerable<Wire> wires, DateTime start, DateTime end)
    {
        if (settingsService.OfflineMode)
            return new Dictionary<Wire, SortedDictionary<DateTime, float[]>>();
        else
            return dao.getBetweenTimeTemperature(wires, start, end);
    }

    public SortedDictionary<DateTime, float> getAllAVGTempSilos(Silos silos, DateTime start, DateTime end)
    {
        if (settingsService.OfflineMode)
            return new SortedDictionary<DateTime, float>();
        else
            return dao.getAllAVGTempSilos(silos, start, end);
    }

    public SortedDictionary<DateTime, float[]> getMaxAvgMinBetweenTime(int[] wires, DateTime start, DateTime end)
    {
        if (settingsService.OfflineMode)
            return new SortedDictionary<DateTime, float[]>();
        else
            return dao.getMaxAvgMinBetweenTime(wires, start, end);
    }


    public List<DateTime> getSetTimeBetweenTime(DateTime start, DateTime end)
    {
        return dao.getSetTimeBetweenTime(start, end);

    }

    public SortedDictionary<DateTime, int> getFillingBetween(DateTime start, DateTime end, int silosId)
    {
        return dao.getFillingBetweenTimes(start, end, silosId);
    }

    public string[] getFillingAndGrain(int silosId, DateTime time)
    {
        return dao.getFillingAndGrain(silosId, time);
    }

    public SortedDictionary<DateTime, float> getAVGTemperatureSilosBetweenTime(Wire wire, DateTime start, DateTime end)
    {
        if (settingsService.OfflineMode)
            return new SortedDictionary<DateTime, float>();
        else
            return dao.getAvgTempSilosBetweenTime(wire, start, end);
    }


    public SortedDictionary<DateTime, float[]> getTemperatureForWireBetweenTime(Wire wire, DateTime start, DateTime end)
    {
        if (settingsService.OfflineMode)
            return new SortedDictionary<DateTime, float[]>();
        else
            return dao.getBetweenTimeTemperature(wire, start, end);
    }
    public bool loadTemperaturesFromFile(string filename)
    {
        if (settingsService.OfflineMode)
            return false;

        var result = FileProcessingService.readTemperatures(filename);
        if (result == null)
            return false;

        byte deviceAddress; ushort leg; float[] temps; DateTime time;
        foreach (var pairOfPairEntry in result)
        {
            deviceAddress = pairOfPairEntry.Key.Key;
            leg = pairOfPairEntry.Key.Value;
            time = pairOfPairEntry.Value.Key;
            temps = pairOfPairEntry.Value.Value;

            if (devices.ContainsKey(deviceAddress))
                foreach (var wire in devices[deviceAddress].Values)
                    if (wire.Leg == leg)
                        dao.addTemperatures(wire, temps, time);
        }

        return true;
    }

    /// <summary>
    /// Возвращает последние записанные температуры. Ключ - id подвески, значение - температруры.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, float[]> getLastTemperaturesInStringFormat()
    {
        var result = new Dictionary<int, float[]>();
        foreach (var wireTemp in lastTemperatures)
        {
            result.Add(wireTemp.Key, getStringFormatForTemperatuers(wireTemp.Value, settingsService.IsGetMidValueForBrokenSensor));
        }

        return result;
    }

    /// <summary>
    /// Возвращает последние записанные температуры. Ключ - id подвески, значение - температруры.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, float[]> getLastTemperatures()
    {
        return lastTemperatures;
    }

    #endregion

    #region silos

    /// <summary>
    /// Добавляет силос.
    /// </summary>
    /// <param name="silos">силос</param>
    /// <returns></returns>
    public Silos addSilos()
    {
        var silos = new Silos();
        if (settingsService.OfflineMode)
        {
            // Ключ выбирается как максимальный из ключей + 1
            if (siloses.Keys.Count == 0)
                silos.Id = 1;
            else
                silos.Id = siloses.Keys.Max() + 1;
            siloses.Add(silos.Id, silos);
            MyLoger.Log("SilosS: add silos offline " + silos.Name);
            return silos;
        }

        var newId = dao.addSilos(silos);
        if (newId == -1)
        {
            MyLoger.Log("SilosS: add silos error " + silos.Name);
            return null;
        }

        silos.Id = newId;
        siloses.Add(newId, silos);
        MyLoger.Log("SilosS: add silos " + silos.Name);
        return silos;
    }

    public Silos copySilos(Silos silos)
    {
        var silosN = addSilos();
        if (silosN == null)
            return null;

        silosN.Name = silos.Name;
        silosN.Red = silos.Red;
        silosN.Yellow = silos.Yellow;
        silosN.StructureId = silos.StructureId;
        silosN.X = silos.X;
        silosN.Y = silos.Y;
        silosN.W = silos.W;
        silosN.H = silos.H;
        silosN.Shape = silos.Shape;
        silosN.GrainId = silos.GrainId;

        return silosN;
    }

    /// <summary>
    /// Возвращает силос по id
    /// </summary>
    /// <param name="id">id Силоса</param>
    /// <returns>Силос</returns>
    public Silos getSilos(int id)
    {
        //Dictionary<int, Wire> wires = dao.getWireForSilos(id);
        //Silos s = dao.getSilos(id);
        //s.Wires = wires;
        return siloses[id];
    }

    /// <summary>
    /// Удаляет силос
    /// </summary>
    /// <param name="silos">силос</param>
    /// <returns>Удачность</returns>
    public bool deleteSilos(Silos silos)
    {
        siloses.Remove(silos.Id);
        MyLoger.Log("SilosS: dell silos " + silos.Name);
        if (settingsService.OfflineMode)
            return true;

        return dao.deleteSilos(silos.Id);
    }

    /// <summary>
    /// Обновляет силос
    /// </summary>
    /// <param name="silos">силос</param>
    /// <returns>Удачность</returns>
    public bool updateSilos(Silos silos)
    {
        MyLoger.Log(string.Format("SilosS: update silos: id= {0}, name= {1}, subdiv = {2}",
           silos.Id, silos.Name, silos.StructureId));
        if (silos == null) return false;
        siloses[silos.Id].Update(silos);
        if (settingsService.OfflineMode)
            return true;

        return dao.updateSilos(silos);
    }

    /// <summary>
    /// вычисление заполненности силоса в конкретный момент времени
    /// </summary>
    /// <param name="silos"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public bool addFilling(Silos silos, DateTime date)
    {
        return true;
    }


    /// <summary>
    /// вычисление заполненности силоса после опроса платы (lastTemperature)
    /// </summary>
    /// <param name="silos"></param>
    /// <returns></returns>
    public bool addFilling(Silos silos)
    {
        var temperature = new Dictionary<int, float[]>();
        var wires = new Dictionary<int, Wire>();
        foreach (var w in siloses[silos.Id].getEnabledWires())
        {
            temperature.Add(w.Id, lastTemperatures[w.Id]);
            wires.Add(w.Id, w);
        }
        var filling = HistoryFillingService.getFilling(temperature, wires);

        return dao.addHistory(silos.Id, filling, silos.GrainId, lastTempTime);
    }

    /// <summary>
    /// Возвращает все существующие силосы
    /// </summary>
    /// <returns>Ключ - id Силоса</returns>
    public Dictionary<int, Silos> getAllSiloses()
    {
        return siloses;
        /*
        Dictionary<int, Wire> wires = dao.getAllWires();
        Dictionary<int, Silos> siloses = dao.getAllSilos();
        // Заполняем силосы подвесками
        foreach (var entry in wires)
        {
            Wire w = entry.Value;
            if (siloses.ContainsKey(w.SilosId))
            {
                siloses[w.SilosId].Wires.Add(w.Id, w);
            }
        }

        return siloses;
        */
    }

    #endregion

    #region wire

    /// <summary>
    /// Возвращает 
    /// </summary>
    /// <returns>
    /// ключ первого словаря - адресс платы.
    /// ключ второго словаря - id подвески
    /// </returns>
    public Dictionary<int, Dictionary<int, Wire>> getDevices()
    {
        return devices;
    }

    public Dictionary<int, Wire> CreateDevice(int legCount = 10)
    {
        var result = new Dictionary<int, Wire>();
        var addres = generateAddress();

        for (var i = 0; i < legCount; i++)
        {
            var w = new Wire(-1, -1, addres, (ushort)(i + 1), 0, false, WireTypeEnum.TOP_TO_BOT_DS18b20, 0.5f, 0.5f);
            //Добавляем подвеску в БД
            if (!addWireToDevice(ref w))
                return new Dictionary<int, Wire>();
            //else                
            //result.Add(w.Leg, w);                
        }

        return devices[addres];
    }
    public Dictionary<int, Wire> CopyDevice(Dictionary<int, Wire> wiresToCopy)
    {
        var result = CreateDevice(wiresToCopy.Count);
        foreach (var w in wiresToCopy.Values)
        {
            foreach (var pair in result)
            {
                if (pair.Value.Leg == w.Leg)
                {
                    result[pair.Key].Number = w.Number;
                    result[pair.Key].SilosId = -1;
                    result[pair.Key].Enable = w.Enable;
                    result[pair.Key].Type = w.Type;
                    result[pair.Key].SensorCount = w.SensorCount;
                    result[pair.Key].X = w.X;
                    result[pair.Key].Y = w.Y;
                    updateWire(result[pair.Key]);
                    break;
                }
            }
        }
        return result;
    }

    private byte generateAddress()
    {
        //генерируем адресс
        for (byte i = 1; i < 255; i++)
            if (!devices.Keys.Contains(i))
                return i;

        return 255;
    }

    /// <summary>
    /// Возвращает словарь с подвесками на плате с данным аддресом
    /// </summary>
    /// <param name="address">Адрес платы</param>
    /// <returns>Словарь с подвесками, null если не существует</returns>
    public Dictionary<int, Wire> getWiresOnDevice(byte address)
    {
        if (devices.ContainsKey(address))
            return devices[address];

        MyLoger.Log("SilosS: get wires error");
        return new Dictionary<int, Wire>();
    }

    /// <summary>
    /// Добавляет подвеску
    /// </summary>
    /// <param name="wire">подвеска, id которой будт заменен<param>
    /// <returns>Id подвески</returns>
    public bool addWireToDevice(ref Wire wire)
    {
        MyLoger.Log("SilosS: add wire " + wire.ToString());
        //Такой платы еще нет
        if (!devices.ContainsKey(wire.DeviceAddress))
            devices.Add(wire.DeviceAddress, new Dictionary<int, Wire>());

        var newId = 0;

        // Создаем новое id путем поиска максимального
        if (settingsService.OfflineMode)
        {
            foreach (var dictEntry in devices)
                if (dictEntry.Value.Keys.Count != 0)
                    newId = Math.Max(dictEntry.Value.Keys.Max(), newId);

            newId++;
        }
        else
        {
            newId = dao.addWire(wire);
            if (newId == -1)
            {
                MyLoger.Log("SilosS: add wire error " + wire.ToString());
                return false;
            }
        }

        wire.Id = newId;
        devices[wire.DeviceAddress].Add(wire.Id, wire);
        MyLoger.Log("SilosS: add wire " + wire.ToString());
        return true;
    }

    /// <summary>
    /// Обновляет подвеску
    /// </summary>
    /// <param name="wire">Подвеска, которую надо обновить</param>
    /// <returns>Успешность</returns>
    public bool updateWire(Wire wire)
    {
        foreach (var dictEntry in devices)
            if (dictEntry.Value.ContainsKey(wire.Id))
            {
                dictEntry.Value[wire.Id] = wire;
                break;
            }

        MyLoger.Log("SilosS: update wire " + wire.ToString());
        if (settingsService.OfflineMode) return true;

        return dao.updateWire(wire);
    }

    /// <summary>
    /// Удаляет подвеску
    /// </summary>
    /// <param name="wire">Подвеска, которую надо удалить</param>
    /// <returns>Успешность</returns>
    public bool deleteWire(Wire wire)
    {
        foreach (var dictEntry in devices)
            if (dictEntry.Value.ContainsKey(wire.Id))
            {
                dictEntry.Value.Remove(wire.Id);
                break;
            }
        MyLoger.Log("SilosS: delete wire " + wire.ToString());
        if (settingsService.OfflineMode) return true;

        return dao.deleteWire(wire.Id);
    }

    public bool deleteDevice(byte deviceAddress)
    {
        if (!devices.ContainsKey(deviceAddress))
            return true;

        foreach (var wireId in devices[deviceAddress].Keys)
        {
            //удаляем подвески из силосов.
            foreach (var silos in siloses.Values)
                if (silos.Wires.ContainsKey(wireId))
                    silos.Wires.Remove(wireId);
            if (!settingsService.OfflineMode)
                if (!dao.deleteWire(wireId))
                {
                    MyLoger.Log("SilosS: delete device error" + deviceAddress.ToString());
                    return false;
                }
        }
        MyLoger.Log("SilosS: delete device " + deviceAddress.ToString());
        devices.Remove(deviceAddress);
        return true;
    }

    public bool changeDeviceAddress(byte oldAddress, byte newAddress)
    {
        if (devices.ContainsKey(newAddress))
        {
            MyLoger.Log("SilosS: update device address error, addres new exists " + newAddress);
            return false;
        }

        if (!devices.ContainsKey(oldAddress))
        {
            MyLoger.Log("SilosS: update device address error, addres old not exists " + oldAddress);
            return false;
        }

        MyLoger.Log("SilosS: update device address from " + oldAddress + " to " + newAddress);

        devices.Add(newAddress, devices[oldAddress]);
        devices.Remove(oldAddress);
        var ok = true;
        foreach (var w in devices[newAddress].Values)
        {
            w.DeviceAddress = newAddress;
            if (!settingsService.OfflineMode)
                if (!dao.updateWire(w))
                {
                    ok = false;
                    break;
                }
        }

        if (!ok)  //Ворочаем все обратно!
        {
            devices.Add(oldAddress, devices[newAddress]);
            devices.Remove(newAddress);
            foreach (var w in devices[newAddress].Values)
            {
                w.DeviceAddress = oldAddress;
            }
            return false;
        }

        return true;
    }

    public bool resizeDevice(byte address, int newLegNumber)
    {
        if (!devices.ContainsKey(address))
            return false;

        var device = devices[address];

        //удаляем лишнее
        if (device.Count > newLegNumber)
        {
            var wires = device.Values.ToList();
            foreach (var w in wires)
            {
                if (w.Leg > newLegNumber)
                    deleteWire(w);
            }
        }
        else //добавляем
        {
            for (var i = device.Count; i < newLegNumber; i++)
            {
                var w = new Wire(-1, -1, address, (ushort)(i + 1), 0, false, WireTypeEnum.TOP_TO_BOT_DS18b20, 0.5f, 0.5f);
                //Добавляем подвеску в БД
                if (!addWireToDevice(ref w))
                    return false;
            }
        }

        return true;

    }

    #endregion

    #region subdivs

    public Dictionary<int, StructureSubdivision> getSubdivisions()
    {
        return subdivisions;
    }

    public bool addSubdivision(string name)
    {
        var s = new StructureSubdivision(-1, name);
        return addSubdivision(ref s);
    }

    public bool addSubdivision(ref StructureSubdivision subdiv)
    {
        if (settingsService.OfflineMode)
        {
            // Ключ выбирается как максимальный из ключей + 1
            if (subdivisions.Keys.Count == 0)
                subdiv.Id = 1;
            else
                subdiv.Id = subdivisions.Keys.Max() + 1;
            subdivisions.Add(subdiv.Id, subdiv);
            MyLoger.Log("SilosS: add subdiv offline " + subdiv.Name);
            return true;
        }

        var newId = dao.addSubdivision(subdiv);
        if (newId == -1)
        {
            MyLoger.Log("SilosS: add subdiv error " + subdiv.Name);
            return false;
        }

        subdiv.Id = newId;
        subdivisions.Add(newId, subdiv);
        MyLoger.Log("SilosS: add subdiv " + subdiv.Name);
        return true;
    }

    public void updateSubdivisions(StructureSubdivision subdiv)
    {
        MyLoger.Log("SilosS: update subdiv" + subdiv.Name);
        if (subdivisions.ContainsKey(subdiv.Id))
        {
            subdivisions[subdiv.Id].Name = subdiv.Name;
            if (!settingsService.OfflineMode)
                dao.updateSubdivision(subdiv);
        }
    }

    public bool deleteSubdivision(StructureSubdivision subdiv)
    {
        if (!subdivisions.ContainsKey(subdiv.Id))
            return true;

        subdivisions.Remove(subdiv.Id);
        MyLoger.Log("SilosS: dell subdiv " + subdiv.Name);
        if (settingsService.OfflineMode)
            return true;

        return dao.deleteSubdivision(subdiv.Id);
    }

    public bool deleteSubdivision(int id)
    {
        if (!subdivisions.ContainsKey(id))
            return true;

        MyLoger.Log("SilosS: dell subdiv " + subdivisions[id].Name);
        subdivisions.Remove(id);

        if (settingsService.OfflineMode)
            return true;

        return dao.deleteSubdivision(id);
    }

    #endregion

    #region graines

    #endregion

    #region Статические методы

    /// <summary>
    /// Определяет цвет для заданных температур
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="yellow"></param>
    /// <param name="red"></param>
    /// <returns></returns>
    static public Color getColor(string temp, float yellow, float red)
    {
        if (temp == "-") return Color.Gray;
        else return getColor(Convert.ToSingle(temp), yellow, red);
    }

    static public Color getColor(float temp, float yellow, float red)
    {
        if (temp > 140 || temp < -90) return Color.LightGray;
        if (temp <= 0) return Color.Blue;
        if (temp >= red) return Color.Red;
        if (temp >= yellow) return Color.Yellow;
        return Color.LightGreen;
    }


    //данные дном к верху
    static public float[] getStringFormatForTemperatuers(float[] data, bool isGetMidForBroken)
    {
        var result = new float[data.Length];

        var countOfNormalSensors = 0;
        float sumTemp = 0;
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] < -90 || data[i] > 140)// ставим прочерк // изменение ставим -100
            {
                result[i] = -100;
                continue;
            }

            sumTemp += data[i];
            countOfNormalSensors++;

            result[i] = (float)Math.Round(data[i], 1);
        }

        if (isGetMidForBroken) //Округляем, если надо округлять
        {
            var mid = sumTemp / countOfNormalSensors;
            for (var i = 0; i < result.Length; i++)
                if (result[i] == -100)
                    result[i] = (float)Math.Round(mid, 1);
        }
        return result;
    }

    static public Image getImageByShape(SilosShapeEnum shape)
    {
        switch (shape)
        {
            case SilosShapeEnum.BIG:
                return Properties.Resources.silos_gray_big;
            case SilosShapeEnum.SMALL:
                return Properties.Resources.silos_gray_small;
                
        }

        return new Bitmap(500, 500);
    }

    static public Image getImageByColorAndShape(SilosColorEnum color, SilosShapeEnum shape)
    {
        switch (shape)
        {
            case SilosShapeEnum.BIG:
                return getBigSilosImageByColor(color);
            case SilosShapeEnum.SMALL:
                return getSmallSilosImageByColor(color);
            default:
                break;
        }

        /*
        switch (color)
        {
            case SilosColorEnum.BLUE:
                return Properties.Resources.silos_blue;
            case SilosColorEnum.BLUEGREEN:
                return Properties.Resources.silos_blue_green;
            case SilosColorEnum.CRIMSON:
                return Properties.Resources.silos_crimson;
            case SilosColorEnum.GREEN:
                return Properties.Resources.silos_green;
            case SilosColorEnum.RED:
                return Properties.Resources.silos_red;
            case SilosColorEnum.YELLOW:
                return Properties.Resources.silos_yellow;
            case SilosColorEnum.YELLOWGREEN:
                return Properties.Resources.silos_yellow_green;
        }*/
        return null;
    }

    static public Image getSmallSilosImageByColor(SilosColorEnum color)
    {
        switch (color)
        {
            case SilosColorEnum.BLUE:
                return Properties.Resources.small_silos_blue;
            case SilosColorEnum.BLUEGREEN:
                return Properties.Resources.small_silos_green;
            case SilosColorEnum.CRIMSON:
                return Properties.Resources.small_silos_crimson;
            case SilosColorEnum.GREEN:
                return Properties.Resources.small_silos_green;
            case SilosColorEnum.RED:
                return Properties.Resources.small_silos_red;
            case SilosColorEnum.YELLOW:
                return Properties.Resources.small_silos_yellow;
            case SilosColorEnum.YELLOWGREEN:
                return Properties.Resources.small_silos_yellow;
            case SilosColorEnum.GREY:
                return Properties.Resources.silos_gray_small;
        }
        return null;
    }

    static public Image getBigSilosImageByColor(SilosColorEnum color)
    {
        switch (color)
        {
            case SilosColorEnum.BLUE:
                return Properties.Resources.silos_blue;
            case SilosColorEnum.BLUEGREEN:
                return Properties.Resources.silos_blue_green;
            case SilosColorEnum.CRIMSON:
                return Properties.Resources.silos_crimson;
            case SilosColorEnum.GREEN:
                return Properties.Resources.silos_green;
            case SilosColorEnum.RED:
                return Properties.Resources.silos_red;
            case SilosColorEnum.YELLOW:
                return Properties.Resources.silos_yellow;
            case SilosColorEnum.YELLOWGREEN:
                return Properties.Resources.silos_yellow_green;
            case SilosColorEnum.GREY:
                return Properties.Resources.silos_gray_big;
        }
        return null;
    }


    /// <summary>
    /// Считает среднее значение температур, исключая неправильные значения
    /// </summary>
    /// <param name="temp">температуры</param>
    /// <returns>среднее значения</returns>
    static public float getMiddleTemperature(float[] temp)
    {
        float sum = 0;
        var count = 0;
        for (var i = 0; i < temp.Length; i++)
            if (temp[i] > -80 && temp[i] < 150)
            {
                sum += temp[i];
                count++;
            }

        if (count == 0)
            return -99;
        return sum / count;
    }

    /// <summary>
    /// Считает среднее значение температур, исключая неправильные значения
    /// </summary>
    /// <param name="temp">температуры</param>
    /// <returns>среднее значения</returns>
    static public float getMinTemperature(float[] temp)
    {
        float result = 150;
        for (var i = 0; i < temp.Length; i++)
            if (temp[i] > -80 && temp[i] < 150 && result > temp[i])
            {
                result = temp[i];
            }

        if (result > 140) return -99;
        else return result;
    }



    #endregion
}
