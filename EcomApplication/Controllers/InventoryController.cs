using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using System.Data;
using EcomApplication.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace EcomApplication.Controllers
{
    public class InventoryController : Controller
    {
        string connString = "DataBase=webapp;Server=ol_informix1410_9;User ID = informix; Password=Rinvoke1;";
        private readonly IWebHostEnvironment hostingEnvironment;
        public InventoryController(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            DataTable table = new DataTable();
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                try
                {
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM Mobiles", Con);
                    ifx.Fill(table);
                }
                catch (Exception ex)
                {
                    string createTable = "Create table Mobiles (SLNo serial PRIMARY KEY, MobileName nvarchar(100) NULL, Price decimal(18, 2), Quantity int NULL,  Description nvarchar(250) NULL, PicURL nvarchar(250) NULL)";
                    IfxCommand cmd = new IfxCommand(createTable, Con);
                    cmd.ExecuteNonQuery();
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM Mobiles", Con);
                    ifx.Fill(table);
                }
            }
            return View(table);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Mobiles());
        }

        // POST: Mobiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Mobiles mobilesModel)
        {
            // To create a Unique file name and URL everytime when User upload a new picture
            string ImageFileName = Path.GetFileNameWithoutExtension(mobilesModel.ImageFile.FileName);
            string ImageFileExtension = Path.GetExtension(mobilesModel.ImageFile.FileName);
            string FinalImageName = ImageFileName + DateTime.Now.ToString("yymmssfff") + ImageFileExtension;
            mobilesModel.PicURL = FinalImageName;
            // To save that newly uploaded image to Disk location inside wwwroot/Images folder
            var uploads = Path.Combine(hostingEnvironment.WebRootPath, "Images");
            var path = Path.Combine(uploads, FinalImageName);
            mobilesModel.ImageFile.CopyTo(new FileStream(path, FileMode.Create));

            // To save the newly added Mobile and the Image disk path to Database table (Mobiles)
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                string query = "INSERT INTO Mobiles (MobileName, Price, Quantity, Description, PicURL) VALUES(?, ?, ?, ?, ?)";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = mobilesModel.MobileName;
                cmd.Parameters.Add("price", IfxType.Decimal).Value = mobilesModel.Price;
                cmd.Parameters.Add("quantity", IfxType.Int).Value = mobilesModel.Quantity;
                cmd.Parameters.Add("description", IfxType.VarChar).Value = mobilesModel.Description;
                cmd.Parameters.Add("picurl", IfxType.VarChar).Value = mobilesModel.PicURL;

                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // GET: /Mobiles/Edit/5
        public ActionResult Edit(int SLNo)
        {
            Mobiles mobile = new Mobiles();
            DataTable mobileTable = new DataTable();
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                // Prone to SQL enjection
                string query = "SELECT * FROM Mobiles Where SLNo = ?";
                IfxDataAdapter ifx = new IfxDataAdapter(query, Con);
                ifx.SelectCommand.Parameters.Add("SLNo", IfxType.Serial).Value = SLNo;
                ifx.Fill(mobileTable);
            }
            if (mobileTable.Rows.Count == 1)
            {
                mobile.SLNo = Convert.ToInt32(mobileTable.Rows[0][0].ToString());
                mobile.MobileName = mobileTable.Rows[0][1].ToString();
                mobile.Price = Convert.ToDecimal(mobileTable.Rows[0][2].ToString());
                mobile.Quantity = Convert.ToInt32(mobileTable.Rows[0][3].ToString());
                mobile.Description = mobileTable.Rows[0][4].ToString();
                mobile.PicURL = mobileTable.Rows[0][5].ToString();
                return View(mobile);
            }
            else
                return RedirectToAction("Index");
        }

        // POST: /Mobiles/Edit/5
        [HttpPost]
        public ActionResult Edit(Mobiles mobile)
        {
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                string query = "UPDATE Mobiles SET MobileName = ? , Price= ? , Quantity = ? , Description = ? , PicURL = ? Where SLNo = ?";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = mobile.MobileName;
                cmd.Parameters.Add("price", IfxType.Decimal).Value = mobile.Price;
                cmd.Parameters.Add("quantity", IfxType.Int).Value = mobile.Quantity;
                cmd.Parameters.Add("description", IfxType.VarChar).Value = mobile.Description;
                cmd.Parameters.Add("picurl", IfxType.VarChar).Value = mobile.PicURL;
                cmd.Parameters.Add("slno", IfxType.Serial).Value = mobile.SLNo;
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // GET: MobilesDelete/5
        [HttpGet]
        public ActionResult Delete(int slno)
        {
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                string query = "DELETE FROM Mobiles WHere SLNo = ?";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("slno", IfxType.Serial).Value = slno;
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}