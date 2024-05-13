using System;
using System.Collections.Generic;
using System.Data;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Model;

namespace SystemOfThermometry3.DAO
{
    partial class MySQLDAO : Dao
    {
        public override int addSubdivision(StructureSubdivision subdivision)
        {
            string query = String.Format("INSERT INTO subdivision" +
                "(name)" +
            " VALUES (\'{0}\');",
            QueryHolder.convertStringToWrite(subdivision.Name));

            return (int)executeInsertQuery(query);
        }

        public override bool updateSubdivision(StructureSubdivision subdivision)
        {

            string query = String.Format("UPDATE subdivision SET" +
                " name = \'{1}\' " +
                " WHERE id = {0};",
            subdivision.Id, QueryHolder.convertStringToWrite(subdivision.Name));

            return executeUpdateQuery(query);
        }

        public override bool deleteSubdivision(int subdivisionId)
        {
            return executeUpdateQuery("DELETE FROM subdivision WHERE id = " + subdivisionId + ";");
        }

        private StructureSubdivision parseSubdivision(DataTable dataTable, int row)
        {
            StructureSubdivision s = new StructureSubdivision();
            s.Id = Convert.ToInt32(dataTable.Rows[row][0]);
            s.Name = QueryHolder.convertStringFromDB(Convert.ToString(dataTable.Rows[row][1]));
            return s;
        }

        public override Dictionary<int, StructureSubdivision> getAllSubdivisions()
        {
            DataTable dataTable = executeSelectQuery("SELECT * FROM subdivision;");
            if (dataTable == null)
                return null;

            Dictionary<int, StructureSubdivision> result = new Dictionary<int, StructureSubdivision>();
            try
            {
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    StructureSubdivision s = parseSubdivision(dataTable, row);
                    result.Add(s.Id, s);
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        public override StructureSubdivision getSubdivision(int id)
        {
            DataTable dataTable = executeSelectQuery("SELECT * FROM subdivision WHERE id = " + id + ";");
            if (dataTable == null || dataTable.Rows.Count == 0)
                return null;

            try
            {
                StructureSubdivision s = parseSubdivision(dataTable, 0);
                return s;
            }
            catch
            {
                return null;
            }
        }


    }
}
