using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomDAL
{
    public class Config
    {
        public static string connStr = "DataBase = prdb; Server = ol_informix1410_9; User ID = informix; Password = ****";

        public static string ConnStr
        {
            get { return connStr; }
        }
    }
}
