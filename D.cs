using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PZHelpers.DbLog;

namespace PZHelpers
{
    /// <summary>
    /// This class helps with creating and changing Dictionary<string, object> related operations.
    /// </summary>
    public static class D
    {
        public static Dictionary<string, object> Field(this Dictionary<string, object> content, string key, object val)
        {
            if (val is string && ((string)val).ValidJson())
                content.Add(key, ((string)val).Unjson());
            else
                content.Add(key, val);

            return content;
        }

        /// <summary>
        /// Relabels the value of a key. For example, by default, content is the key for
        /// content in result. However, the front-end sometimes expects data.
        /// </summary>
        public static Dictionary<string, object> Relabel(this Dictionary<string, object> result, string oldlabel, string newLabel)
        {
            var lbl = result.Keys.FirstOrDefault(k => k == oldlabel);
            if (!ReferenceEquals(null, lbl))
            {
                var val = result[lbl];
                result.Remove(lbl);
                result[newLabel] = val;
            }
            return result;
        }

        public static Dictionary<string, object>[] RowsToDict(this DbDataReader r)
        {
            var res = new List<Dictionary<string, object>>();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    var dict = new Dictionary<string, object>();
                    for (var i = 0; i < r.FieldCount; ++i)
                        dict[r.GetName(i)] = r[i];
                    res.Add(dict);
                }
                return res.ToArray();
            }
            return null;
        }

        public static Dictionary<string, object> RowToDict(this DbDataReader r)
        {
            if (r.HasRows)
            {
                var dict = new Dictionary<string, object>();
                while (r.Read())
                {
                    for (var i = 0; i < r.FieldCount; ++i)
                        dict[r.GetName(i)] = r[i];
                }
                return dict;
            }

            return null;
        }

        public static Dictionary<string, object> RmvCols(this Dictionary<string, object> d, params string[] cols)
        {
            var d1 = new Dictionary<string, object>(d);
            foreach (var c in cols)
                if(d1.ContainsKey(c))
                    d1.Remove(c);
            return d1;
        }

        public static Dictionary<string, object>[] RmvCols(this Dictionary<string, object>[] d, params string[] cols)
        {
            var d1 = new List<Dictionary<string, object>>();
            foreach (var itm in d)
                d1.Add(itm.RmvCols(cols));
            return d1.ToArray();
        }

        public static string ToJson<T1, T2>(this Dictionary<T1, T2>[] obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string ToJson<T1, T2>(this Dictionary<T1, T2> obj)
        {
            if (ReferenceEquals(null, obj))
                return null;

            return JsonConvert.SerializeObject(obj);
        }

        public static Dictionary<string, object> J()
        {
            return new Dictionary<string, object>();
        }

        public static Dictionary<string, object> Contact(this Dictionary<string, object> src, Dictionary<string, object> obj)
        {
            for (var i = 0; i < obj.Count; ++i)
            {
                var o = obj.ElementAt(i);
                src.Field(o.Key, o.Value);
            }
            return src;
        }

        public static string Json(this object obj)
        {
            if (obj is SqlDataReader)
                return obj.ToString();

            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                ex.Error(MethodBase.GetCurrentMethod())
                    .Describe("Error while trying to object into json file.")
                    .Ok();

                return null;
            }
        }

        public static object Unjson(this string obj)
        {
            return JsonConvert.DeserializeObject(obj);
        }

        public static bool Null(this object o)
        {
            return ReferenceEquals(o, null);
        }

        public static bool ValidJson(this string strInput)
        {
            try
            {
                JToken.Parse(strInput);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
