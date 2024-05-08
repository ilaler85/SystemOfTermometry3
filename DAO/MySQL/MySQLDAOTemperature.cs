using System;
using System.Collections.Generic;
using System.Data;
using SystemOfTermometry2.DAO;
using SystemOfTermometry2.Model;
using SystemOfTermometry2.Services;

namespace SystemOfThermometry2.DAO
{
    partial class MySQLDAO : Dao
    {
        private Dictionary<int, string> partition = new Dictionary<int, string> { { 2022, "p2022" }, { 2023, "p2023" }, { 2024, "p2024" } };
        private string getPartition(DateTime start, DateTime end)
        {
            //return "p2022, p2023, p2024, pMAXVALUE";
            string result = "";
            for(int i= start.Year; i<=end.Year;++i)
            {
                if(partition.ContainsKey(i))
                    result+= partition[i]+", ";
            }

            if (result != "")
                result = result.Remove(result.LastIndexOf(','));
            else
                return "pMAXVALUE";
           
            return result;
        }

        public override float[] getLastTempForWire(Wire wire)
        {
            /*string query = String.Format("Select temperature, sensor_index from sensor_temp" +
                " WHERE device_addres = {0} AND leg = {1} AND time in " +
                "(Select MAX(time) from sensor_temp where device_addres = {0} AND leg = {1});", wire.DeviceAddress, wire.Leg);
                */
            string query = QueryHolder.getLastTempForWireQuery(wire);

            DataTable resultTable = executeSelectQuery(query);
            if (resultTable == null || resultTable.Rows.Count == 0)
                return null;

            float[] result = new float[wire.SensorCount];
            for (int row = 0; row < wire.SensorCount; row++) // Заполнение нереальными значениями            
                result[row] = -99;


            for (int row = 0; row < resultTable.Rows.Count; row++)
            {
                ushort sensor_idx = Convert.ToUInt16(resultTable.Rows[row][1]);
                float temp = Convert.ToSingle(resultTable.Rows[row][0]);
                if (sensor_idx < wire.SensorCount && sensor_idx >= 0)
                {
                    result[sensor_idx] = temp;
                }
            }
            return result;
        }

        public override float[] getLastTempForWire(Wire wire, ref DateTime time)
        {
            /*
            string query = String.Format("Select temperature, sensor_index, time from sensor_temp" +
                " WHERE device_addres = {0} AND leg = {1} AND time in " +
                "(Select MAX(time) from sensor_temp where device_addres = {0} AND leg = {1});", wire.DeviceAddress, wire.Leg);
            */
            string query = QueryHolder.getLastTempForWireQuery(wire);

            //if (time == new DateTime())
            //time = ;

            DataTable resultTable = executeSelectQuery(query);
            if (resultTable == null || resultTable.Rows.Count == 0)
                return null;

            float[] result = new float[wire.SensorCount];
            for (int row = 0; row < wire.SensorCount; row++) // Заполнение нереальными значениями            
                result[row] = -99;

            for (int row = 0; row < resultTable.Rows.Count; row++)
            {
                time = Convert.ToDateTime(resultTable.Rows[row][2]);
                ushort sensor_idx = Convert.ToUInt16(resultTable.Rows[row][1]);
                float temp = Convert.ToSingle(resultTable.Rows[row][0]);
                if (sensor_idx < wire.SensorCount && sensor_idx >= 0)
                {
                    result[sensor_idx] = temp;
                }
            }
            return result;
        }

        public override DateTime getTime (ref DateTime dateTime)
        {
            string partitions = getPartition(dateTime, dateTime);
            string query = String.Format("SELECT MAX(time) FROM sensor_temp PARTITION ({1}) " +
                                         "WHERE time <= \'{0}\';",
            dateTime.ToString("yyyy-MM-dd HH:mm:ss"), partitions);

            DataTable resultTable = executeSelectQuery(query);
            if (resultTable.Rows[0][0] == DBNull.Value)
                return DateTime.MinValue;

            DateTime tmp = Convert.ToDateTime(resultTable.Rows[0][0]);
            return tmp;
            
            
        }



        public override float[] getUpperBoundTempTempForWire(Wire wire, ref DateTime time)
        {
            string partitioms = getPartition(time, time);
            string query = String.Format(
            "SELECT temperature, sensor_index" +
            " FROM sensor_temp PARTITION ({3}) " +
            " WHERE device_addres = {0} AND leg = {1} " +
            "AND time = \'{2}\'",
            wire.DeviceAddress, wire.Leg, time.ToString("yyyy-MM-dd H:mm:ss"), partitioms);

            /*string query = String.Format(
            "SELECT temperature, sensor_index, time " +
            " FROM sensor_temp" +
            " WHERE device_addres = {0} AND leg = {1} " +
            "AND time = (SELECT MAX(time) " +
                        "FROM sensor_temp " +
                        "WHERE device_addres = {0} AND leg = {1} AND time <= \'{2}\');",
            wire.DeviceAddress, wire.Leg, time.ToString("yyyy-MM-dd H:mm:ss"));*/
            
            
            DataTable resultTable = executeSelectQuery(query);
            //MessageBox.Show(query);
            //MessageBox.Show(resultTable.Rows.Count.ToString());
            if (resultTable == null || resultTable.Rows.Count == 0)
                return null;

            float[] result = new float[wire.SensorCount];
            for (int row = 0; row < wire.SensorCount; row++) // Заполнение нереальными значениями            
                result[row] = (float)-100.0;

            for (int row = 0; row < resultTable.Rows.Count; row++)
            {
                //time = Convert.ToDateTime(resultTable.Rows[row][2]);
                ushort sensor_idx = Convert.ToUInt16(resultTable.Rows[row][1]);
                float temp = Convert.ToSingle(resultTable.Rows[row][0]);
                if (sensor_idx < wire.SensorCount && sensor_idx >= 0)
                {
                    result[sensor_idx] = temp;
                }
            }
            return result;
        }

        public override bool addTemperatures(Wire wire, float[] temps, DateTime time)
        {

            string query = QueryHolder.getAddTemperaturesQuery(wire, temps, time);
            bool ok = executeUpdateQuery(query);
            if (!ok) return false;
            //обновление температуры для таблицы последних температур
            query = QueryHolder.getInsertOrUpdateLastTempQuery(wire, temps, time);
            return executeUpdateQuery(query);
        }

        public override Dictionary<Wire, SortedDictionary<DateTime, float[]>> getBetweenTimeTemperature(IEnumerable<Wire> wires, DateTime start, DateTime end)
        {
            Dictionary<Wire, SortedDictionary<DateTime, float[]>> result
                = new Dictionary<Wire, SortedDictionary<DateTime, float[]>>(); ;
            string partitions = getPartition(start, end);
            foreach (Wire wire in wires)
            {
                string query = String.Format("SELECT sensor_index, temperature, time FROM sensor_temp PARTITION ({4}) WHERE device_addres = {0} and leg = {1}"
                    + " AND time between \'{2}\' AND \'{3}\'",
                    wire.DeviceAddress, wire.Leg, start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions);
                DataTable dataTable = executeSelectQuery(query);
                result.Add(wire, parseTimeDataTable(dataTable, wire));
            }
            return result;
        }

        

        ///<summary>
        ///Выдает таблицу максимумов, средних и минимальных температур
        /// </summary>
        ///
        public override SortedDictionary<DateTime, float[]> getMaxAvgMinBetweenTime(int[] wires, DateTime start, DateTime end)
        {
            string partitions = getPartition(start, end);
            string query = String.Format(
            "SELECT ST.time,  round(max(ST.temperature), 1), round(avg(ST.temperature),1), round(min(ST.temperature),1) " +
            "FROM sensor_temp PARTITION ({3}) AS ST JOIN wire AS W " +
            "ON ST.device_addres = W.device_address AND ST.leg = W.leg " +
            "WHERE ST.temperature between -80 AND 140 AND ST.time between \'{0}\' AND \'{1}\' AND W.id IN ({2}) " +
            "GROUP BY ST.time;", start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), string.Join(", ", wires), partitions);

            DataTable dataTable = executeSelectQuery(query);
            return parseMaxAvgMinTable(dataTable);
        }

        private SortedDictionary<DateTime, float[]> parseMaxAvgMinTable(DataTable data)
        {
            SortedDictionary<DateTime, float[]> result = new SortedDictionary<DateTime, float[]>();

            if (data == null)
            {
                
                return result;
            }
            try {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DateTime dt = Convert.ToDateTime(data.Rows[i][0]);
                    float[] mam = new float[3];
                    mam[0] = Convert.ToSingle(data.Rows[i][1]);
                    mam[1] = Convert.ToSingle(data.Rows[i][2]);
                    mam[2] = Convert.ToSingle(data.Rows[i][3]);
                    result.Add(dt, new float[3]);
                    result[dt] = mam; 
                    

                }
                return result;
            }
            catch (Exception e)
            {
                return new SortedDictionary<DateTime, float[]>();
            }
        }



        public override List<DateTime> getSetTimeBetweenTime(DateTime start, DateTime end, int silosId)
        {
            string partitions = getPartition(start, end);
            List<DateTime> result = new List<DateTime>();
            string query = String.Format(
                " SELECT  DISTINCT time " +
                " FROM sensor_temp PARTITION ({2}) AS st JOIN wire AS w ON w.device_address = st.device_addres AND w.leg =st.leg " +
                " WHERE time BETWEEN \'{0}\' AND \'{1}\' AND w.silos_id ={3};",
                start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions, silosId);
            DataTable dataTable = executeSelectQuery(query);

            if (dataTable != null)
                result = parseTime(dataTable);

            return result;
        }

        private List<DateTime> parseTime(DataTable dataTable)
        {
            List<DateTime> result  = new List<DateTime>();
            if (dataTable == null)
            {
                return result;
            }
            try
            {
                for (int i = 0; i < dataTable.Rows.Count; ++i)
                {
                    DateTime dt = Convert.ToDateTime(dataTable.Rows[i][0]);
                    result.Add(dt);
                }
                return result;
            }
            catch (Exception e)
            {
                return new List<DateTime>();
            }
        }

        public override List<DateTime> getSetTimeBetweenTime(DateTime start, DateTime end)
        {
            string partitions = getPartition(start, end);
            List<DateTime> result = new List<DateTime> ();
            string query = String.Format(
                " SELECT  DISTINCT time " +
                " FROM sensor_temp PARTITION ({2}) " +
                " WHERE time BETWEEN \'{0}\' AND \'{1}\';", 
                start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions);
            DataTable dataTable = executeSelectQuery(query);

            if(dataTable != null)
                result = parseTime(dataTable);

            return result;
        }


        /// <summary>
        /// Выдает таблицу средних температур на одной подвеске
        /// </summary>
        /// <param name="wire"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public override SortedDictionary<DateTime, float> getAvgTempSilosBetweenTime(Wire wire, DateTime start, DateTime end) 
        {
            string partitions = getPartition(start, end);
            string query = String.Format(
                " SELECT time, ROUND(AVG(temperature), 1) " +
                " FROM sensor_temp PARTITION ({4}) " +
                " WHERE device_addres = {0} AND leg = {1}  AND temperature>-80 AND time between \'{2}\' AND \'{3}\' " +
                " GROUP BY time", 
                wire.DeviceAddress, wire.Leg, start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions);
            DataTable dataTable = executeSelectQuery(query);
            //MessageBox.Show(dataTable.ToString());
            return parseAVGTimeDataTable(dataTable);
        }

        /// <summary>
        /// Выводит таблицу средних температур
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="silos"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public override SortedDictionary<DateTime, float> getAllAVGTempSilos(Silos silos, DateTime start, DateTime end)
        {
            string partitions = getPartition(start, end);
            string query = String.Format(
                " SELECT ST.time , ROUND(AVG(ST.temperature), 1) " +
                " FROM sensor_temp PARTITION ({3}) AS ST join wire as W ON W.device_address = ST.device_addres " +
                " JOIN silos AS S ON W.silos_id = S.id " +
                " WHERE ST.temperature >-80 AND S.name = \'{0}\' AND ST.time BETWEEN " +
                " \'{1}\' AND \'{2}\' " +
                " GROUP BY ST.time",
                silos.Name, start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions);
            DataTable dataTable = executeSelectQuery(query);
            return parseAVGTimeDataTable(dataTable);
        }

        private SortedDictionary<DateTime, float> parseAVGTimeDataTable(DataTable dataTable)
        {
            SortedDictionary<DateTime, float> result = new SortedDictionary<DateTime, float>();
            if(dataTable ==null)
            {
                //MessageBox.Show("Таблица пустая");
                return result;
            }
            try
            {
                for(int i = 0; i<dataTable.Rows.Count;++i)
                {
                    DateTime dt = Convert.ToDateTime(dataTable.Rows[i][0]);
                    float temp = Convert.ToSingle(dataTable.Rows[i][1]);
                    result.Add(dt, temp);
                }
                return result;
            }
            catch (Exception e)
            {
                return new SortedDictionary<DateTime, float>();
            }
        }


        public override List<float> getAVGSilosTemp(List<int> idDevice, DateTime time)
        {
            string partitions = getPartition(time, time);
            List<float> result = new List<float>();
            string valueDevice = "";
            foreach (int i in idDevice)
            {
                valueDevice += i.ToString()+",";
            }
            valueDevice = valueDevice.Remove(valueDevice.Length-1);
            string query = String.Format("select round(avg(temperature), 2) " +
                "from sensor_temp PARTITION ({2}) " +
                "WHERE time = \'{1}\' AND device_addres in ({0})  AND temperature>-90 " +
                "group by  sensor_index;", valueDevice, time.ToString("yyyy-MM-dd H:mm:ss"), partitions);
            
            DataTable dataTable = executeSelectQuery(query);
            
            if (dataTable == null)  return result; 

            result = parseFloats(dataTable);
            MyLoger.Log(DateTime.Now.ToString() + " Запрос к бд количество строк " + dataTable.Rows.Count.ToString()+" Количетсво строк после парсинга "+result.Count.ToString());
            return result;
        }

        private List<float> parseFloats(DataTable table)
        {
            var result = new List<float>();
            for (int i = 0; i<table.Rows.Count; ++i)
            {
                result.Add(Convert.ToSingle(table.Rows[i][0]));
            }
            return result;
        }

        public override SortedDictionary<DateTime, float[]> getBetweenTimeTemperature(Wire wire, DateTime start, DateTime end)
        {
            SortedDictionary<DateTime, float[]> result
                = new SortedDictionary<DateTime, float[]>(); ;
            //нужна индексация
            string partitions = getPartition(start, end);
            string query = String.Format(
                " SELECT sensor_index, temperature, time " +
                " FROM sensor_temp PARTITION ({4}) " +
                " WHERE device_addres = {0} and leg = {1} "
                + " AND time BETWEEN \'{2}\' AND \'{3}\' AND temperature BETWEEN -80 AND 140;",
                wire.DeviceAddress, wire.Leg, start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"), partitions);
            DataTable dataTable = executeSelectQuery(query);
            result = parseTimeDataTable(dataTable, wire);

            return result;
        }

        
        private SortedDictionary<DateTime, float[]> parseTimeDataTable(DataTable dataTable, Wire wire)
        {
            SortedDictionary<DateTime, float[]> result = new SortedDictionary<DateTime, float[]>();
            if (dataTable == null)
                return result;
            try
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    int sensorIdx = Convert.ToInt32(dataTable.Rows[i][0]);
                    float temperature = Convert.ToSingle(dataTable.Rows[i][1]);
                    DateTime date = Convert.ToDateTime(dataTable.Rows[i][2]);

                    if (sensorIdx < 0 || sensorIdx > wire.SensorCount)//Нельзя добавить
                        continue;

                    if (!result.ContainsKey(date)) // для этого времени пока нет результатов.
                    {
                        result.Add(date, new float[wire.SensorCount]);
                    }

                    //записываем значение температуры.
                    result[date][sensorIdx] = temperature;
                }

                return result;
            }
            catch
            {
                return new SortedDictionary<DateTime, float[]>();
            }
        }

        public override bool deleteTemperaturesBeforeDate(DateTime time)
        {
            string formatTime = time.ToString("yyyy-MM-dd") + " 23:59:59";
            string query = "delete from sensor_temp where time <= \'" + formatTime + "\';";
            return executeUpdateQuery(query);
        }

        public override void deleteLastTemperatur()
        {
            string query = String.Format("delete from sensor_temp_last;");
            executeUpdateQuery(query);
        }
    }
}
