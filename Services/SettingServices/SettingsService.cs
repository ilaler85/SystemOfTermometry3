using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.Services;

public partial class SettingsService
{
    private SystemOfThermometry3.DAO.Dao dao;
    private Dictionary<string, string> settings; //Кэшированные настройки

    private bool offlineMode = false; // Режим, в котором база данных не обязательна,
                                      // конфигурация и считываемые данные будут сохраняться в файл

    private bool newSyle = false;
    public bool OfflineMode { get => offlineMode; set => offlineMode = value; }

    private bool isAdminMode = false; // Режим, в котором можно осуществлять настройку конфигурации и двигать компоненты
    public bool IsAdminMode { get => isAdminMode; set => isAdminMode = value; }

    public SettingsService(SystemOfThermometry3.DAO.Dao dao)
    {
        this.dao = dao ?? throw new ArgumentNullException("dao is null");
        settings = new Dictionary<string, string>();

        if (!Directory.Exists(MailPlotToSendBasisPath))
            Directory.CreateDirectory(MailPlotToSendBasisPath);
        if (!Directory.Exists(OverheatMailExportPath))
            Directory.CreateDirectory(OverheatMailExportPath);

    }

    /// <summary>
    /// Синхронизируется с базой данных, путем загрузки конфигурации из базы данных
    /// </summary>
    public void synchronizeWithGettingData()
    {
        foreach (var keyValueEntry in settings.ToArray())
        {
            string result = keyValueEntry.Value; // Если не будет найдено, то оставим так как было
            dao.getSettingRecord(keyValueEntry.Key, ref result);
            settings[keyValueEntry.Key] = result;
        }
    }

    /// <summary>
    /// Синхронизация с базой данных, путем сохранения текущей конфигурации в базу данных
    /// </summary>
    public void synchronizeWithSettingData()
    {
        foreach (var keyValueEntry in settings)
        {
            string propertyName = keyValueEntry.Key;
            string value = keyValueEntry.Value;
            string result = "";
            if (!dao.getSettingRecord(propertyName, ref result))
                dao.addSettingRecord(propertyName, value.ToString());
            else
                dao.updateSettingRecord(propertyName, value.ToString());
        }
    }

    /// <summary>
    /// Задает значение настройки
    /// </summary>
    /// <param name="propertyName">Уникальное название настройки</param>
    /// <param name="value">новое значение</param>
    private void setProperty(string propertyName, string value)
    {
        if (!settings.ContainsKey(propertyName))
            settings.Add(propertyName, value);
        else
            settings[propertyName] = value;

        string result = "";
        //Такой записи еще нет
        if (!dao.getSettingRecord(propertyName, ref result))
            dao.addSettingRecord(propertyName, value.ToString());
        else
            dao.updateSettingRecord(propertyName, value.ToString());

    }

    /// <summary>
    /// Возвращает занчение настройки
    /// </summary>
    /// <param name="propertyName">Уникальное название настройки</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <returns>Значение</returns>
    private string getProperty(string propertyName, string defaultValue)
    {
        string result = defaultValue;
        if (settings.ContainsKey(propertyName)) // Есть кэшированное значение, возвращаем его.
            return settings[propertyName];

        // Нет кешированного значения
        if (offlineMode) // Нет доступа к базе данных.
        {
            settings[propertyName] = defaultValue; // кэшируем значение по умолчанию
            return defaultValue; // и возвращаем его.
        }

        // если есть доступ к базе данных, то кэшируем данные из нее
        dao.getSettingRecord(propertyName, ref result);
        settings[propertyName] = result;
        return result;
    }

    /// <summary>
    /// Будет ли запрашиваться пароль при открытии настроек и старте/остановке опроса
    /// </summary>
    public bool IsBlockUserSettingsAndObservWithPSWD
    {
        get => Convert.ToBoolean(getProperty("block_settings_n_observ", false.ToString()));
        set => setProperty("block_settings_n_observ", value.ToString());

    }


    /// <summary>
    /// Если true, то опрос будет недоступен, но будет пытаться подтянуть температуры с удаленной бд
    /// </summary>
    public bool IsOnlyReadMode
    {

        get
        {
            bool result = false;
            string str = FileProcessingService.getStringFromFile("IsOnlyReadMode");
            if (str != "")
                try { result = Convert.ToBoolean(str); }
                catch { }

            return result;


        }
        set => FileProcessingService.setStringToFile("IsOnlyReadMode", value.ToString());
    }

    /// <summary>
    /// Если true, то опрос будет недоступен, но будет пытаться подтянуть температуры с удаленной бд
    /// </summary>
    static public bool IsOnlyReadModeS
    {

        get
        {
            bool result = false;
            string str = FileProcessingService.getStringFromFile("IsOnlyReadMode");
            if (str != "")
                try { result = Convert.ToBoolean(str); }
                catch { }

            return result;


        }
        set => FileProcessingService.setStringToFile("IsOnlyReadMode", value.ToString());
    }


    /// <summary>
    /// Путь к файлу, в который сохраняются температуры
    /// </summary>
    public string temperatureFilePath
    {
        get => getProperty("temperature_file_path", "temperature.txt");
        set => setProperty("temperature_file_path", value);
    }


    /// <summary>
    /// Опрашиваются ли сейчас платы или нет. Только для информации.
    /// </summary>
    public bool IsObserving
    {
        get => Convert.ToBoolean(getProperty("is_observing", false.ToString()));
        set => setProperty("is_observing", value.ToString());
    }


}
