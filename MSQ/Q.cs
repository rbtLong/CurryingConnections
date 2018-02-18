using System;
using System.Data;
using System.Data.SqlClient;

namespace PZHelpers.MSQ
{
    /// <summary>
    /// This class is a statically-defined class that has set of extension methods that
    /// `hinge` off of the Db class. It is usually the second point when Currying Connections.
    /// 
    /// These functions return a QBuilder which will store important query information such
    /// as the SQL string command or string stored procedure, and the parameters. These functions
    /// include Cmd and Proc. For information on how the it is stored, see QBuilder.cs.
    ///   
    /// Functions like Dt and Ds will extract data by returning ADO.Net objects like the DataTable (Dt)
    /// or DataSet wich our codebase occasionally uses.
    ///
    /// Connect and Cmd are more generic ways to connect to the database while still guaranteeing
    /// no connection leaks. These functions make use of callback functions and is used by the
    /// Currying expressions to connect to the database.
    /// 
    /// </summary>
    public static class Q
    {

        public static QBuilder Cmd(this Db info, string sCmd, CommandType cmdType = CommandType.Text)
        {
            return new QBuilder()
            {
                StrCmd = sCmd,
                DbInfo = info,
                TypeCmd = cmdType
            };
        }

        public static QBuilder Proc(this Db info, string procName)
        {
            return new QBuilder()
            {
                StrCmd = procName,
                DbInfo = info,
                TypeCmd = CommandType.StoredProcedure
            };
        }

        public static T Cmd<T>(this Db info, string strCmd, CommandType type, Func<SqlCommand, SqlConnection, T> fn)
        {
            return info.Connect(conn =>
            {
                using (var cmd = new SqlCommand(strCmd, conn))
                {
                    cmd.CommandType = type;
                    return fn(cmd, conn);
                }
            });
        }

        public static void Cmd(this Db info, string strCmd, CommandType type, Action<SqlCommand, SqlConnection> fn)
        {
            info.Connect(conn =>
            {
                using (var cmd = new SqlCommand(strCmd, conn))
                {
                    cmd.CommandType = type;
                    fn(cmd, conn);
                }
            });
        }

        public static DataSet Ds(this Db info, string sCmd, CommandType type)
        {
            return info.Cmd(sCmd, type, (cmd, conn) =>
            {
                var adp = new SqlDataAdapter(cmd);
                var ds = new DataSet();

                conn.Open();
                adp.Fill(ds);

                return ds;
            });
        }

        public static DataTable Dt(this Db info, string sCmd, CommandType type)
        {
            return info.Cmd(sCmd, type, (cmd, conn) =>
            {
                var adp = new SqlDataAdapter(cmd);
                var dt = new DataTable();

                conn.Open();
                adp.Fill(dt);

                return dt;
            });
        }

        public static DataSet Ds(this Db info, string sCmd, string dtName, CommandType type)
        {
            return info.Cmd(sCmd, type, (cmd, conn) =>
            {
                var adp = new SqlDataAdapter(cmd);
                var ds = new DataSet(dtName);

                conn.Open();
                adp.Fill(ds);

                return ds;
            });
        }

        public static T Connect<T>(this Db info, Func<SqlConnection, T> fn)
        {
            using (var conn = new SqlConnection(info.ConnStr))
            {
                return fn(conn);
            }
        }

        public static void Connect(this Db info, Action<SqlConnection> fn)
        {
            using (var conn = new SqlConnection(info.ConnStr))
            {
                fn(conn);
            }
        }

    }
}
