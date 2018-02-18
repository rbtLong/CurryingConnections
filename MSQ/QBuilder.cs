using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace PZHelpers.MSQ
{
    /// <summary>
    /// This class is meant to store Connection Currying expression. It stores the string
    /// SQL command, the SQL commad type, the parameters, and the database connection string.
    /// Function-wise, it also contains how the expression can execute, usually the last point
    /// in Connection Currying. Row and Rows make use of Dictionary<string, object> (like JSON)
    /// because in many instances, we can simply pass this straight to front-end as a JSON object.
    /// 
    /// Here are the ways a Currying Expression can terminate:
    ///  Scalar - Executes the query with parameters as prepared statements and returns the first column of the first row as an object.
    ///  NonQuery - Executes the query with parameters as prepared statement and returns an integer representing rows affected by the execution
    ///             this typically occurs on update, delete, and insert SQL statements. 
    ///  Rows - Executes the query with parameters as prepared statement and iterates through the results, adds each row into a 
    ///         Dictionary<string, object> where the dictionary key is the column name (string) and the dictionary value is the column's value (object).
    ///  Row - Same as Rows execpt, it will take the first row returned.
    ///  Ds - Executes the query with parameters as prepared statement and returns an ADO.Net DataSet object.
    ///
    /// For Scalar, Rows, and Row, if there is no data, null will be returned.
    /// </summary>
    public class QBuilder
    {
        private readonly List<SqlParameter> _allParams = new List<SqlParameter>();

        public string StrCmd { get; set; }
        public Db DbInfo { get; set; }
        public List<SqlParameter> AllParams => _allParams;
        public CommandType TypeCmd { get; set; } = CommandType.Text;

        public QBuilder Param(string name, object value)
        {
            _allParams.Add(new SqlParameter(name, value));
            return this;
        }

        public QBuilder Param(string name, SqlDbType t, object value)
        {
            var p = new SqlParameter(name, t) {Value = value};
            _allParams.Add(p);
            return this;
        }

        public QBuilder Param(string name, SqlDbType t, int size, object value)
        {
            var p = new SqlParameter(name, t, size) {Value = value};
            _allParams.Add(p);
            return this;
        }

        public int NonQuery()
        {
            return DbInfo.Cmd(StrCmd, TypeCmd, (cmd, conn) =>
            {
                foreach (var p in _allParams)
                    cmd.Parameters.Add(p);

                conn.Open();
                return cmd.ExecuteNonQuery();
            });
        }

        public Dictionary<string, object>[] Rows()
        {
            return DbInfo.Cmd(StrCmd, TypeCmd, (cmd, conn) =>
            {
                foreach (var p in _allParams)
                    cmd.Parameters.Add(p);

                conn.Open();
                return cmd.ExecuteReader().RowsToDict();
            });
        }

        public Dictionary<string, object> Row()
        {
            return DbInfo.Cmd(StrCmd, TypeCmd, (cmd, conn) =>
            {
                foreach (var p in _allParams)
                {
                    cmd.Parameters.Add(p);
                }

                conn.Open();
                var res = cmd.ExecuteReader();
                return res.RowToDict();
            });
        }

        public object Scalar()
        {
            return DbInfo.Cmd(StrCmd, TypeCmd, (cmd, conn) =>
            {
                foreach (var p in _allParams)
                    cmd.Parameters.Add(p);

                conn.Open();
                return cmd.ExecuteScalar();
            });
        }

        public DataSet Ds()
        {
            return DbInfo.Cmd(StrCmd, TypeCmd, (cmd, conn) =>
            {
                foreach (var p in _allParams)
                    cmd.Parameters.Add(p);

                var adp = new SqlDataAdapter(cmd);
                var ds = new DataSet();

                conn.Open();
                adp.Fill(ds);
                return ds;
            });
        }
    }
}