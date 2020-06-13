using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using EcomApplication.Models;
using System.Data;

namespace EcomApplication.Controllers
{
    public class MyOrdersController : Controller
    {
        public IActionResult Index()
        {
            string connString = "DataBase=webapp;Server=ol_informix1410_9;User ID = informix; Password=Rinvoke1;";
            DataTable table = new DataTable();
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                try
                {
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM orderdetails", Con);
                    ifx.Fill(table);
                }
                catch (Exception ex)
                {
                    string createTable = "Create table orderdetails (orderid serial PRIMARY KEY, SLNo int, MobileName nvarchar(100) NULL, " +
                            " Description nvarchar(250) NULL, PicURL nvarchar(250) NULL, Model nvarchar(50) NULL, Features nvarchar(200) NULL, " +
                            "Color nvarchar(20) NULL, SimType nvarchar(10) NULL, PurchaseDate varchar(50), Price decimal(18, 2), Quantity int NULL, TotalAmount decimal(18,2))";
                    IfxCommand cmd = new IfxCommand(createTable, Con);
                    cmd.ExecuteNonQuery();
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM orderdetails", Con);
                    ifx.Fill(table);
                }
                finally
                {
                    Con.Close();
                }
            }
            return View(table);
        }
    }
}