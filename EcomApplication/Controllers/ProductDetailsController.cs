using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcomApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using System.Data;

namespace EcomApplication.Controllers
{
    public class ProductDetailsController : Controller
    {
        string connString = "DataBase=webapp;Server=ol_informix1410_9;User ID = informix; Password=Rinvoke1;";
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult EachProductDetails(Mobiles mobile)
        {
            DataTable mobileTable = new DataTable();
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                // Prone to SQL enjection
                string query = "SELECT * FROM Mobiles Where SLNo = ?";
                IfxDataAdapter ifx = new IfxDataAdapter(query, Con);
                ifx.SelectCommand.Parameters.Add("SLNo", IfxType.Serial).Value = mobile.SLNo;
                ifx.Fill(mobileTable);
            }

            List<Mobiles> mobileDetails = new List<Mobiles>();

            for (int i = 0; i < mobileTable.Rows.Count; i++)
            {
                Mobiles mobileDetail = new Mobiles();
                mobileDetail.SLNo = Convert.ToInt32(mobileTable.Rows[0][0].ToString());
                mobileDetail.MobileName = mobileTable.Rows[0][1].ToString();
                mobileDetail.Price = Convert.ToDecimal(mobileTable.Rows[0][2].ToString());
                mobileDetail.Quantity = Convert.ToInt32(mobileTable.Rows[0][3].ToString());
                mobileDetail.Description = mobileTable.Rows[0][4].ToString();
                mobileDetail.PicURL = mobileTable.Rows[0][5].ToString();
                mobileDetail.Model = mobileTable.Rows[0][6].ToString();
                mobileDetail.Features = mobileTable.Rows[0][7].ToString();
                mobileDetail.Color = mobileTable.Rows[0][8].ToString();
                mobileDetail.SimType = mobileTable.Rows[0][9].ToString();
                mobileDetails.Add(mobileDetail);
            }
            return View(mobileDetails);
        }
    }
}