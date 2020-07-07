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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Informix.Net.Core;
using System.Configuration;

namespace EcomApplication.Controllers
{
    public class HomeController : Controller
    {
        string connString = Config.ConnStr;

        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly ILogger<HomeController> _logger;
        EcomDetailsDAL ecomDAL = new EcomDetailsDAL();
        DataTable dt;
        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger)
        {
            hostingEnvironment = environment;
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
                        " Model nvarchar(50) NULL, Features nvarchar(200) NULL, Color nvarchar(20) NULL, SimType nvarchar(10) NULL, ImageFile Blob)";
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
                mob.SLNo = Convert.ToInt32(dt.Rows[i]["SLNo"]);
                mob.MobileName = dt.Rows[i]["MobileName"].ToString();
                mob.Price = Convert.ToDecimal(dt.Rows[i]["Price"]);
                mob.Description = dt.Rows[i]["Description"].ToString();
                mob.PicURL = dt.Rows[i]["PicURL"].ToString();
                list.Add(mob);
                // Downloading the photo from databse and storing it on the disk
                // To save that newly uploaded image to Disk location inside wwwroot/Images folder
                var downloads = Path.Combine(hostingEnvironment.WebRootPath, "DownloadImages");
                var imagePath = Path.Combine(downloads, mob.PicURL);

                FileInfo file = new FileInfo(imagePath);
                if (file.Exists)
                {
                    file.Delete();
                }

                using (IfxConnection Con = new IfxConnection(connString))
                {
                    Con.Open();
                    string selectImage = "select LOTOFILE (imagefile, " + "'" + imagePath + "!'" + ", 'client') from mobiles where slno = ?";
                    IfxCommand selectImagecmd = new IfxCommand(selectImage, Con);
                    selectImagecmd.Parameters.Add("slno", IfxType.Serial).Value = mob.SLNo;
                    selectImagecmd.ExecuteScalar();
                    Con.Close();
                }
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
