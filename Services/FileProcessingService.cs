using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using SystemOfThermometry3.Model;
using Windows.ApplicationModel;
using Windows.Storage;
using FileAttributes = System.IO.FileAttributes;

namespace SystemOfThermometry3.Services;

/// <summary>
/// Статический класс для работы с файлами
/// </summary>
static public class FileProcessingService
{
    //private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static string appData = "D:\\";
    private static string settingsFolder 
    { 
        get => Path.Combine(WorkingDir, ".folder");
    }
    private static string pswdFolder
    {
        get => Path.Combine(WorkingDir, ".pswd");
    }

    public static string ExportDir
    {
        get => Path.Combine(WorkingDir, "Export");
    }
    public static string ExportOverheatDir
    {
        get => Path.Combine(WorkingDir, "ExportOverheat");
    }

    public static string LogDir
    {
        get => Path.Combine(WorkingDir, "Log");
    }

    public static string WorkingDir
    {
        get => Path.Combine(appData, "SystemOfThermometry3");
    }

    /// Сериалайзер для сохранения и чтения из файла конфигурации силосов и подвесок.
    private static DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(
            typeof(Tuple<Dictionary<int, Silos>, Dictionary<int, Dictionary<int, Wire>>, Dictionary<int, StructureSubdivision>>));


    static private void createSettingsFolderIfNotExists()
    {
        if (!Directory.Exists(WorkingDir))
        {
            var di = Directory.CreateDirectory(WorkingDir) ;
            di.Attributes = FileAttributes.Directory;
        }

        if (!Directory.Exists(settingsFolder))
        {
            var di = Directory.CreateDirectory(settingsFolder);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        }

        if (!Directory.Exists(ExportDir))
        {
            var di = Directory.CreateDirectory(ExportDir);
            di.Attributes = FileAttributes.Directory;
        }

        if (!Directory.Exists(ExportOverheatDir))
        {
            var di = Directory.CreateDirectory(ExportOverheatDir);
            di.Attributes = FileAttributes.Directory;
        }

        if (!Directory.Exists(LogDir))
        {
            var di = Directory.CreateDirectory(LogDir);
            di.Attributes = FileAttributes.Directory;
        }

    }

    /// <summary>
    /// Пытается найти файл со стройкой, которая необходима для подключения к бд
    /// </summary>
    /// <returns>строка подключения</returns>
    static public string getConnectionString()
    {
        createSettingsFolderIfNotExists();
        var file = Path.Combine(settingsFolder, "connection");
        if (!File.Exists(file))
            return "";

        var stream = new StreamReader(file);
        var connectionString = stream.ReadToEnd();
        stream.Close();

        return connectionString;
    }

    /// <summary>
    /// Изменяет файл с строкой подключения
    /// </summary>
    /// <param name="connectionString">Новая строка подключения</param>
    static public void setConnectionString(string connectionString)
    {
        createSettingsFolderIfNotExists();
        var file = Path.Combine(settingsFolder, "connection");

        var stream = new StreamWriter(file);
        stream.Write(connectionString);
        stream.Close();
    }

    /// <summary>
    /// Пытается найти файл с хешем палроля и вернуть его
    /// </summary>
    /// <returns>хэш пароля</returns>
    static public byte[][] getHashPassword(string file)
    {
        file = Path.Combine(pswdFolder, file);

        if (!File.Exists(file))
            return null;
        try
        {
            var stream = new StreamReader(file);
            var pswdSalt = new byte[2][];
            pswdSalt[0] = new byte[20];
            pswdSalt[1] = new byte[20];

            var symbols = stream.ReadLine().Split();
            for (var i = 0; i < 20; i++)
            {
                pswdSalt[0][i] = Convert.ToByte(symbols[i]);
            }
            for (var i = 0; i < 20; i++)
            {
                pswdSalt[1][i] = Convert.ToByte(symbols[20 + i]);
            }

            stream.Close();
            return pswdSalt;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Get pswd hash error");
        }
        return null;
    }

    /// <summary>
    /// задает новый хэш пароля
    /// </summary>
    /// <param name="pswd">хэш пароля</param>
    static public void setHashPassword(string file, byte[] pswd, byte[] salt)
    {
        if (!Directory.Exists(pswdFolder))
        {
            var di = Directory.CreateDirectory(pswdFolder);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        }

        file = Path.Combine(pswdFolder, file);

        try
        {
            var stream = new StreamWriter(file);

            for (var i = 0; i < 20; i++)
            {
                stream.Write(pswd[i] + " ");
            }

            for (var i = 0; i < 20; i++)
            {
                stream.Write(salt[i] + " ");
            }

            //stream.Write(Encoding.ASCII.GetString(pswd));
            //stream.Write(Encoding.ASCII.GetString(salt));
            stream.Close();

        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Set pswd hash error");
        }
    }

    /// <summary>
    /// Сохраняет текущую конфигурацию в файл
    /// </summary>
    /// <param name="filepath">путь к файлу</param>
    /// <param name="siloses">архитектура силосов</param>
    /// <param name="devices">архитектура подвесок</param>
    /// <returns>удачность</returns>
    static public bool serialize(string filepath, Dictionary<int, Silos> siloses, Dictionary<int, Dictionary<int, Wire>> devices, Dictionary<int, StructureSubdivision> subdivs)
    {
        try
        {
            var
                result = new Tuple<Dictionary<int, Silos>, Dictionary<int, Dictionary<int, Wire>>, Dictionary<int, StructureSubdivision>>
                (siloses, devices, subdivs);
            //KeyValuePair <, > pair =
            //new KeyValuePair<Dictionary<int, Silos>, Dictionary<int, Dictionary<int, Wire>>>(siloses, devices);

            using (var fs = new FileStream(filepath, FileMode.OpenOrCreate))
            {
                jsonFormatter.WriteObject(fs, result);
            }
        }
        catch (Exception e)
        {
            MyLoger.Log(e, "FPS: serialize error");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Загружает конфигурацию силосов и плат из файла
    /// </summary>
    /// <param name="filename">путь к файлу</param>
    /// <param name="siloses">архитектура силосов</param>
    /// <param name="devices">архитектура подвесок</param>
    /// <returns>удачность</returns>
    static public bool deserialize(string filename, out Dictionary<int, Silos> siloses, out Dictionary<int, Dictionary<int, Wire>> devices, out Dictionary<int, StructureSubdivision> subdivs)
    {
        siloses = null;
        devices = null;
        subdivs = null;
        try
        {
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var
                    result =
                    (Tuple<Dictionary<int, Silos>, Dictionary<int, Dictionary<int, Wire>>, Dictionary<int, StructureSubdivision>>)
                    jsonFormatter.ReadObject(fs);
                siloses = result.Item1;
                devices = result.Item2;
                subdivs = result.Item3;

                foreach (var device in devices.Values)
                    foreach (var ww in device.Values)
                        if (siloses.ContainsKey(ww.SilosId))
                            siloses[ww.SilosId].Wires[ww.Id] = ww;

            }
        }
        catch (Exception e)
        {
            MyLoger.Log(e, "FPS: deserialize error");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Записывает заначение температуры, снятые с определенной платы с определенной ножки, в файл
    /// </summary>
    /// <param name="filename">путь к файлу</param>
    /// <param name="deviceAddress">адрес платы</param>
    /// <param name="leg">номер ножки подвески</param>
    /// <param name="temps">значение температур</param>
    /// <param name="time">время</param>
    /// <returns>удачность</returns>
    static public bool writeTemperature(string filename, byte deviceAddress, ushort leg, float[] temps, DateTime time)
    {
        var formatTime = time.ToString("yyyy-MM-dd H:mm:ss");
        try
        {
            using (var fs = new StreamWriter(filename, true))
            {
                var str = deviceAddress + " " + leg + " " + time + " " + temps.Length;
                for (var i = 0; i < temps.Length; i++)
                    str += " " + temps[i];
                //str += "\n";
                fs.WriteLine(str);
            }
            return true;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "FPS: write temp error");
            return false;
        }
    }

    /// <summary>
    /// Читает файл с температурами и возвращает значения.
    /// </summary>
    /// <param name="filename">путь к файлу</param>
    /// <returns>Лист с парами значений, 1)адрес платы и номер ножки подвески;
    /// 2)время и значения температур</returns>
    static public List<KeyValuePair<KeyValuePair<byte, ushort>, KeyValuePair<DateTime, float[]>>> readTemperatures(string filename)
    {
        //string formatTime = time.ToString("yyyy-MM-dd H:mm:ss");
        var result =
            new List<KeyValuePair<KeyValuePair<byte, ushort>, KeyValuePair<DateTime, float[]>>>();
        try
        {
            using (var fs = new StreamReader(filename))
            {
                byte deviceAddress; ushort leg; float[] temps; DateTime time; int tempCount;
                string[] splitedString;
                var str = "";
                str = fs.ReadLine();
                while (str != null)
                {

                    splitedString = str.Split();
                    deviceAddress = Convert.ToByte(splitedString[0]);
                    leg = Convert.ToUInt16(splitedString[1]);
                    time = Convert.ToDateTime(splitedString[2] + " " + splitedString[3]);
                    tempCount = Convert.ToInt32(splitedString[4]);

                    temps = new float[tempCount];
                    for (var i = 0; i < tempCount; i++)
                    {
                        temps[i] = Convert.ToSingle(splitedString[5 + i]);
                    }

                    result.Add(new KeyValuePair<KeyValuePair<byte, ushort>, KeyValuePair<DateTime, float[]>>(
                        new KeyValuePair<byte, ushort>(deviceAddress, leg),
                        new KeyValuePair<DateTime, float[]>(time, temps)));

                    str = fs.ReadLine();
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "FPS: readTempError");
            return null;
        }

    }

    /// <summary>
    /// Возвращает список получателей рассылки
    /// </summary>
    /// <param name="filename">путь к файлу</param>
    /// <returns>Нулевой массив - почты, Первый - заметки</returns>
    public static string[][] getDestinations(string filename)
    {
        createSettingsFolderIfNotExists();
        filename = Path.Combine(settingsFolder, filename);

        var result = new string[2][];
        result[0] = new string[0];
        result[1] = new string[0];
        try
        {
            using (var fs = new StreamReader(filename))
            {
                var str = fs.ReadLine();
                if (str == null)
                    return result;

                var count = Convert.ToInt32(str);
                result[0] = new string[count];
                result[1] = new string[count];

                str = fs.ReadLine();
                string[] splitedString;
                var idx = 0;
                while (str != null)
                {

                    splitedString = str.Split();
                    result[0][idx] = splitedString[0];
                    if (splitedString.Length == 2)
                        result[1][idx] = splitedString[1];
                    else
                        result[1][idx] = "";

                    idx++;
                    str = fs.ReadLine();
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            result[0] = new string[0];
            result[1] = new string[0];
            return result;
        }
    }

    /// <summary>
    /// Задает список получателей рассылки
    /// </summary>
    /// <param name="filename">путь к файлу</param>
    /// <param name="destinations">получатели рассылки, нулевой массив - получатели
    /// первый - заметки</param>
    public static void setDestinations(string filename, string[][] destinations)
    {
        createSettingsFolderIfNotExists();
        filename = Path.Combine(settingsFolder, filename);
        try
        {
            using (var fs = new StreamWriter(filename, false))
            {
                fs.WriteLine(destinations[0].Length);
                for (var i = 0; i < destinations[0].Length; i++)
                    fs.WriteLine(destinations[0][i] + " " + destinations[1][i]);
            }
        }
        catch (Exception ex)
        {
        }
    }

    /// <summary>
    /// Читает из файла первую строку
    /// </summary>
    /// <param name="filename">файл</param>
    /// <returns>строка</returns>
    public static string getStringFromFile(string filename)
    {
        createSettingsFolderIfNotExists();
        filename = Path.Combine(settingsFolder, filename);
        var result = "";
        try
        {
            using (var fs = new StreamReader(filename))
            {
                var str = fs.ReadLine();
                if (str != null)
                    result = str;
            }
            return result;
        }
        catch (Exception ex)
        {
            return result;
        }
    }

    /// <summary>
    /// Записывает в файл строку, стирает предыдущую
    /// </summary>
    /// <param name="filename">файл</param>
    /// <param name="str">строка</param>
    public static void setStringToFile(string filename, string str)
    {
        createSettingsFolderIfNotExists();
        filename = Path.Combine(settingsFolder, filename);
        try
        {
            using (var fs = new StreamWriter(filename, false))
            {
                fs.WriteLine(str);
            }
        }
        catch (Exception ex)
        {
        }
    }

}
