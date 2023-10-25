using FastKDS.DAL;
using FastKDS.Hubs;
using FastKDS.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FastKDS.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateNewOrder()
        {
            var newOrder = new Orders
            {
                OrderDetail = new List<OrderDetail> { new OrderDetail() } // 创建一个空的订单明细
            };

            return View(newOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewOrder(Orders order)
        {
            if (ModelState.IsValid)
            {
                // 在这里将订单保存到数据库
                using (var context = new ApplicationDbContext())
                {
                    context.Orders.Add(order);
                    context.SaveChanges();
                }

                // 重定向到订单列表或其他适当的页面
                return RedirectToAction("Index");
            }

            // 向客户端廣播通知
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            hubContext.Clients.All.addNewMessageToPage("系统", "新訂單已添加");

            // 如果模型验证失败，返回创建订单的页面，以便用户重新输入数据
            return View(order);
        }

        [HttpPost]
        public JsonResult AddNewOrder(Orders order)
        {
            
            // 向客户端廣播通知
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            hubContext.Clients.All.addNewMessageToPage("系统", "新訂單已添加");

            return Json(new { success = true });
        }
    }
}