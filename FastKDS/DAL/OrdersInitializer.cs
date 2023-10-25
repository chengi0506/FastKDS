using FastKDS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FastKDS.DAL
{
    public class OrdersInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            context.Orders.Add(
                new Orders(1, DateTime.Now, "內用", new List<OrderDetail>
                {
                    new OrderDetail(1, "芋頭冰淇淋", 1,"杯裝"),
                    new OrderDetail(2, "牛奶冰淇淋", 2,"杯裝")
                }, "等待製作")
            );

            context.Orders.Add(
                new Orders(2, DateTime.Now, "外帶", new List<OrderDetail>
                {
                    new OrderDetail(1, "鍋燒意麵", 1,"麵另外裝"),
                    new OrderDetail(2, "紅茶", 2,"去冰微糖")
                }, "製作中")
            );

            context.SaveChanges();
        }
    }
}