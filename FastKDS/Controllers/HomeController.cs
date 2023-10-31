using FastKDS.DAL;
using FastKDS.Hubs;
using FastKDS.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FastKDS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public ActionResult Index()
        {
            // 創建一些假資料示例
            //var orders = new List<Orders>
            //{
            //    new Orders(1, DateTime.Now, "內用", new List<ItemModel>
            //    {
            //        new ItemModel(1, "芋頭冰淇淋", 1,"杯裝"),
            //        new ItemModel(2, "牛奶冰淇淋", 2,"杯裝")
            //    },"待製作"),

            //    new Orders(2, DateTime.Now, "外帶", new List<ItemModel>
            //    {
            //        new ItemModel(1, "鍋燒意麵", 1,"麵另外裝"),
            //        new ItemModel(2, "紅茶", 2,"去冰微糖")
            //    },"製作中")
            //};

            // 從資料庫檢索訂單數據
            var orders = context.Orders.Include("OrderDetail").ToArray();

            return View(orders);  // 將資料傳遞給視圖
        }

        public ActionResult Chat()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateNewOrder()
        {
            //取得最新一筆訂單
            using (var context = new ApplicationDbContext())
            {
                var latestOrder = context.Orders.OrderByDescending(o => o.DateTime).FirstOrDefault();

                if (latestOrder != null)
                {
                    string message = $"訂單時間: {latestOrder.DateTime.ToString("HH:mm:ss")}{Environment.NewLine}" +
                                     $"訂單編號: {latestOrder.OrderID}{Environment.NewLine}" +
                                     $"訂單備註: {latestOrder.Note}{Environment.NewLine}" +
                                     $"-----------------------{Environment.NewLine}";

                    var latestOrderDetail = context.OrderDetails.Where(o => o.OrderID == latestOrder.OrderID).ToList();
                    int Pno = 1;
                    foreach (var orderDetail in latestOrderDetail)
                    {
                        message += $"{Pno}.{orderDetail.Name} X {orderDetail.Quantity} ({orderDetail.Note}){Environment.NewLine}";
                        Pno++;
                    }

                    // 向客户端廣播通知
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hubContext.Clients.All.addNewMessageToPage("訂單通知", message);
                    
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, string currentState)
        {
            var order = context.Orders.FirstOrDefault(o => o.OrderID == orderId);

            if (order != null)
            {
                // 根據當前狀態更新訂單狀態
                if (currentState == "待製作")
                {
                    order.State = "製作中";
                    order.CookTime = DateTime.Now;
                }
                else if (currentState == "製作中")
                {
                    order.State = "待取餐";
                    order.MakeTime = DateTime.Now;
                }
                else if (currentState == "待取餐")
                {
                    order.State = "已完成";
                    order.TakeTime = DateTime.Now;
                }

                context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}