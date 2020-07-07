using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApplication
{
    public class Config
    {
        public static string connStr = "DataBase = webapp; Server = ol_informix1410_9; User ID = informix; Password = ****";

        public static string ConnStr
        {
            get { return connStr; }
        }
    }
}
