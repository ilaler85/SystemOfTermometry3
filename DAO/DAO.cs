using System;
using System.Collections.Generic;
using System.Data;
using SystemOfThermometry3.Model;

namespace SystemOfThermometry3.DAO;

//Интерфейс для работы с базой данных
//Что бы перейти на другую бд, нужно унаследоваться, реализовать все методы
// и поменять в коде место, где инициализируется DAO
public abstract class Dao
{

    /// <summary>
    /// Проверяет базу данных на наличие необходимых таблиц
    /// </summary>
    /// <returns></returns>
    public abstract bool DBisCorrect();

    /// <summary>
    /// Подключиться к базе данных
    /// </summary>
    /// <param name="connectProperties">Строка с параметрами подключения</param>
    /// <returns>Удачность</returns>
    public abstract bool connectToDB(string connectProperties, bool OnlyView = false);

    /// <summary>
    /// Сброс таблиц силосов, подвесок, подразделений и настроек и создание новых
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool dropAndCreateModel();

    /// <summary>
    /// Сброс всех таблиц и создание новых
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool dropAndCreateDB();

    /// <summary>
    /// Сброс всех таблицы температур и создание новой
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool dropTemperatureTable();

    /// <summary>
    /// Добавляет пользователя - обзорщика к базе данных.
    /// </summary>
    /// <param name="dataBaseName">база данных</param>
    /// <returns></returns>
    public abstract bool addUser(string dataBaseName);

    /// <summary>
    /// Выполнения запроса, возвращающего значения
    /// </summary>
    /// <param name="sqlQuery">запрос</param>
    /// <returns>Полученная таблица. null если неудачно</returns>
    protected abstract DataTable executeSelectQuery(string sqlQuery);

    /// <summary>
    /// Выполнение запроса, вставляющего значение.
    /// </summary>
    /// <param name="sqlQuery">запрос</param>
    /// <returns>Id вставленного значения. -1 если неудачно</returns>
    protected abstract long executeInsertQuery(string sqlQuery);

    /// <summary>
    /// Выполнение запроса, обнавляющие запись
    /// </summary>
    /// <param name="sqlQuery">запрос</param>
    /// <returns>Удачность</returns>
    protected abstract bool executeUpdateQuery(string sqlQuery);

    #region silos working

    /// <summary>
    /// Возвращает все имеющиеся силосы.
    /// </summary>
    /// <returns>Словарь, где ключ - id Силоса</returns>
    public abstract Dictionary<int, Silos> getAllSilos();

    /// <summary>
    /// Возвращает Силос по id.
    /// </summary>
    /// <returns>Силос</returns>
    public abstract Silos getSilos(int id);

    /// <summary>
    /// Добавляет Силос.
    /// </summary>
    /// <returns>id добавленного силоса</returns>
    public abstract int addSilos(Silos silos);

    /// <summary>
    /// Обновляет Силос.
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool updateSilos(Silos silos);

    /// <summary>
    /// Удаляет Силос.
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool deleteSilos(int silosId);

    /// <summary>
    /// Удаляет Силос.
    /// </summary>
    /// <returns>Удачность</returns>
    public bool deleteSilos(Silos silos)
    {
        return deleteSilos(silos.Id);
    }

    #endregion

    #region wire working

    /// <summary>
    /// Возвращает все существующие подвески
    /// </summary>
    /// <returns>Словарь с подвесками, где ключ - id подвески</returns>
    public abstract Dictionary<int, Wire> getAllWires();

    /// <summary>
    /// Возвращает подвесу по id
    /// </summary>
    /// <param name="id">id Подвески</param>
    /// <returns>Подвеска</returns>
    public abstract Wire getWire(int id);

    /// <summary>
    /// Возвращает все подвески для силоса
    /// </summary>
    /// <param name="silosId">id силоса</param>
    /// <returns>Словарь с подвесками, где ключ - id подвески</returns>
    public abstract Dictionary<int, Wire> getWireForSilos(int silosId);

    /// <summary>
    /// Возвращает все подвески для силоса
    /// </summary>
    /// <param name="silos">силоса</param>
    /// <returns>Словарь с подвесками, где ключ - id подвески</returns>
    public Dictionary<int, Wire> getWireForSilos(Silos silos)
    {
        return getWireForSilos(silos.Id);
    }

    /// <summary>
    /// Добавляет подвеску.
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <returns>Id Подвески</returns>
    public abstract int addWire(Wire wire);

    /// <summary>
    /// Обновляет подвеску.
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <returns>Удачность</returns>
    public abstract bool updateWire(Wire wire);

    /// <summary>
    /// Удаляет подвеску.
    /// </summary>
    /// <param name="wireId">Id Подвески</param>
    /// <returns>Удачность</returns>
    public abstract bool deleteWire(int wireId);

    /// <summary>
    /// Удаляет подвеску.
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <returns>Удачность</returns>
    public bool deleteWire(Wire wire)
    {
        return deleteWire(wire.Id);
    }

    #endregion

    #region subdivision working

    /// <summary>
    /// Возвращает все имеющиеся структурные подразделения.
    /// </summary>
    /// <returns>Словарь, где ключ - id подразделения</returns>
    public abstract Dictionary<int, StructureSubdivision> getAllSubdivisions();

    /// <summary>
    /// Возвращает подразделение по id.
    /// </summary>
    /// <returns>Подразделение</returns>
    public abstract StructureSubdivision getSubdivision(int id);

    /// <summary>
    /// Добавляет подразделение.
    /// </summary>
    /// <returns>id добавленного подразделения</returns>
    public abstract int addSubdivision(StructureSubdivision subdivision);

    /// <summary>
    /// Обновляет подразделение.
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool updateSubdivision(StructureSubdivision subdivision);

    /// <summary>
    /// Удаляет подразделение.
    /// </summary>
    /// <returns>Удачность</returns>
    public abstract bool deleteSubdivision(int subdivisionId);

    /// <summary>
    /// Удаляет подразделение.
    /// </summary>
    /// <returns>Удачность</returns>
    public bool deleteSubdivision(StructureSubdivision subdivision)
    {
        return deleteSilos(subdivision.Id);
    }

    #endregion

    #region temperature working

    /// <summary>
    /// Возвращает последние записанные данные для подвески
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <returns>Значения</returns>
    public abstract float[] getLastTempForWire(Wire wire);

    /// <summary>
    /// Возвращает последние записанные данные для подвески
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <param name="time">Время записи температуры</param>
    /// <returns>Значения</returns>
    public abstract float[] getLastTempForWire(Wire wire, ref DateTime time);




    /// <summary>
    /// Возвращает время близкое к заданному
    /// </summary>
    /// <param name="wire"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public abstract DateTime getTime(ref DateTime dateTime);

    /// <summary>
    /// Возвращает значения температур для заданой подвески, добавленные раньше заданного времени
    /// </summary>
    /// <param name="wire">Подвеска</param>
    /// <param name="time">Время записи температуры</param>
    /// <returns>Значения</returns>
    public abstract float[] getUpperBoundTempTempForWire(Wire wire, ref DateTime time);

    /// <summary>
    /// Добавляет значения температуры для подвески
    /// </summary>
    /// <param name="wire_id">Подвеска</param>
    /// <param name="temps">значение температур</param>
    /// <param name="time">Время</param>
    /// <returns>Удачность</returns>
    public abstract bool addTemperatures(Wire wire, float[] temps, DateTime time);

    /// <summary>
    /// Удаляет все записи температур до определенного времени.
    /// </summary>
    /// <param name="date">Время до которого все будет удалено</param>
    /// <returns>Удачность</returns>
    public abstract bool deleteTemperaturesBeforeDate(DateTime time);
    /// <summary>
    /// очищение таблицы последних температур перед заполнением этой таблицы
    /// </summary>
    public abstract void deleteLastTemperatur();

    /// <summary>
    /// Возвращает значения температур за данный промежуток времени
    /// </summary>
    /// <param name="wires">список подвесок, для которых нужно получить температуру</param>
    /// <param name="start">начальное время</param>
    /// <param name="end">конечное время</param>
    /// <returns>Словарь, где ключ - id подвески; значение - словарь, где ключ - время, а значение - температуры.</returns>
    public abstract Dictionary<Wire, SortedDictionary<DateTime, float[]>> getBetweenTimeTemperature(IEnumerable<Wire> wires, DateTime start, DateTime end);

    /// <summary>
    /// Возвращает значения температур за данный промежуток времени
    /// </summary>
    /// <param name="wire">подвеска, для которой нужно получить температуру</param>
    /// <param name="start">начальное время</param>
    /// <param name="end">конечное время</param>
    /// <returns>Словарь, где ключ - время, а значение - температуры.</returns>
    public abstract SortedDictionary<DateTime, float[]> getBetweenTimeTemperature(Wire wire, DateTime start, DateTime end);

    ///<summary>
    ///Выдает таблицу максимумов, средних и минимальных температур
    /// </summary>
    /// <param name="wires">Список индексов подвесок</param>
    /// <param name="start">начальное время</param>
    /// <param name="end">конечное время</param>
    /// <returns>Словарь, где ключи - время, значения - температуры максимум, среднее, мнимум</returns>
    public abstract SortedDictionary<DateTime, float[]> getMaxAvgMinBetweenTime(int[] wires, DateTime start, DateTime end);


    /// <summary>
    /// Возвращает множество точек времени измерения температуры на заданном отрезке времени для конкреного силоса
    /// </summary>
    /// <param name="start">Времена старта</param>
    /// <param name="end">Время конца</param>
    /// <param name="silosId">ИД силоса</param>
    /// <returns></returns>
    public abstract List<DateTime> getSetTimeBetweenTime(DateTime start, DateTime end, int silosId);


    /// <summary>
    /// Возвращает множество точек времени записи в заданном отрезке времени
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract List<DateTime> getSetTimeBetweenTime(DateTime start, DateTime end);

    /// <summary>
    /// Запрашивает сразу список средней температуры для конкретного силуса
    /// </summary>
    /// <param name="silos"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract SortedDictionary<DateTime, float> getAvgTempSilosBetweenTime(Wire wire, DateTime start, DateTime end);

    /// <summary>
    /// Выдвет таблицу средних температур в силусе для всех подвесок
    /// </summary>
    /// <param name="silos"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract SortedDictionary<DateTime, float> getAllAVGTempSilos(Silos silos, DateTime start, DateTime end);

    /// <summary>
    /// Метод получения из бд среднюю температуру по уровням в конкретный момент времени
    /// </summary>
    /// <param name="idDevice">Лист id девайсов которые закреплены за силосами</param>
    /// <param name="time">Время вычисления </param>
    /// <returns>list средних температур по силосу/returns>
    public abstract List<float> getAVGSilosTemp(List<int> idDevice, DateTime time);
    #endregion

    #region grain working
    /// <summary>
    /// добавление типа зерна
    /// </summary>
    public abstract int addGrain(Grain grain);

    /// <summary>
    /// удаление таблицы типов зерна
    /// </summary>
    /// <param name="grainId"></param>
    /// <returns></returns>
    public abstract bool deleteGrain(int grainId);

    /// <summary>
    /// Получение конкретного типа зерна
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public abstract Grain getGrain(int id);

    /// <summary>
    /// Получение списка типов зерна
    /// </summary>
    /// <returns></returns>
    public abstract Dictionary<int, Grain> getAllGrains();
    /// <summary>
    /// обновление информации о типе зерна
    /// </summary>
    /// <param name="grain"></param>
    /// <returns></returns>
    public abstract bool updateGrain(Grain grain);



    #endregion

    #region history working

    /// <summary>
    /// добавление данных об истории заполненности
    /// </summary>
    /// <returns></returns>
    public abstract bool addHistory(int silosID, int filling, int grainID, DateTime time);

    public abstract bool addMoreHistory(DateTime time, Dictionary<int, int> silosFilling);

    /// <summary>
    /// удаление таблицы истории
    /// </summary>
    /// <returns></returns>
    public abstract bool deleteHistory();

    /// <summary>
    /// обновление данных об истории
    /// </summary>
    /// <returns></returns>
    public abstract bool updateHistory();

    /// <summary>
    /// получает данные по заполненности конкретного силоса
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="silosId"></param>
    /// <returns></returns>
    public abstract SortedDictionary<DateTime, int> getFillingBetweenTimes(DateTime start, DateTime end, int silosId);
    /// <summary>
    /// получаем лист дат по которым есть информация
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public abstract List<DateTime> getTimesBetweenFromFillingTable(DateTime start);
    /// <summary>
    /// получает процент заполненности силоса и тип зерна в нем
    /// </summary>
    /// <param name="silosId">Силос</param>
    /// <param name="time">Время измерения</param>
    /// <returns>Возвращает массив из двух элеметнов уровень заполнения и названия типа зерна</returns>
    public abstract string[] getFillingAndGrain(int silosId, DateTime time);
    /// <summary>
    /// Получение времени близкого к введенному
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public abstract DateTime getFillingTime(DateTime time);
    #endregion

    #region setting working
    /// <summary>
    /// Добавляет запись в таблицу с настройками
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    /// <returns>Удачность</returns>
    public abstract bool addSettingRecord(string key, string value);

    /// <summary>
    /// Изменяет запись в таблице с настройками
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    /// <returns>Удачность</returns>
    public abstract bool updateSettingRecord(string key, string value);

    /// <summary>
    /// Берет значение из таблицы с настройками
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Возвращаемое значение</param>
    /// <returns>Удачность, также false если такого значения нету</returns>
    public abstract bool getSettingRecord(string key, ref string value);
    #endregion
}
