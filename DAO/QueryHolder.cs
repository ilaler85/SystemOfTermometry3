using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfThermometry2.DAO;

/// <summary>
/// Статический класс, который содержит в себе некоторые запросы.
/// Может быть полезным, так как почти все запросы в одном месте,
/// а так же они одинаковы для большенства баз данных
/// </summary>
public static partial class QueryHolder
{
    /// <summary>
    /// Запрос на создание таблици силосов
    /// </summary>
    /// <returns></returns>
    static public string getCteateSilosTableQuery()
    {
        return "CREATE TABLE silos ( id INT(32) AUTO_INCREMENT, " +
                                    "name VARCHAR(50) NOT NULL, " +
                                    "max FLOAT, " +
                                    "mid FLOAT, " +
                                    "min FLOAT, " +
                                    "red FLOAT, " +
                                    "yellow FLOAT, " +
                                    "structure_id int, " +
                                    "x FLOAT, " +
                                    "y FLOAT, " +
                                    "w INT(32), " +
                                    "h INT(32), " +
                                    "shape INT(32)," +
                                    "id_grainid INT(11) NULL, " +
                                    "PRIMARY KEY(id));";
    }

    /// <summary>
    /// Зарос на создание таблицы с подвесками
    /// </summary>
    /// <returns></returns>
    static public string getCreateWireTableQuery()
    {
        return "CREATE TABLE wire (  id INT(32) AUTO_INCREMENT, " +
                                    "number INT(32), " +
                                    "silos_id INT(32), " +
                                    "device_address INT(32), " +
                                    "leg INT(32), " +
                                    "sensor_count INT(32), " +
                                    "enable BOOL, " +
                                    "provider VARCHAR(50)," +
                                    "x FLOAT," +
                                    "y FLOAT," +
                                    "PRIMARY KEY(id));";
    }

    /// <summary>
    /// Запрос на создание таблицы со значениями температур
    /// </summary>
    /// <returns></returns>
    static public string getCreateTemperatureTableQuery()
    {
        return "CREATE TABLE sensor_temp (   id INT(64) AUTO_INCREMENT, " +
                                            "device_addres INT(16) NOT NULL, " +
                                            "leg INT(16) NOT NULL, " +
                                            "sensor_index INT(32) NOT NULL, " +
                                            "temperature FLOAT, " +
                                            "time DATETIME, " +
                                            "PRIMARY KEY(id));" +
               "CREATE INDEX leg_index ON sensor_temp(leg);" +
               "CREATE INDEX da_index ON sensor_temp(device_addres);";
    }

    static public string getCreateSubdivisionTableQuery()
    {
        return "CREATE TABLE IF NOT EXISTS subdivision ( id INT(64) AUTO_INCREMENT, " +
                                                        "name VARCHAR(255) NOT NULL, " +
                                                        "PRIMARY KEY(id));";
    }


    /// <summary>
    /// Запрос на создание таблицы с настройками
    /// </summary>
    /// <returns></returns>
    static public string getCreateSettingsTableQuery()
    {
        return "CREATE TABLE settings (  name VARCHAR(255) NOT NULL, " +
                                        "value VARCHAR(255) NOT NULL, " +
                                        "PRIMARY KEY(name));";
    }

    /// <summary>
    /// Запрос на создание таблицы с последними значениями температур
    /// </summary>
    /// <returns></returns>
    static public string getCreateLastTemperatureTableQuery()
    {
        return "CREATE TABLE sensor_temp_last (  device_addres INT(16) NOT NULL, " +
                                                "leg INT(16) NOT NULL, " +
                                                "sensor_index INT(32) NOT NULL, " +
                                                "temperature FLOAT, " +
                                                "time DATETIME, " +
                                                "PRIMARY KEY(device_addres, leg, sensor_index));";
    }


    static public string getGrainQuery()
    {
        return "CREATE TABLE IF NOT EXISTS grain " +
            "( " +
            "id int AUTO_INCREMENT, " +
            "nameGrain varchar(255) NOT NULL, " +
            "yellowTemp float, " +
            "redTemp float, " +
            "PRIMARY KEY(id)" +
            ");"; /*
            "ALTER TABLE historyfilling " +
            "ADD foreign key (silos_id) references silos(id); " +
            "ALTER TABLE historyfilling " +
            "ADD foreign key (id_grain) references grain(id);"*/ 
    }


    static public string getHistoryFilling()
    {
        return "CREATE TABLE IF NOT EXISTS historyfilling " +
            "(" +
            "  filling int(4) NOT NULL," +
            "  silos_id int(32) NOT NULL," +
            " dat datetime NOT NULL," +
            "  id_grain int(11) DEFAULT '-1'," +
            "  PRIMARY KEY (dat,silos_id) " +
            ") ;";


    }


    static public string getCreateUserQuery(string database)
    {
        return string.Format("drop user  if exists 'user'@'%'; " +
            "CREATE USER 'user'@'%' IDENTIFIED BY '12345'; " +
            "GRANT select on {0} . * TO 'user'@'%'; " +
            "FLUSH PRIVILEGES; ", database);
    }

    static public string getCheckCreateTable()
    {
        return "CREATE TABLE check_table ( " +
            "id INT NOT NULL, " +
            "value VARCHAR(255) NOT NULL);";
    }

    static public string getCheckInsert()
    {
        return "INSERT INTO check_table(" +
            "id, value)" +
        " VALUES (1, \'asd\');";
    }

    static public string getCheckSelect()
    {
        return "SELECT * FROM check_table";
    }

    static public string getCheckUpdate()
    {
        return "UPDATE check_table SET" +
            " value = \'qwer\'" +
            " WHERE id = 1;";
    }

    static public string getCheckDelete()
    {
        return "DELETE FROM check_table WHERE id = 1";
    }

    static public string getCheckDropTable()
    {
        return "DROP TABLE IF EXISTS check_table;";
    }


    static Dictionary<int, char> MapToRead = new Dictionary<int, char>()
    {
        { 81,'А' },
        { 82,'Б' },
        { 83,'В' },
        { 84,'Г' },
        { 85,'Д' },
        { 86,'Е' },
        { 87,'Ё' },
        { 88,'Ж' },
        { 89,'З' },
        { 10,'И' },
        { 11,'Й' },
        { 12,'К' },
        { 13,'Л' },
        { 14,'М' },
        { 15,'Н' },
        { 16,'О' },
        { 17,'П' },
        { 18,'Р' },
        { 19,'С' },
        { 20,'Т' },
        { 21,'У' },
        { 22,'Ф' },
        { 23,'Х' },
        { 24,'Ц' },
        { 25,'Ч' },
        { 26,'Ш' },
        { 27,'Щ' },
        { 28,'Ъ' },
        { 29,'Ы' },
        { 30,'Ь' },
        { 31,'Э' },
        { 32,'Ю' },
        { 33,'Я' },

        { 41,'а' },
        { 42,'б' },
        { 43,'в' },
        { 44,'г' },
        { 45,'д' },
        { 46,'е' },
        { 47,'ё' },
        { 48,'ж' },
        { 49,'з' },
        { 50,'и' },
        { 51,'й' },
        { 52,'к' },
        { 53,'л' },
        { 54,'м' },
        { 55,'н' },
        { 56,'о' },
        { 57,'п' },
        { 58,'р' },
        { 59,'с' },
        { 60,'т' },
        { 61,'у' },
        { 62,'ф' },
        { 63,'х' },
        { 64,'ц' },
        { 65,'ч' },
        { 66,'ш' },
        { 67,'щ' },
        { 68,'ъ' },
        { 69,'ы' },
        { 70,'ь' },
        { 71,'э' },
        { 72,'ю' },
        { 73,'я' }
    };
    static Dictionary<char, int> MapToWrite = new Dictionary<char, int>()
    {
        {'А',  81},
        {'Б',  82},
        {'В',  83},
        {'Г',  84},
        {'Д',  85},
        {'Е',  86},
        {'Ё',  87},
        {'Ж',  88},
        {'З',  89},
        {'И',  10},
        {'Й',  11},
        {'К',  12},
        {'Л',  13},
        {'М',  14},
        {'Н',  15},
        {'О',  16},
        {'П',  17},
        {'Р',  18},
        {'С',  19},
        {'Т',  20},
        {'У',  21},
        {'Ф',  22},
        {'Х',  23},
        {'Ц',  24},
        {'Ч',  25},
        {'Ш',  26},
        {'Щ',  27},
        {'Ъ',  28},
        {'Ы',  29},
        {'Ь',  30},
        {'Э',  31},
        {'Ю',  32},
        {'Я',  33},

        {'а',  41},
        {'б',  42},
        {'в',  43},
        {'г',  44},
        {'д',  45},
        {'е',  46},
        {'ё',  47},
        {'ж',  48},
        {'з',  49},
        {'и',  50},
        {'й',  51},
        {'к',  52},
        {'л',  53},
        {'м',  54},
        {'н',  55},
        {'о',  56},
        {'п',  57},
        {'р',  58},
        {'с',  59},
        {'т',  60},
        {'у',  61},
        {'ф',  62},
        {'х',  63},
        {'ц',  64},
        {'ч',  65},
        {'ш',  66},
        {'щ',  67},
        {'ъ',  68},
        {'ы',  69},
        {'ь',  70},
        {'э',  71},
        {'ю',  72},
        {'я',  73}

    };

    public static string convertStringToWrite(string s)
    {

        string result = "";
        for (int i = 0; i < s.Length; i++)
        {
            if (result.Length > 250)
                return result;

            if (MapToWrite.ContainsKey(s[i]))
            {
                result += "/" + MapToWrite[s[i]];
            }
            else
            {
                result += s[i];
            }
        }

        return result;
    }

    public static string convertStringFromDB(string s)
    {
        string result = "";
        for (int i = 0; i < s.Length; i++)
        {
            
            if (s[i] == '/' && i < s.Length - 2)
            {
                try
                {
                    string toIntConver = "" + s[i + 1] + s[i + 2];
                    int id = Convert.ToInt32(toIntConver);
                    if (MapToRead.ContainsKey(id))
                    {
                        result += MapToRead[id];
                        i += 2;
                    }
                    else
                    {
                        result += s[i];
                    }
                }
                catch
                {

                }
            }
            else
            {
                result += s[i];
            }
        }

        return result;
    }
}
