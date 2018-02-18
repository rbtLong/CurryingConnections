using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

namespace PZHelpers
{
    /// <summary>
    /// NOTICE: Please look at files in MSQ for Connection Currying related stuff.
    ///
    /// This class is used to connect to the database to grab the connection strings to
    /// connect to other databases (including itself). This was how Jenzabar (our vendor)
    /// company built the framework. This class doesn't deal with the Currying aspects
    /// of connection strings. It primarily serves as a substitute to the default framework's
    /// method of connecting to the database.
    ///
    /// One of the advantages of this method, over using the framework's built-in config settings,
    /// is a more direct connection to the database without relying on the entire Jenzabar
    /// application to run. This allows us to unit test without launching the monolithic application
    /// every time. The custom we write for this product has to run as plug-ins and every time a
    /// change is pushed, the entire application has to reload all the plug-ins which can take more than
    /// a minute. Using this as a substitution, we were able to test functionalities directly.
    ///
    /// Another advantage we can control the cache and clear it at any time.
    /// 
    /// This is used as an alternative to Jenzabar's FWK_ConfigSettings.
    /// </summary>
    public static class Cfg
    {
        const string _jicsProd = "<intentionally hidden>";
        const string _jicsTest = "<intentionally hidden>";
        const string _jicsDev = "<intentionally hidden>";
        const string _jicsLocal = "<intentionally hidden>";

        public static string Jics
        { 
            get
            {
                var n = Environment.MachineName.ToLower();
                if( n == "mycampus2" ) return _jicsProd;
                if (n == "mycampus2test") return _jicsTest;
                if (n == "mycampus2dev") return _jicsDev;
                return _jicsLocal;
            }
        }

        public static Dictionary<string, string>[] GetRows(bool useCache = true)
        {
            System.Runtime.Caching.ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1);

            if (useCache && cache["FWK_ConfigSettings"] != null)
                return cache["FWK_ConfigSettings"] as Dictionary<string, string>[];

            var result = new List<Dictionary<string, string>>();
            using (var conn = new SqlConnection(Jics))
            {
                const string scmd = "select * from [dbo].FWK_ConfigSettings;";
                using (var cmd = new SqlCommand(scmd, conn))
                {
                    conn.Open();
                    var r = cmd.ExecuteReader();

                    if (!r.HasRows)
                        throw new Exception("No data from FWK_ConfigSettings");

                    while (r.Read())
                    {
                        var row = new Dictionary<string, string>();
                        for (var i = 0; i < r.FieldCount; ++i)
                            row[r.GetName(i)] = r[i] as string;
                        result.Add(row);
                    }
                }
            }

            cache.Set("FWK_ConfigSettings", result.ToArray(), policy);
            return cache["FWK_ConfigSettings"] as Dictionary<string, string>[];
        }

        public static string Value(string category, string key)
        {
            var r = GetRows().FirstOrDefault(s => s["Category"] == category && s["Key"] == key);

            if (ReferenceEquals(null, r))
                return null;
            else
                return r["Value"];
        }
    }
}