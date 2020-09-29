using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using EcomApplication.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using EcomDAL;

namespace EcomApplication.Controllers
{
    public class CartController : Controller
    {
        string connString = Config.ConnStr;

        public IActionResult Index()
        {
            DataTable table = new DataTable();
            using (IfxConnection Con = new IfxConnection(connString))
            {
                   Con.Open();
                try
                {
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM cart", Con);
                    ifx.Fill(table);
                }
                catch (Exception ex)
                {
                    string createTable = "Create table cart (cartid serial PRIMARY KEY, SLNo int, MobileName nvarchar(100) NULL, " +
                            " Description nvarchar(250) NULL, PicURL nvarchar(250) NULL, Model nvarchar(50) NULL, Features nvarchar(200) NULL, " +
                            "Color nvarchar(20) NULL, SimType nvarchar(10) NULL, Price decimal(18, 2), Quantity int NULL, TotalAmount decimal(18,2))";
                    IfxCommand cmd = new IfxCommand(createTable, Con);
                    cmd.ExecuteNonQuery();
                    IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM cart", Con);
                    ifx.Fill(table);
                }
                finally
                {
                    Con.Close();
                }
            }
            List<Cart> cartList = new List<Cart>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Cart cart = new Cart();
                cart.CartID = Convert.ToInt32(table.Rows[i]["CartID"]);
                cart.SLNo = Convert.ToInt32(table.Rows[i]["SLNo"]);
                cart.MobileName = table.Rows[i]["MobileName"].ToString();
                cart.Description = table.Rows[i]["Description"].ToString();
                cart.PicURL = table.Rows[i]["PicURL"].ToString();
                cart.Model = table.Rows[i]["Model"].ToString();
                cart.Features = table.Rows[i]["Features"].ToString();
                cart.Color = table.Rows[i]["Color"].ToString();
                cart.SimType = table.Rows[i]["SimType"].ToString();
                cart.Price = Convert.ToDecimal(table.Rows[i]["Price"]);
                cart.Quantity = Convert.ToInt32(table.Rows[i]["Quantity"]);
                cart.TotalAmount = Convert.ToDecimal(table.Rows[i]["TotalAmount"]);

                cartList.Add(cart);
            }
            return View(cartList);
        }

        [HttpGet]
        public ActionResult Delete(int cartID)
        {
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                string query = "DELETE FROM Cart Where CartID = ?";
                IfxCommand cmd = new IfxCommand(query, Con);
                cmd.Parameters.Add("cartid", IfxType.Serial).Value = cartID;
                cmd.ExecuteNonQuery();
                Con.Close();
            }
            return RedirectToAction("Index");
        }
    }
}