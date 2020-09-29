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
using EcomDAL;

namespace EcomApplication.Controllers
{
    public class InventoryController : Controller
    {
        string connString = Config.ConnStr;

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
                    string createTable = "Create table Mobiles (SLNo serial PRIMARY KEY, MobileName nvarchar(100) NULL, Price decimal(18, 2)," +
                        " Quantity int NULL,  Description nvarchar(250) NULL, PicURL nvarchar(250) NULL," +
                        " Model nvarchar(50) NULL, Features nvarchar(200) NULL, Color nvarchar(20) NULL, SimType nvarchar(10) NULL, ImageFile Blob)";
                    IfxCommand cmd = new IfxCommand(createTable, Con);
                    cmd.ExecuteNonQuery();
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM Mobiles", Con);
                    ifx.Fill(table);
                }
                finally
                {
                    Con.Close();
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
            var imagePath = Path.Combine(uploads, FinalImageName);

            FileStream fileStream = new FileStream(imagePath, FileMode.Create);
            mobilesModel.ImageFile.CopyTo(fileStream);
            fileStream.Close();


            //string fileByteArray = null;
            //var fileBytes = 0;
            /*
            if (mobilesModel.ImageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    //mobilesModel.ImageFile.CopyTo(ms);
                    //var fileBytes = ms.ToArray();
                    //fileByteArray = Convert.ToBase64String();
                    // act on the Base64 data

                    // To save the newly added Mobile and the Image disk imagePath to Database table (Mobiles)
                    */
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();

                // Insert the form data into mobiles table but not the picture
                string query = "INSERT INTO Mobiles (MobileName, Price, Quantity, Description, PicURL, Model, Features, Color, SimType) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?)";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = mobilesModel.MobileName;
                cmd.Parameters.Add("price", IfxType.Decimal).Value = mobilesModel.Price;
                cmd.Parameters.Add("quantity", IfxType.Int).Value = mobilesModel.Quantity;
                cmd.Parameters.Add("description", IfxType.VarChar).Value = mobilesModel.Description;
                cmd.Parameters.Add("picurl", IfxType.VarChar).Value = mobilesModel.PicURL;
                cmd.Parameters.Add("model", IfxType.VarChar).Value = mobilesModel.Model;
                cmd.Parameters.Add("features", IfxType.VarChar).Value = mobilesModel.Features;
                cmd.Parameters.Add("color", IfxType.VarChar).Value = mobilesModel.Color;
                cmd.Parameters.Add("simtype", IfxType.VarChar).Value = mobilesModel.SimType;
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                // Getting the latest inserted row's slno to insert the picture in the same row
                string selQuery = "Select max(slno) from Mobiles";
                IfxCommand selcmd = new IfxCommand(selQuery, Con);
                int serialnumber = -1;
                try
                {
                    IfxDataReader rows = selcmd.ExecuteReader();
                    while (rows.Read())
                    {
                        serialnumber = Convert.ToInt32(rows[0]);
                    }
                    rows.Close();
                    selcmd.Dispose();

                    string updatePicQuery = "update mobiles set(imagefile) = (Filetoblob(" + "'" + imagePath + "'" + ", 'client', 'mobiles', 'imagefile')) where slno = ?";
                    IfxCommand insertPiccmd = new IfxCommand(updatePicQuery, Con);
                    insertPiccmd.Parameters.Add("slno", IfxType.Int).Value = serialnumber;
                    insertPiccmd.ExecuteNonQuery();
                    insertPiccmd.Dispose();

                    // Delete the temprary created image file from Disk
                    
                    FileInfo file = new FileInfo(imagePath);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    
                }
                catch (IfxException ex)
                {
                    
                }
                finally
                {
                   Con.Close();
                }
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
                Con.Close();
            }
            if (mobileTable.Rows.Count == 1)
            {
                mobile.SLNo = Convert.ToInt32(mobileTable.Rows[0][0].ToString());
                mobile.MobileName = mobileTable.Rows[0][1].ToString();
                mobile.Price = Convert.ToDecimal(mobileTable.Rows[0][2].ToString());
                mobile.Quantity = Convert.ToInt32(mobileTable.Rows[0][3].ToString());
                mobile.Description = mobileTable.Rows[0][4].ToString();
                mobile.PicURL = mobileTable.Rows[0][5].ToString();
                mobile.Model = mobileTable.Rows[0][6].ToString();
                mobile.Features = mobileTable.Rows[0][7].ToString();
                mobile.Color = mobileTable.Rows[0][8].ToString();
                mobile.SimType = mobileTable.Rows[0][9].ToString();
                return View(mobile);
            }
            else
                return RedirectToAction("Index");
        }

        // POST: /Mobiles/Edit/5
        [HttpPost]
        public ActionResult Edit(Mobiles mobile)
        {
            // To create a Unique file name and URL everytime when User upload a new picture
            string ImageFileName = Path.GetFileNameWithoutExtension(mobile.ImageFile.FileName);
            string ImageFileExtension = Path.GetExtension(mobile.ImageFile.FileName);
            string FinalImageName = ImageFileName + DateTime.Now.ToString("yymmssfff") + ImageFileExtension;
            mobile.PicURL = FinalImageName;

            // To save that newly uploaded image to Disk location inside wwwroot/Images folder
            var uploads = Path.Combine(hostingEnvironment.WebRootPath, "Images");
            var imagePath = Path.Combine(uploads, FinalImageName);
            mobile.ImageFile.CopyTo(new FileStream(imagePath, FileMode.Create));

            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                string query = "UPDATE Mobiles SET MobileName = ? , Price= ? , Quantity = ? , Description = ? , PicURL = ? , Model = ? , " +
                    "Features = ? , Color = ? , SimType = ?  Where SLNo = ?";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = mobile.MobileName;
                cmd.Parameters.Add("price", IfxType.Decimal).Value = mobile.Price;
                cmd.Parameters.Add("quantity", IfxType.Int).Value = mobile.Quantity;
                cmd.Parameters.Add("description", IfxType.VarChar).Value = mobile.Description;
                cmd.Parameters.Add("picurl", IfxType.VarChar).Value = mobile.PicURL;
                cmd.Parameters.Add("model", IfxType.VarChar).Value = mobile.Model;
                cmd.Parameters.Add("features", IfxType.VarChar).Value = mobile.Features;
                cmd.Parameters.Add("color", IfxType.VarChar).Value = mobile.Color;
                cmd.Parameters.Add("simtype", IfxType.VarChar).Value = mobile.SimType;
                cmd.Parameters.Add("slno", IfxType.Serial).Value = mobile.SLNo;
                cmd.ExecuteNonQuery();
                Con.Close();
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
                string query = "DELETE FROM Mobiles Where SLNo = ?";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("slno", IfxType.Serial).Value = slno;
                cmd.ExecuteNonQuery();
                Con.Close();
            }
            return RedirectToAction("Index");
        }
    }
}