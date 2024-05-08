using Google.Protobuf;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using SystemOfTermometry2.DAO;
using SystemOfTermometry2.Model;

namespace SystemOfThermometry2.DAO;

partial class MySQLDAO : Dao
{
    public override int addGrain(Grain grain)
    {
        
        string red = grain.RedTemp.ToString().Replace(',', '.');
        string yellow = grain.YellowTemp.ToString().Replace(',', '.');
        string value = QueryHolder.convertStringToWrite(grain.Name);
        string query = String.Format("INSERT INTO grain (nameGrain, yellowTemp, redTemp) VALUES (\'{0}\', {1}, {2});",
        value, yellow, red);
        int res = (int)executeInsertQuery(query);
        return res;
    }

    public override bool updateGrain(Grain grain)
    {
        
        string red = grain.RedTemp.ToString().Replace(',', '.');
        string yellow = grain.YellowTemp.ToString().Replace(',', '.');
        string value = QueryHolder.convertStringToWrite(grain.Name);
        
        string query = String.Format("UPDATE grain SET" +
            " nameGrain = \"{1}\",redTemp = {2} , yellowTemp = {3}" +
            " WHERE id = {0};",

        grain.ID, value, red, yellow);

        return executeUpdateQuery(query);
    }

    public override bool deleteGrain(int grainId)
    {
        return executeUpdateQuery("DELETE FROM grain WHERE id = " + grainId + ";");
    }

    private Grain parserGrain(DataTable data, int row)
    {
        Grain grain = new Grain();
        grain.ID = Convert.ToInt32(data.Rows[row][0]);
        string value = QueryHolder.convertStringFromDB(data.Rows[row][1].ToString());
        grain.Update(value, Convert.ToSingle(data.Rows[row][3]), Convert.ToSingle(data.Rows[row][2]));

        return grain;
    }

    public override Dictionary<int, Grain> getAllGrains()
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM grain;");
        if (dataTable == null)
            return null;

        Dictionary<int, Grain> result = new Dictionary<int, Grain>();
        try
        {
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                Grain g = parserGrain(dataTable, row);
                result.Add(g.ID, g);
            }
        }
        catch
        {
            return null;
        }

        return result;
    }

    public override Grain getGrain(int id)
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM grain WHERE id = " + id + ";");
        if (dataTable == null || dataTable.Rows.Count == 0)
            return null;

        try
        {
            Grain s = parserGrain(dataTable, 0);
            return s;
        }
        catch
        {
            return null;
        }
    }
}
