using FastKDS.DAL;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FastKDS.Models;
using FastKDS.Hubs;
using Newtonsoft.Json.Linq;

namespace FastKDS.Controllers
{
    public class OrderController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetNewOrder()
        {
            using (var context = new ApplicationDbContext())
            {
                var latestOrder = context.Orders.OrderByDescending(o => o.DateTime).FirstOrDefault();

                if (latestOrder != null)
                {
                    return Ok(latestOrder);
                }
            }
            return NotFound();
        }

        // POST api/<controller>
        [HttpPost]
        public IHttpActionResult CreateNewOrder([FromBody] JObject jsonData)
        {
            try
            {
                // 將JSON數據反序列化為Orders對象
                var orders = JsonConvert.DeserializeObject<Orders>(jsonData["order"].ToString());
                var orderDetails = JsonConvert.DeserializeObject<List<OrderDetail>>(jsonData["orderDetails"].ToString());

                if (ModelState.IsValid)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        // 添加訂單主文件
                        context.Orders.Add(orders);

                        // 添加訂單明細
                        if (orderDetails != null)
                        {
                            foreach (var orderDetail in orderDetails)
                            {
                                // 設置訂單主文件的外鍵以關聯訂單明細
                                orderDetail.OrderID = orders.OrderID;
                                context.OrderDetails.Add(orderDetail);
                            }
                        }

                        context.SaveChanges();
                    }

                    // 取得最新一筆訂單
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

                            // 向客戶端廣播通知
                            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                            hubContext.Clients.All.addNewMessageToPage("訂單通知", message);

                            return Ok(latestOrder);
                        }
                    }

                    return Ok(new { success = false });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}