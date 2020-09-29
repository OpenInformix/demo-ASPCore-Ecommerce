using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcomApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using System.Data;
using EcomDAL;

namespace EcomApplication.Controllers
{
    public class ProductDetailsController : Controller
    {
        string connString = Config.ConnStr;
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
                Con.Close();
            }

            Mobiles mobileDetail = new Mobiles();
            for (int i = 0; i < mobileTable.Rows.Count; i++)
            {
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
            }
            return View(mobileDetail);
        }

        public ActionResult AddToCart(Mobiles mobileDetails)
        {
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                int selectedQuantity = mobileDetails.Quantity;

                Mobiles mobile = new Mobiles();
                DataTable mobileTable = new DataTable();

                string query = "SELECT * FROM Mobiles Where SLNo = ?";
                IfxDataAdapter ifx = new IfxDataAdapter(query, Con);
                ifx.SelectCommand.Parameters.Add("SLNo", IfxType.Serial).Value = mobileDetails.SLNo;
                ifx.Fill(mobileTable);

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
                }
                else
                {
                    Con.Close();
                    mobile.ErrorMessage = "Error : Unable to get mobile details";
                }

                if (selectedQuantity > mobile.Quantity)
                {
                    Con.Close();
                    mobile.ErrorMessage = "Cannot purchase " + selectedQuantity + " quantities, available quantities are : " + mobile.Quantity;
                }
                else
                {
                    try
                    {
                        searchSLNoInCartTable(Con, mobileDetails.SLNo, mobile, selectedQuantity);
                    }
                    catch (Exception ex)
                    {
                        string createCartTable = "Create table cart (cartid serial PRIMARY KEY, SLNo int, MobileName nvarchar(100) NULL, " +
                            " Description nvarchar(250) NULL, PicURL nvarchar(250) NULL, Model nvarchar(50) NULL, Features nvarchar(200) NULL, " +
                            "Color nvarchar(20) NULL, SimType nvarchar(10) NULL, Price decimal(18, 2), Quantity int NULL, TotalAmount decimal(18,2))";

                        IfxCommand cmd2 = new IfxCommand(createCartTable, Con);
                        cmd2.ExecuteNonQuery();

                        searchSLNoInCartTable(Con, mobileDetails.SLNo, mobile, selectedQuantity);
                    }
                    finally
                    {
                        Con.Close();
                        mobile.ErrorMessage = "Added to cart successfully";
                    }
                }
                // return View(mobile);
                return RedirectToAction("Index", "Home");
            }
        }

        private void searchSLNoInCartTable(IfxConnection con, int sLNo, Mobiles mobile, int selectedQuantity)
        {
            DataTable cartTable = new DataTable();
            string cartQuery = "SELECT * FROM Cart Where SLNo = ?";
            IfxDataAdapter ifxCart = new IfxDataAdapter(cartQuery, con);
            ifxCart.SelectCommand.Parameters.Add("SLNo", IfxType.Serial).Value = sLNo;
            ifxCart.Fill(cartTable);

            if (cartTable.Rows.Count == 1)
            {
                int SavedQuantity = Convert.ToInt32(cartTable.Rows[0][10].ToString());
                decimal ItemPrice = Convert.ToDecimal(cartTable.Rows[0][9].ToString());
                updateToCart(con, mobile, selectedQuantity, SavedQuantity, ItemPrice);
            }
            else
            {
                insertToCart(con, mobile, selectedQuantity);
            }

        }

        private void updateToCart(IfxConnection con, Mobiles mobile, int selectedQuantity, int savedQuantity, decimal itemPrice)
        {
            int totalQuantity = selectedQuantity + savedQuantity;
            decimal totalamount = itemPrice * totalQuantity;
            string updateQuery = "UPDATE Cart SET (Quantity, TotalAmount) = (?, ?) Where SLNo = ?";
            IfxCommand cmd = new IfxCommand(updateQuery, con);

            cmd.Parameters.Add("quantity", IfxType.Int).Value = totalQuantity;
            cmd.Parameters.Add("totalamount", IfxType.Decimal).Value = totalamount;
            cmd.Parameters.Add("slno", IfxType.Int).Value = mobile.SLNo;
            cmd.ExecuteNonQuery();
        }

        private void insertToCart(IfxConnection con, Mobiles mobile, int selectedQuantity)
        {
            decimal totalamount = mobile.Price * selectedQuantity;

            string query = "INSERT INTO cart (SLNo, MobileName, Description, PicURL, Model, Features, Color, SimType, Price, Quantity, TotalAmount) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            IfxCommand cmd = new IfxCommand(query, con);
            cmd.Parameters.Add("slno", IfxType.Int).Value = mobile.SLNo;
            cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = mobile.MobileName;
            cmd.Parameters.Add("description", IfxType.VarChar).Value = mobile.Description;
            cmd.Parameters.Add("picurl", IfxType.VarChar).Value = mobile.PicURL;
            cmd.Parameters.Add("model", IfxType.VarChar).Value = mobile.Model;
            cmd.Parameters.Add("features", IfxType.VarChar).Value = mobile.Features;
            cmd.Parameters.Add("color", IfxType.VarChar).Value = mobile.Color;
            cmd.Parameters.Add("simtype", IfxType.VarChar).Value = mobile.SimType;
            cmd.Parameters.Add("price", IfxType.Decimal).Value = mobile.Price;
            cmd.Parameters.Add("quantity", IfxType.Int).Value = selectedQuantity;
            cmd.Parameters.Add("totalamount", IfxType.Decimal).Value = totalamount;

            cmd.ExecuteNonQuery();
        }
    }
}