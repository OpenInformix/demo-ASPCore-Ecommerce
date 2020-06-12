using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EcomApplication.Models;
using EcomDAL;
using System.Data;

namespace EcomApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        EcomDetailsDAL ecomDAL = new EcomDetailsDAL();
        DataTable dt;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string mycmd = "select * from Mobiles";
            dt = new DataTable();
            try
            {
                dt = ecomDAL.SelactAll(mycmd);
            }
            catch (Exception ex)
            {
                string createTable = "Create table Mobiles (SLNo serial PRIMARY KEY, MobileName nvarchar(100) NULL, Price decimal(18, 2)," +
                        " Quantity int NULL,  Description nvarchar(250) NULL, PicURL nvarchar(250) NULL," +
                        " Model nvarchar(50) NULL, Features nvarchar(200) NULL, Color nvarchar(20) NULL, SimType nvarchar(10) NULL)";
                Boolean status = ecomDAL.DDLOpperation(createTable);
                if (status)
                {
                    dt = ecomDAL.SelactAll(mycmd);
                }
            }

            List<Mobiles> list = new List<Mobiles>();
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Mobiles mob = new Mobiles();
                mob.SLNo = Convert.ToInt32(dt.Rows[i]["slNo"]);
                mob.MobileName = dt.Rows[i]["MobileName"].ToString();
                mob.Price = Convert.ToDecimal(dt.Rows[i]["Price"]);
                mob.Description = dt.Rows[i]["Description"].ToString();
                mob.PicURL = dt.Rows[i]["PicURL"].ToString();
                list.Add(mob);
            }
                return View(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
