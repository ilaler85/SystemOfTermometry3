using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.DAO;

// Разбил на три части
// Общая часть, Соединение и запись температуры
partial class MySQLDAO : Dao
{
    private MySqlConnection connection;
    private bool isBusy;
    private object sync = new object();

    public MySQLDAO()
    {
        isBusy = false;
    }

    /// <summary>
    /// Проверка на то, что существуют все необходимые таблицы
    /// </summary>
    /// <returns></returns>
    public override bool DBisCorrect()
    {
        string query = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = \'" + connection.Database + "\'";
        DataTable res = executeSelectQuery(query);
        HashSet<string> needTables = new HashSet<string>() { "silos", "wire", "settings", "sensor_temp", "subdivision", "sensor_temp_last" , "grain" , "historyfilling" };
        HashSet<string> tables = new HashSet<string>();

        // пытаемся добавить таблиц, которые были добавлены позже
        for (int i = 0; i < res.Rows.Count; i++)
        {
            string tableName = res.Rows[i]["TABLE_NAME"].ToString();
            tables.Add(tableName);
        }

        foreach (string tableName in needTables)
        {
            if (!tables.Contains(tableName))
            {
                switch(tableName)
                {
                    case "subdivision":
                        executeUpdateQuery(QueryHolder.getCreateSubdivisionTableQuery());
                        break;
                    case "sensor_temp_last":
                        executeUpdateQuery(QueryHolder.getCreateLastTemperatureTableQuery());
                        break;
                    case "grain":
                        executeUpdateQuery(QueryHolder.getGrainQuery());
                        break;
                    case "historyfilling":
                        executeUpdateQuery(QueryHolder.getHistoryFilling());
                        break;
                    default: break;
                }
            }
        }   

        res = executeSelectQuery(query);
        for (int i = 0; i < res.Rows.Count; i++)
        {
            string tableName = res.Rows[i]["TABLE_NAME"].ToString();
            if (needTables.Contains(tableName))
                needTables.Remove(tableName);
        }

        if (needTables.Count != 0)//Все плохо, надо пересоздавать бд          
            return false;

        return true;
    }

    //Парсит строку и создает базу данных, если ее не существует
    private bool createBDIfNotExist(string connectProperties)
    {
        MySqlConnection tmpConnection = null;
        try
        {
            string connectPropertiesWithoutDB = "";
            string dataBaseName = "";
#pragma warning disable IDE0007 // Использование неявного типа
            string[] splitedStr = connectProperties.Split(';');
#pragma warning restore IDE0007 // Использование неявного типа
            for (int i = 0; i < splitedStr.Length; i++)
            {
                if (splitedStr[i].Length < 0)
                    continue;
                else if (splitedStr[i][0] != 'd') //database==...
                    connectPropertiesWithoutDB += splitedStr[i] + ";";
                else
                    dataBaseName = splitedStr[i].Split('=')[1];
            }
            tmpConnection = new MySqlConnection(connectPropertiesWithoutDB);
            MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + dataBaseName + ";", tmpConnection);
            tmpConnection.Open();
            int res = cmd.ExecuteNonQuery();
            cmd = new MySqlCommand(QueryHolder.getCreateUserQuery(dataBaseName), tmpConnection);
            res = cmd.ExecuteNonQuery();
            tmpConnection.Close();
            return true;
        }
        catch (Exception e)
        {
            MyLoger.LogError(e.Message);
            if (tmpConnection != null)
                tmpConnection.Close();
            return false;
        }
    }

    public override bool connectToDB(string connectProperties, bool OnlyView = false)
    {
        try
        {
            //Удаляем подключение, если оно было
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }

            //Создаем бд, если не существует и если не только простмотра
            if (!OnlyView)
                if (!createBDIfNotExist(connectProperties)) return false;

            connection = new MySqlConnection(connectProperties);

            /*
            if (!executeUpdateQuery(QueryHolder.getCheckDropTable())) return false;
            if (!executeUpdateQuery(QueryHolder.getCheckCreateTable())) return false;
            if (executeInsertQuery(QueryHolder.getCheckInsert()) == -1) return false;
            if (executeSelectQuery(QueryHolder.getCheckSelect()) == null) return false;
            if (!executeUpdateQuery(QueryHolder.getCheckUpdate())) return false;
            if (!executeUpdateQuery(QueryHolder.getCheckDropTable())) return false;
            */

            return true;
        }
        catch (Exception e)
        {
            MyLoger.Log("DAO Error:" + e.Message);
            return false;
        }
    }

    public override bool dropAndCreateModel()
    {
        string dropQuery;

        dropQuery = "DROP TABLE IF EXISTS silos;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS wire;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS settings;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS subdivision;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS sensor_temp_last;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS grain;";
        if (!executeUpdateQuery(dropQuery)) return false;


        string createQuery;
        createQuery = QueryHolder.getCteateSilosTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateWireTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateSettingsTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateSubdivisionTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateLastTemperatureTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getGrainQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        return true;
    }

    public override bool dropAndCreateDB()
    {
        string dropQuery;

        dropQuery = "DROP TABLE IF EXISTS silos;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS silos_wire;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS wire;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS sensor_temp;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS settings;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS subdivision;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS sensor_temp_last;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS grain;";
        if (!executeUpdateQuery(dropQuery)) return false;

        dropQuery = "DROP TABLE IF EXISTS historyfilling;";
        if (!executeUpdateQuery(dropQuery)) return false;

        string createQuery;
        createQuery = QueryHolder.getCteateSilosTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateWireTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateTemperatureTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateSettingsTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateSubdivisionTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getCreateLastTemperatureTableQuery();
        if (!executeUpdateQuery(createQuery)) return false;
        //добавлено новое 
        createQuery = QueryHolder.getGrainQuery();
        if (!executeUpdateQuery(createQuery)) return false;

        createQuery = QueryHolder.getHistoryFilling();
        if (!executeUpdateQuery(createQuery)) return false;

        return true;
    }

    public override bool dropTemperatureTable()
    {
        string query = "DROP TABLE IF EXISTS sensor_temp;";
        if (!executeUpdateQuery(query)) return false;

        query = "DROP TABLE IF EXISTS sensor_temp_last;";
        if (!executeUpdateQuery(query)) return false;

        query = QueryHolder.getCreateTemperatureTableQuery();
        if (!executeUpdateQuery(query)) return false;

        query = QueryHolder.getCreateLastTemperatureTableQuery();
        if (!executeUpdateQuery(query)) return false;

        return true;
    }

    public override bool addUser(string dataBaseName)
    {
        return executeUpdateQuery(QueryHolder.getCreateUserQuery(dataBaseName));
    }

    public override bool addSettingRecord(string key, string value)
    {
        value = QueryHolder.convertStringToWrite(value);
        string query = String.Format("INSERT INTO settings " +
            "(name, value) VALUES (\'{0}\', \'{1}\');", key, value);
        return executeUpdateQuery(query);
    }

    public override bool getSettingRecord(string key, ref string value)
    {
        string query = String.Format("SELECT * FROM settings WHERE name = '" + key + "';");
        DataTable res = executeSelectQuery(query);
        if (res == null || res.Rows.Count == 0)
            return false;

        value = QueryHolder.convertStringFromDB(res.Rows[0][1].ToString());
        //value = res.Rows[0][1].ToString();
        
        return true;
    }

    public override bool updateSettingRecord(string key, string value)
    {
        value = QueryHolder.convertStringToWrite(value);
        string query = String.Format("UPDATE settings SET" +
            " value = \'{0}\' WHERE name = \'{1}\';", value, key);
        return executeUpdateQuery(query);
    }

    protected override DataTable executeSelectQuery(string sqlQuery)
    {
        if (connection == null)
            return null;

        lock (sync)
        {
            while (isBusy) //Спин блокировка
            {
                Thread.Sleep(201);
            }
            isBusy = true;

            DataTable result = new DataTable();
            MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
            try
            {
                connection.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                result.Load(reader);
                reader.Close();
                connection.Close();
                return result;
            }
            catch (MySqlException e)
            {
                //MessageBox.Show("Ошибка базы данных");
                //InsertLogErr("Не удалось получить информацию о силосах!", DateTime.Now);
                MyLoger.Log(e, "Query = " + sqlQuery);
                connection.Close();
                return null;
            }
            catch (Exception e)
            {
                MyLoger.Log(e, "Query = " + sqlQuery);
                connection.Close();
                return null;
            }
            finally
            {
                isBusy = false;
                connection.Close();
            }
        }
    }

    protected override bool executeUpdateQuery(string sqlQuery)
    {
        if (connection == null)
            return false;
        lock (sync)
        {
            while (isBusy) //Спин блокировка
            {
                Thread.Sleep(201);
            }
            isBusy = true;

            MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
            try
            {
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch (MySqlException e)
            {
                MyLoger.Log(e, "Query = " + sqlQuery);
                connection.Close();
                return false;
            }
            catch (Exception e)
            {
                MyLoger.Log(e, "Query = " + sqlQuery);
                connection.Close();
                return false;
            }
            finally
            {
                isBusy = false;
                //connection.Close();
            }
        }
    }

    protected override long executeInsertQuery(string sqlQuery)
    {
        if (connection == null)
            return -1;

        lock (sync)
        {

            while (isBusy) //Спин блокировка
            {
                Thread.Sleep(201);
            }
            isBusy = true;

            MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
            try
            {
                connection.Open();
                int res = cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
            catch (MySqlException e)
            {
                MyLoger.Log(e, "Query = " + sqlQuery);
                return -1;
            }
            catch (Exception e)
            {
                MyLoger.Log(e, "Query = " + sqlQuery);
                return -1;
            }
            finally
            {
                isBusy = false;
                connection.Close();
            }
        }
    }
}
