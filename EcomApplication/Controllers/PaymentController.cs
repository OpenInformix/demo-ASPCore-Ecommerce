using System;
using System.Collections.Generic;
using System.Data;
using EcomDAL;
using System.Linq;
using System.Threading.Tasks;
using EcomApplication.Models;
using Informix.Net.Core;
using Microsoft.AspNetCore.Mvc;

namespace EcomApplication.Controllers
{
    public class PaymentController : Controller
    {
        string connString = Config.ConnStr;

        public IActionResult Index()
        {
            DataTable table = new DataTable();

            using (IfxConnection Con = new IfxConnection(connString))
            {
                string query = "SELECT SUM(TotalAmount) FROM Cart";
                IfxCommand cmd = new IfxCommand(query, Con);
                Con.Open();
                int sum = 0;
                try
                {
                    IfxDataReader rows = cmd.ExecuteReader();
                    while (rows.Read())
                    {
                        sum = Convert.ToInt32(rows[0]);
                    }
                    rows.Close();
                    cmd.Dispose();    
                }
                catch (IfxException ex)
                {

                }
                finally
                {
                    Con.Close();
                }

                table.Columns.Add("TotalAmount", typeof(int));
                {
                   table.Rows.Add(sum);
                }
            }
            return View(table);
        }
    }
}