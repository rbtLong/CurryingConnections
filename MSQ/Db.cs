namespace PZHelpers.MSQ
{
    /// <summary>
    /// This class initiates the Connection Method-Chaining. Below is a list of statically-defined 
    /// databases to connect to. The connection strings are retrieved from the database,
    /// this was the approach taken by our vendor Jenzabar. The Cfg class is a substitution
    /// for Jenzabar's built-in way to grab connection strings for more direct testing without
    /// launching the framework. For more information, please see the Cfg class. 
    ///
    /// To use or test this class, simply replace the Db(... param ...) parameter with an actual SQL database
    /// string connection. (You may also want to change the statically-defined variable names to be
    /// more suitable for your uses.)
    /// </summary>
    public class Db
    {
        public static readonly Db Logs = new Db(Cfg.Value("C_ConnStr", "dblog_ms"));
        public static readonly Db Jics = new Db(Cfg.Value("C_ConnStr", "jicsdb_ms"));
        public static readonly Db Powerfaids = new Db(Cfg.Value("C_ConnStr", "powerfaidsdb_ms"));
        public static readonly Db Irb = new Db(Cfg.Value("C_ConnStr", "irb_ms"));
        public static readonly Db Irbdev = new Db(Cfg.Value("C_ConnStr", "irbdev_ms"));
        public static readonly Db Forms = new Db(Cfg.Value("C_ConnStr", "formsdb_ms"));

        public string ConnStr { get; set; }

        public Db(string connStr)
        {
            ConnStr = connStr;
        }
    }
}