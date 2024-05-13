using System;
using System.Collections.Generic;
using System.Data;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Model;

namespace SystemOfThermometry3.DAO;

partial class MySQLDAO : Dao
{
    public override int addWire(Wire wire)
    {
        string x = wire.X.ToString().Replace(',', '.');
        string y = wire.Y.ToString().Replace(',', '.');
        string query = String.Format("INSERT INTO wire " +
            " (number, silos_id, device_address, leg, sensor_count, enable, provider, x, y)" +
            " VALUES ({0}, {1}, {2}, {3}, {4}, {5}, \'{6}\', {7}, {8});",
            wire.Number, wire.SilosId, wire.DeviceAddress, wire.Leg, wire.SensorCount, wire.Enable, wire.Type.ToString(), x, y);
        
        return (int)executeInsertQuery(query);
    }

    public override bool updateWire(Wire wire)
    {
        string x = wire.X.ToString().Replace(',', '.');
        string y = wire.Y.ToString().Replace(',', '.');
        string query = String.Format(
            "UPDATE wire SET " +
            " number = {1}, silos_id = {2}, device_address = {3}, leg = {4}, sensor_count = {5}, enable = {6}, provider = \'{7}\', x = {8}, y = {9}" +
            " WHERE id = {0};",
            wire.Id, wire.Number, wire.SilosId, wire.DeviceAddress, wire.Leg, wire.SensorCount, wire.Enable, wire.Type.ToString(), x, y);
            
        return executeUpdateQuery(query);
    }


    public override bool deleteWire(int wireId)
    {
        return executeUpdateQuery("DELETE FROM wire WHERE id = " + wireId + ";");
    }


    private Wire parseWire(DataTable dataTable, int row)
    {
        Wire w = new Wire();
        w.Id = Convert.ToInt32(dataTable.Rows[row][0]);
        w.Number = Convert.ToInt32(dataTable.Rows[row][1]);
        w.SilosId = Convert.ToInt32(dataTable.Rows[row][2]);
        w.DeviceAddress = Convert.ToByte(dataTable.Rows[row][3]);
        w.Leg = Convert.ToUInt16(dataTable.Rows[row][4]);
        w.SensorCount = Convert.ToUInt16(dataTable.Rows[row][5]);
        w.Enable = Convert.ToBoolean(dataTable.Rows[row][6]);

        string type = Convert.ToString(dataTable.Rows[row][7]);
        WireTypeEnum en = WireTypeEnum.TOP_TO_BOT_DS18b20;
        Enum.TryParse<WireTypeEnum>(type, out en);
        w.Type = en;

        w.X = Convert.ToSingle(dataTable.Rows[row][8]);
        w.Y = Convert.ToSingle(dataTable.Rows[row][9]);

        return w;
    }

    public override Dictionary<int, Wire> getAllWires()
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM wire;");
        if (dataTable == null)
            return null;

        Dictionary<int, Wire> result = new Dictionary<int, Wire>();
        try
        {
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                Wire s = parseWire(dataTable, row);
                result.Add(s.Id, s);
            }
        }
        catch
        {
            return null;
        }

        return result;
    }

    public override Wire getWire(int id)
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM wire WHERE id = " + id + ";");
        if (dataTable == null || dataTable.Rows.Count == 0)
            return null;

        try
        {
            Wire s = parseWire(dataTable, 0);
            return s;
        }
        catch
        {
            return null;
        }
    }

    public override Dictionary<int, Wire> getWireForSilos(int silosId)
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM wire WHERE silos_id = " + silosId + ";");
        if (dataTable == null)
            return null;

        Dictionary<int, Wire> result = new Dictionary<int, Wire>();
        try
        {
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                Wire w = parseWire(dataTable, row);
                result.Add(w.Id, w);
            }
        }
        catch
        {
            return null;
        }

        return result;
    }
}
