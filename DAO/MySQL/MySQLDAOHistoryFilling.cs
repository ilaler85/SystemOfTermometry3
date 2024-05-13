using OxyPlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using SystemOfThermometry3.DAO;

namespace SystemOfThermometry3.DAO;

partial class MySQLDAO: Dao
{
    public override bool addHistory(int silosID, int filling, int grainID, DateTime time)
    {
        string formatTime = time.ToString("yyyy-MM-dd H:mm:ss");
        string values = String.Format("({0}, {1}, \'{2}\', {3}) ", filling.ToString(), silosID.ToString(), formatTime, grainID.ToString());

        string query = String.Format("INSERT INTO historyfilling(filling, silos_id, dat, id_grain)" +
            " VALUES {0}; ", values);
        return executeUpdateQuery(query); ;
    }

    public override bool addMoreHistory(DateTime time, Dictionary<int, int> silosFilling)
    {
        string values = "";
        foreach(int id in silosFilling.Keys)
        {
            values += String.Format(" ({0}, {1}, \'{2}\', {3}),", silosFilling[id].ToString(),
                id.ToString(), time.ToString("yyyy-MM-dd H:mm:ss"), -1);
        }
        values = values.Remove(values.Length - 1);
        string query = String.Format("INSERT INTO historyfilling(filling, silos_id, dat, id_grain)" +
            " VALUES {0}; ", values);

        return executeUpdateQuery(query);
    }

    private SortedDictionary<DateTime, int> parserFilling(DataTable dt)
    {
        SortedDictionary<DateTime, int > result = new SortedDictionary<DateTime, int>();
        foreach (DataRow row in dt.Rows)
        {
            DateTime time = Convert.ToDateTime(row[0]);
            int filling = Convert.ToInt32(row[1]);
            result.Add(time, filling);
        }
        return result;
    }

    public override SortedDictionary<DateTime, int> getFillingBetweenTimes(DateTime start, DateTime end, int silosId)
    {
        string query = String.Format("SELECT dat, filling " +
            "FROM historyfilling " +
            "WHERE dat BETWEEN \'{0}\' AND \'{1}\' AND silos_id = {2};",start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), silosId.ToString());
        DataTable dt = executeSelectQuery(query);
        //MessageBox.Show(query);
        if(dt != null )
        {
            return parserFilling(dt);
        }
        return new SortedDictionary<DateTime, int>();
    }

    

    public override List<DateTime> getTimesBetweenFromFillingTable(DateTime start)
    {
        List<DateTime> result = new List<DateTime>(); 
        string query = String.Format("SELECT distinct dat " +
            " FROM historyfilling " +
            " WHERE dat>=\'{0}\';", start.ToString("yyyy-MM-dd H:mm:ss"));
        DataTable table = executeSelectQuery(query);
        //обращаемся к методу parseTime в классе MYSQLDAOTemperature так как наследуются от MYSQLDAO
        result = parseTime(table);
        return result;
    }

    public override string[] getFillingAndGrain(int silosId, DateTime time)
    {
        string[] result = new string[2];
        string query = String.Format("SELECT filling, id_grain " +
            "FROM historyfilling " +
            "WHERE silos_id = {0} AND dat = \'{1}\';",
            silosId.ToString(), time.ToString("yyyy-MM-dd H:mm:ss"));
        DataTable table = executeSelectQuery(query);
        if (table == null || table.Rows.Count == 0)
            return null;
        //MessageBox.Show(table.Rows.Count.ToString());
        result[0] = table.Rows[0][0].ToString();
        if (table.Rows[0][1] == DBNull.Value)
            result[1] = "-1"; 
        else
            result[1] = table.Rows[0][1].ToString();

        return result;
    }

    public override DateTime getFillingTime(DateTime time)
    {
        DateTime result;
        string query = String.Format("SELECT MAX(dat) " +
            "FROM historyfilling " +
            "WHERE dat <= \'{0}\';", time.ToString("yyyy-MM-dd H:mm:ss"));
        DataTable table = executeSelectQuery(query);
        result = Convert.ToDateTime(table.Rows[0][0]);
        return result;
    }

    public override bool deleteHistory()
    {
        throw new NotImplementedException();
    }

    public override bool updateHistory()
    {
        throw new NotImplementedException();

    }
}
