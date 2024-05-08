using System;
using System.Collections.Generic;
using System.Data;
using SystemOfTermometry2.DAO;
using SystemOfTermometry2.Model;

namespace SystemOfThermometry2.DAO;

partial class MySQLDAO : Dao
{
    public override int addSilos(Silos silos)
    {
        string x = silos.X.ToString().Replace(',', '.');
        string y = silos.Y.ToString().Replace(',', '.');
        string max = silos.Max.ToString().Replace(',', '.');
        string mid = silos.Mid.ToString().Replace(',', '.');
        string min = silos.Min.ToString().Replace(',', '.');
        string red = silos.Red.ToString().Replace(',', '.');
        string yellow = silos.Yellow.ToString().Replace(',', '.');
        string query = String.Format("INSERT INTO silos" +
            "(name, max, mid, min, red, yellow, structure_id, x, y, w, h, shape, id_grainid)" +
        " VALUES (\'{0}\', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12});", 
        silos.Name, max, mid, min, red, yellow, silos.StructureId, x, y, silos.W, silos.H, (int)silos.Shape, silos.GrainId);
        
        return (int)executeInsertQuery(query);
    }

    private string replace(string s)
    {
        return s.Replace(',', '.');
    }

    public override bool updateSilos(Silos silos)
    {
        string x = replace(silos.X.ToString());
        string y = replace(silos.Y.ToString());
        string max = replace(silos.Max.ToString());
        string mid = replace(silos.Mid.ToString());
        string min = replace(silos.Min.ToString());
        string red = replace(silos.Red.ToString());
        string yellow = replace(silos.Yellow.ToString());

        string query = String.Format("UPDATE silos SET" +
            " name = \'{1}\', max = {2}, mid = {3}, min = {4}, red = {5} , yellow = {6}," +
            " structure_id = {7}, x = {8}, y = {9}, w = {10}, h = {11}, shape = {12}, id_grainid ={13}" +
            " WHERE id = {0};",
        silos.Id, silos.Name, max, mid, min, red, yellow, silos.StructureId, x, y, silos.W, silos.H, (int)silos.Shape, silos.GrainId);

        return executeUpdateQuery(query);
    }

    public override bool deleteSilos(int silosId)
    {
        return executeUpdateQuery("DELETE FROM silos WHERE id = " + silosId + ";");
    }

    private Silos parseSilos(DataTable dataTable, int row)
    {
        Silos s = new Silos();
        s.Id = Convert.ToInt32(dataTable.Rows[row][0]);
        s.Name = Convert.ToString(dataTable.Rows[row][1]);
        s.Max = Convert.ToSingle(dataTable.Rows[row][2]);
        s.Mid = Convert.ToSingle(dataTable.Rows[row][3]);
        s.Min = Convert.ToSingle(dataTable.Rows[row][4]);
        s.Red = Convert.ToSingle(dataTable.Rows[row][5]);
        s.Yellow = Convert.ToSingle(dataTable.Rows[row][6]);
        s.StructureId = Convert.ToInt32(dataTable.Rows[row][7]);
        s.X = Convert.ToSingle(dataTable.Rows[row][8]);
        s.Y = Convert.ToSingle(dataTable.Rows[row][9]);
        s.W = Convert.ToInt32(dataTable.Rows[row][10]);
        s.H = Convert.ToInt32(dataTable.Rows[row][11]);
        s.Shape = (SilosShapeEnum)Convert.ToInt32(dataTable.Rows[row][12]);
        if (dataTable.Rows[row][13] != DBNull.Value)
            s.GrainId = Convert.ToInt32(dataTable.Rows[row][13]);
        else
            s.GrainId = -1;
        return s;
    }

    public override Dictionary<int, Silos> getAllSilos()
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM silos;");
        if (dataTable == null)
            return null;

        Dictionary<int, Silos> result = new Dictionary<int, Silos>();
        try
        {
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                Silos s = parseSilos(dataTable, row);
                result.Add(s.Id, s);
            }
        }
        catch
        {
            return null;
        }

        return result;
    }

    public override Silos getSilos(int id)
    {
        DataTable dataTable = executeSelectQuery("SELECT * FROM silos WHERE id = " + id + ";");
        if (dataTable == null || dataTable.Rows.Count == 0)
            return null;

        try
        {
            Silos s = parseSilos(dataTable, 0);
            return s; 
        }
        catch
        {
            return null;
        }
    }

}
