using System;
using System.Configuration;
using System.Data;
using Informix.Net.Core;

namespace EcomDAL
{
    public class EcomDetailsDAL 
    {
        IfxCommand cmd;
        IfxDataAdapter da;
        public static IfxConnection connect()
        {
            string connection = Config.connStr;
            //string connection = ConfigurationManager.ConnectionStrings["Connect"].ConnectionString;
            IfxConnection con = new IfxConnection(connection);
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            else con.Open();
            return con;
        }

        public bool DMLOpperation(string query)
        {
            cmd = new IfxCommand(query, EcomDetailsDAL.connect());
            int x = cmd.ExecuteNonQuery();
            if (x == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool DDLOpperation(string query)
        {
            cmd = new IfxCommand(query, EcomDetailsDAL.connect());
            int x = cmd.ExecuteNonQuery();
            if (x == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public DataTable SelactAll(string query)
        {
            da = new IfxDataAdapter(query, EcomDetailsDAL.connect());
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}
