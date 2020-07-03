# demo-ASPCore-Ecommerce
ECommerce web application in ASP .Net Core (MVC) without EF core.

## Prerequisites

* Visual Studio 2019 
* ASP.NET Core 3.1 
* .Net Core 3.1
* HCL Informix

## How to run the project

* Git clone this project to a location in your disk.
* Open the solution file EcomApplication.sln using the Visual Studio 2019.
* Restore the NuGet packages by rebuilding the solution.
* Change the connection string in all the Controller.cs file.
* Run the project(it will open a web browser with application running on it).

## Agenda

* Performing CURD Operation on Inventory for Shop Admin.
* Displaying product details from the database.
* Showing particular product details from database.
* Adding selected and purchase item to the shopping cart.
* Showing all recently added cart items.
* Removing items from a cart.
* Confirming the purchase.
* Making Payment through Card.
* Updating the summary of all placed orders in Order Details.


##Application Functinality

**Mobile Shop-**Allows online shopping customers to accumulate a list of items for purchase, once user will click on the item it will redirect the user to **Product Specification** page with all the mobile details, from where user can select the quantity and place the items in the Shopping Cart or Add to Cart and Continue Shooping will redirect the user to the shopping page.

**Shopping Cart-**Displays the list of all product details from the database with the total amount according to the quantity selected. Upon checkout,it typically calculates a total amount of each product for the order, and redirect the user to the payment page. User can also delete the particular product from Cart.

**Payment-**Displays the Total Amount to be paid for the order and user can enter the credit/debit card details and Make Payment. After making Payment user will be redirected to the MyOrders Page with the Order summary and order number.

**My Orders-**Displays the summary of all placed orders with Order Number and Total amount.

**Inventory-**It is for the Mobile Shop admin to Add,Delete,Update and add picture of the product for the mobile shop.



