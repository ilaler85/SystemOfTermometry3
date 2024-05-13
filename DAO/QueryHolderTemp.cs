using System;
using SystemOfThermometry3.Model;

namespace SystemOfThermometry3.DAO;

partial class QueryHolder
{
    public static string getLastTempForWireQuery(Wire wire)
    {
        /* old
        string query = String.Format("Select temperature, sensor_index from sensor_temp" +
            " WHERE device_addres = {0} AND leg = {1} AND time in " +
            "(Select MAX(time) from sensor_temp where device_addres = {0} AND leg = {1});", wire.DeviceAddress, wire.Leg);
            */
        string query = String.Format("Select temperature, sensor_index, time " +
            " from sensor_temp_last" +
            " WHERE device_addres = {0} AND leg = {1};", wire.DeviceAddress, wire.Leg);

        return query;
    }

    public static string getAddTemperaturesQuery(Wire wire, float[] temps, DateTime time)
    {
        string formatTime = time.ToString("yyyy-MM-dd H:mm:ss");
        string values = "";
        for (int i = 0; i < temps.Length - 1; i++)
        {
            values += String.Format("({0}, {1}, {2}, {3}, \'{4}\'), ", wire.DeviceAddress, wire.Leg, i, temps[i].ToString().Replace(',', '.'), formatTime);
        }
        values += String.Format("({0}, {1}, {2}, {3}, \'{4}\') ", wire.DeviceAddress, wire.Leg, temps.Length - 1,
            temps[temps.Length - 1].ToString().Replace(',', '.'), formatTime);

        string query = String.Format("INSERT INTO sensor_temp(device_addres, leg, sensor_index, temperature, time)" +
            " VALUES {0}; ", values);
        return query;
    }

    public static string getInsertOrUpdateLastTempQuery(Wire wire, float[] temps, DateTime time)
    {
        string formatTime = time.ToString("yyyy-MM-dd H:mm:ss");
        string values = "";
        for (int i = 0; i < temps.Length - 1; i++)
        {
            values += String.Format("({0}, {1}, {2}, {3}, \'{4}\'), ", wire.DeviceAddress, wire.Leg, i, temps[i].ToString().Replace(',', '.'), formatTime);
        }
        values += String.Format("({0}, {1}, {2}, {3}, \'{4}\') ", wire.DeviceAddress, wire.Leg, temps.Length - 1,
            temps[temps.Length - 1].ToString().Replace(',', '.'), formatTime);

        string query = String.Format("INSERT INTO sensor_temp_last (device_addres, leg, sensor_index, temperature, time)" +
        " VALUES {0} ON DUPLICATE KEY UPDATE temperature = VALUES(temperature), time = VALUES(time);", values);

        return query;
    }



}
