using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Informix.Net.Core;
using EcomApplication.Models;
using System.Data;
using EcomDAL;

namespace EcomApplication.Controllers
{
    public class MyOrdersController : Controller
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
                return View(table);
            }
        }

        [HttpGet]
        public ActionResult CreateOrder()
        {
            using (IfxConnection Con = new IfxConnection(connString))
            {
                Con.Open();
                DataTable cartTable = new DataTable();

                IfxDataAdapter ifx = new IfxDataAdapter("SELECT * FROM cart", Con);
                ifx.Fill(cartTable);
                Con.Close();

                List<MyOrders> orderList = new List<MyOrders>();

                for (int i = 0; i < cartTable.Rows.Count; i++)
                {
                    MyOrders order = new MyOrders();
                    order.PurchaseDate = DateTime.UtcNow.ToString();
                    order.SLNo = Convert.ToInt32(cartTable.Rows[i]["SLNo"]);
                    order.MobileName = cartTable.Rows[i]["MobileName"].ToString();
                    order.Description = cartTable.Rows[i]["Description"].ToString();
                    order.PicURL = cartTable.Rows[i]["PicURL"].ToString();
                    order.Model = cartTable.Rows[i]["Model"].ToString();
                    order.Features = cartTable.Rows[i]["Features"].ToString();
                    order.Color = cartTable.Rows[i]["Color"].ToString();
                    order.SimType = cartTable.Rows[i]["SimType"].ToString();
                    order.Price = Convert.ToDecimal(cartTable.Rows[i]["Price"]);
                    order.Quantity = Convert.ToInt32(cartTable.Rows[i]["Quantity"]);
                    order.TotalAmount = Convert.ToDecimal(cartTable.Rows[i]["TotalAmount"]);

                    orderList.Add(order);
                }

                foreach (MyOrders order in orderList)
                {
                    Con.Open();
                    int SLNo = order.SLNo;
                    int availableQuantity = 0;

                    string selectMobileDetails = "select Quantity from mobiles where SLNo = ?";
                    IfxCommand cmd = new IfxCommand(selectMobileDetails, Con);
                    cmd.Parameters.Add("slno", IfxType.Serial).Value = SLNo;
                    try
                    {
                        IfxDataReader rows = cmd.ExecuteReader();
                        while (rows.Read())
                        {
                            availableQuantity = Convert.ToInt32(rows[0]);
                        }
                        rows.Close();
                    }
                    catch (IfxException ex)
                    {
                        Con.Close();
                        order.ErrorMessage = "Error : " + ex.Message;
                    }

                    if (order.Quantity > availableQuantity)
                    {
                        Con.Close();
                        order.ErrorMessage = "Cannot purchase " + order.Quantity + " quantities, available quantities are : " + availableQuantity;
                    }
                    else
                    {
                        int newMobileQuantity = availableQuantity - order.Quantity;

                        string updateMobileQuantity = "UPDATE Mobiles SET Quantity = ? Where SLNo = ?";
                        IfxCommand cmd1 = new IfxCommand(updateMobileQuantity, Con);
                        cmd1.Parameters.Add("quantity", IfxType.Int).Value = newMobileQuantity;
                        cmd1.Parameters.Add("slno", IfxType.Serial).Value = SLNo;
                        cmd1.ExecuteNonQuery();

                        try
                        {
                            insertNewOrder(Con, order);
                        }
                        catch (Exception ex)
                        {
                            string createOrderTable = "Create table orderdetails (orderid serial PRIMARY KEY, SLNo int, MobileName nvarchar(100) NULL, " +
                            " Description nvarchar(250) NULL, PicURL nvarchar(250) NULL, Model nvarchar(50) NULL, Features nvarchar(200) NULL, " +
                            "Color nvarchar(20) NULL, SimType nvarchar(10) NULL, PurchaseDate varchar(50), Price decimal(18, 2), Quantity int NULL, TotalAmount decimal(18,2))";

                            IfxCommand cmd2 = new IfxCommand(createOrderTable, Con);
                            cmd2.ExecuteNonQuery();
                            insertNewOrder(Con, order);
                        }
                        finally
                        {
                            Con.Close();
                            order.ErrorMessage = "Purchase successful";
                        }
                    }
                    Con.Close();
                }
                Con.Open();
                string delQuery = "DELETE FROM Cart";
                IfxCommand delCmd = new IfxCommand(delQuery, Con);
                delCmd.ExecuteNonQuery();
                Con.Close();
            }
            return RedirectToAction("Index");
        }

        private void insertNewOrder(IfxConnection con, MyOrders order)
        {
            string query = "INSERT INTO orderdetails (purchasedate, slno, mobilename, description, picurl, model, features, color, simtype, price, quantity, totalamount)" +
                " VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            IfxCommand cmd = new IfxCommand(query, con);

            cmd.Parameters.Add("purchasedate", IfxType.DateTime).Value = order.PurchaseDate;
            cmd.Parameters.Add("slno", IfxType.Int).Value = order.SLNo;
            cmd.Parameters.Add("mobilename", IfxType.VarChar).Value = order.MobileName;
            cmd.Parameters.Add("description", IfxType.VarChar).Value = order.Description;
            cmd.Parameters.Add("picurl", IfxType.VarChar).Value = order.PicURL;
            cmd.Parameters.Add("model", IfxType.VarChar).Value = order.Model;
            cmd.Parameters.Add("features", IfxType.VarChar).Value = order.Features;
            cmd.Parameters.Add("color", IfxType.VarChar).Value = order.Color;
            cmd.Parameters.Add("simtype", IfxType.VarChar).Value = order.SimType;
            cmd.Parameters.Add("price", IfxType.Decimal).Value = order.Price;
            cmd.Parameters.Add("quantity", IfxType.Int).Value = order.Quantity;
            cmd.Parameters.Add("totalamount", IfxType.Decimal).Value = order.TotalAmount;

            cmd.ExecuteNonQuery();
        }
    }
}