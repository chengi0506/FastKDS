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
using System.Data.Entity;
using System.IO;
using System.Text;

namespace FastKDS.Controllers
{
    public class OrderController : ApiController
    {
        public enum 訂單狀態
        {
            全部,
            待製作,
            製作中,
            待取餐,
            已完成
        }

        /// <summary>
        /// 取得全部訂單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult getAllOrders()
        {
            return Ok(GenOrdersHtml(false));
        }

        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="currentState"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult UpdateOrderStatus(int orderId, string currentState)
        {
            using (var context = new ApplicationDbContext())
            {
                var order = context.Orders.FirstOrDefault(o => o.OrderID==orderId);

                if (order != null)
                {
                    // 根據當前狀態更新訂單狀態
                    if (currentState == 訂單狀態.待製作.ToString())
                    {
                        order.State = 訂單狀態.製作中.ToString();
                        order.CookTime = DateTime.Now;
                    }
                    else if (currentState == 訂單狀態.製作中.ToString())
                    {
                        order.State = 訂單狀態.待取餐.ToString();
                        order.MakeTime = DateTime.Now;
                    }
                    else if (currentState == 訂單狀態.待取餐.ToString())
                    {
                        order.State = 訂單狀態.已完成.ToString();
                        order.TakeTime = DateTime.Now;
                    }

                    context.SaveChanges();
                }
            }
                
            return Ok(GenOrdersHtml(false));
        }

        /// <summary>
        /// 建立訂單
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        // POST api/CreateNewOrder
        [HttpPost]
        //[Route("api/[controller]")]
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

                        GenOrdersHtml(true);

                        return Ok();
                    }
                    
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

        /// <summary>
        /// 列印訂單
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        //[Route("api/[controller]/printorder")]
        public IHttpActionResult PrintOrder(int orderId)
        {
            try
            {
                string text = string.Empty;
                using (var context = new ApplicationDbContext())
                {
                    //取得訂單資料
                    var orders = context.Orders
                        .Include(o => o.OrderDetail)
                        .SingleOrDefault(o => o.OrderID==orderId);

                    if (orders != null)
                    {
                        string pickNum = string.Empty;
                        string note = string.Empty;

                        // 先檢查字串中是否包含冒號
                        if (orders.Note.Contains(":"))
                        {
                            // 使用 Split 方法分割字串
                            string[] parts = orders.Note.Split(':');

                            // 檢查是否有足夠的部分
                            if (parts.Length == 2 && parts[1].Length == 3)
                            {
                                note = parts[0];
                                pickNum = parts[1];
                            }
                            else
                            {
                                note = orders.InvoNo;
                            }
                        }

                        text += $"取餐編號: {pickNum}{Environment.NewLine}訂單時間: {orders.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}訂單編號: {orders.PosID.ToString()}{Environment.NewLine}發票號碼: {orders.InvoNo}{Environment.NewLine}訂單備註: {note}{Environment.NewLine}---------------------------{Environment.NewLine}品名        數量        備註{Environment.NewLine}---------------------------{Environment.NewLine}";

                        var latestOrderDetail = context.OrderDetails.Where(o => o.OrderID == orders.OrderID).ToList();
                        int Pno = 1;
                        foreach (var orderDetail in latestOrderDetail)
                        {
                            string noteText = string.IsNullOrEmpty(orderDetail.Note) ? "" : $" {orderDetail.Note}";
                            string line = string.Empty;

                            //如果為「組合▲」時只顯示品名
                            if (orderDetail.Name.Contains("組合▲"))
                            {
                                line = $"{orderDetail.Name}";
                                Pno--;
                            }
                            else
                            {
                                line = $"{Pno}.{orderDetail.Name} X {orderDetail.Quantity}{noteText}";
                            }

                            // 判斷字串轉換為字節陣列後的字數
                            int maxLengthInBytes = 30;
                            int totalBytes = Encoding.UTF8.GetByteCount(line);

                            if (totalBytes <= maxLengthInBytes)
                            {
                                text += line + Environment.NewLine;
                            }
                            else
                            {
                                int startIndex = 0;
                                while (startIndex < line.Length)
                                {
                                    int endIndex = startIndex;
                                    int currentBytes = 0;

                                    while (endIndex < line.Length && currentBytes <= maxLengthInBytes)
                                    {
                                        currentBytes += Encoding.UTF8.GetByteCount(new char[] { line[endIndex] });
                                        endIndex++;
                                    }

                                    text += line.Substring(startIndex, endIndex - startIndex) + Environment.NewLine;
                                    startIndex = endIndex;
                                }
                            }

                            Pno++;
                        }

                        text += $"---------------------------{Environment.NewLine}取餐機震動後請至櫃台取餐{Environment.NewLine}";
                    }

                }

                // 指定目錄位置
                string printDir = Properties.Settings.Default.processPath;

                // 檢查目錄是否存在
                if (!Directory.Exists(printDir))
                {
                    Directory.CreateDirectory(printDir);
                }

                // 寫入訂單文字檔
                string fileName = $"KDS_{DateTime.Now:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")}";
                string filePath = Path.Combine(printDir, fileName);
                System.IO.File.WriteAllText(filePath, text);

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 產生訂單Html
        /// </summary>
        private string GenOrdersHtml(Boolean showMessage)
        {
            using (var context = new ApplicationDbContext())
            {     
                DateTime currentDate = DateTime.Now.Date; // 取得當前日期的開始時間
                DateTime nextDate = currentDate.AddDays(1); // 取得當前日期的下一天的開始時間

                //取得當日所有訂單資料
                var orders = context.Orders
                    .Include(o => o.OrderDetail)
                    .Where(o => o.DateTime >= currentDate && o.DateTime < nextDate)
                    .OrderByDescending(o => o.DateTime)
                    .ThenByDescending(o => o.OrderID)
                    .ToList();


                string html = String.Empty;
                int index = 0;
                while (index <= 4)
                {
                    // 應用通用條件，包括日期和排序
                    var filter = orders.ToList();

                    switch (index)
                    {
                        case (int)訂單狀態.待製作:
                            filter = filter.Where(o => o.State == 訂單狀態.待製作.ToString()).ToList();
                            break;
                        case (int)訂單狀態.製作中:
                            filter = filter.Where(o => o.State == 訂單狀態.製作中.ToString()).ToList();
                            break;
                        case (int)訂單狀態.待取餐:
                            filter = filter.Where(o => o.State == 訂單狀態.待取餐.ToString()).ToList();
                            break;
                        case (int)訂單狀態.已完成:
                            filter = filter.Where(o => o.State == 訂單狀態.已完成.ToString()).ToList();
                            break;
                        default:
                            break;
                    }

                    for (int i = 0; i < filter.Count; i++)
                    {
                        string filtertatusHtml = GenerateOrderStatusHtml(filter[i].State, filter[i].DateTime, filter[i].CookTime, filter[i].MakeTime, filter[i].TakeTime);

                        string pickNum = string.Empty;
                        string note = string.Empty;

                        // 先檢查字串中是否包含冒號
                        if (filter[i].Note.Contains(":"))
                        {
                            // 使用 Split 方法分割字串
                            string[] parts = filter[i].Note.Split(':');

                            // 檢查是否有足夠的部分
                            if (parts.Length == 2 && parts[1].Length == 3)
                            {
                                note = parts[0];
                                pickNum = parts[1];
                            }
                            else
                            {
                                note = filter[i].Note;
                            }
                        }

                        html += $@"
                                    <tr>
                                        <td>
                                            <div class='col-12'>
                                                <div class='bs-stepper linear' style='flex-direction: row;'>
                                                    <div class='card-header row row-cols-md-12'>
                                                      <h3 class='card-title'>
                                                          <div class='col-md-12'>
                                                            <label>訂單編號:{pickNum}</label>&nbsp;&nbsp;
                                                            <label>發票號碼:{filter[i].InvoNo}</label>
                                                          </div>
                                                      </h3>
                                                    </div>
                                                    <div class='card-body'>
                                                      <div class='bs-stepper-header' role='tablist'>
                                                        {filtertatusHtml}  
                                                      </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </td>
                                        <td>
                                            <div class='col'>
                                                <ul>
                                                    {string.Join("", filter[i].OrderDetail.Select(item => $"<li>{item.Name} X {item.Quantity} {item.Note}</li>"))}
                                                </ul>
                                            </div>
                                        </td>
                                        <td><div class='col'>{note}</div></td>
                                        <td>
                                            <div class='col'>
                                            {(filter[i].State != "已完成" ? "<button class='btn btn-block btn-outline-danger' onclick='updateOrderStatus(" + filter[i].OrderID + ", \"" + filter[i].State + "\")'>下一步</button><br><br>" : "")}
                                            <button class='btn btn-block btn-outline-info' onclick='printOrder(""{filter[i].OrderID}"")'>列印</button>
                                            </div>
                                        </td>
                                    </tr>
                        ";
                    }
                    index++;
                    html += "||";
                }

                var stats = new
                {
                    AllOrdersCount = orders.Count,
                    NewOrdersCount = orders.Count(o => o.State == 訂單狀態.待製作.ToString()),
                    InProgressOrdersCount = orders.Count(o => o.State == 訂單狀態.製作中.ToString()),
                    WaitingForPickupOrdersCount = orders.Count(o => o.State == 訂單狀態.待取餐.ToString()),
                    CompletedOrdersCount = orders.Count(o => o.State == 訂單狀態.已完成.ToString())
                };

                html += JsonConvert.SerializeObject(stats);
                html += "||";

                //訂單通知訊息
                if (showMessage == true)
                {
                    //取得最新一筆訂單
                    var latestOrder = context.Orders.OrderByDescending(o => o.DateTime).FirstOrDefault();
                    html += $"{latestOrder.DateTime.ToString("HH:mm:ss")}新訂單【{latestOrder.PosID}】通知!";
                    html += "||";

                    if (latestOrder != null)
                    {
                        string text = $"訂單時間: {latestOrder.DateTime.ToString("HH:mm:ss")}{Environment.NewLine}訂單編號: {latestOrder.PosID}{Environment.NewLine}訂單備註: {latestOrder.Note}{Environment.NewLine}-----------------------{Environment.NewLine}";

                        var latestOrderDetail = context.OrderDetails.Where(o => o.OrderID == latestOrder.OrderID).ToList();
                        int Pno = 1;
                        foreach (var orderDetail in latestOrderDetail)
                        {
                            text += $"{Pno}.{orderDetail.Name} X {orderDetail.Quantity}{Environment.NewLine}{orderDetail.Note}{Environment.NewLine}";
                            Pno++;
                        }

                        html += text + "||" + latestOrder.OrderID + "||";
                    }
                }

                // 向客户端廣播
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                hubContext.Clients.All.addNewMessageToPage(html);
                return html;
            }
        }

        /// <summary>
        /// GenerateOrderStatusHtml
        /// </summary>
        /// <param name="orderState"></param>
        /// <param name="DateTime"></param>
        /// <returns></returns>
        private string GenerateOrderStatusHtml(string orderState, DateTime DateTime, DateTime? CookTime, DateTime? MakeTime, DateTime? TakeTime)
        {
            string activeHtml = "<div class='step active' data-target='#logins-part'>";
            string disabledHtml = "<div class='step' data-target='#information-part'>";

            string orderStatusHtml = String.Empty;

            switch (orderState)
            {
                case "待製作":
                    orderStatusHtml = $@"
                {activeHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='logins-part' id='logins-part-trigger' aria-selected='true'>
                        <span class='bs-stepper-circle'>1</span>
                        <span class='bs-stepper-label'>待製作</span>
                        <span class='bs-stepper-label'>{DateTime.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>2</span>
                        <span class='bs-stepper-label'>製作中</span>
                        <span class='bs-stepper-label'>{CookTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>3</span>
                        <span class='bs-stepper-label'>待取餐</span>
                        <span class='bs-stepper-label'>{MakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>4</span>
                        <span class='bs-stepper-label'>已完成</span>
                        <span class='bs-stepper-label'>{TakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
            ";
                    break;
                case "製作中":
                    orderStatusHtml = $@"
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>1</span>
                        <span class='bs-stepper-label'>待製作</span>
                        <span class='bs-stepper-label'>{DateTime.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {activeHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='logins-part' id='logins-part-trigger' aria-selected='true'>
                        <span class='bs-stepper-circle'>2</span>
                        <span class='bs-stepper-label'>製作中</span>
                        <span class='bs-stepper-label'>{CookTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>3</span>
                        <span class='bs-stepper-label'>待取餐</span>
                        <span class='bs-stepper-label'>{MakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>4</span>
                        <span class='bs-stepper-label'>已完成</span>
                        <span class='bs-stepper-label'>{TakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
            ";
                    break;
                case "待取餐":
                    orderStatusHtml = $@"
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>1</span>
                        <span class='bs-stepper-label'>待製作</span>
                        <span class='bs-stepper-label'>{DateTime.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>2</span>
                        <span class='bs-stepper-label'>製作中</span>
                        <span class='bs-stepper-label'>{CookTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {activeHtml}
                    <button type='button' class='step-trigger' role='tab'  aria-controls='logins-part' id='logins-part-trigger' aria-selected='true'>
                        <span class='bs-stepper-circle'>3</span>
                        <span class='bs-stepper-label'>待取餐</span>
                        <span class='bs-stepper-label'>{MakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>4</span>
                        <span class='bs-stepper-label'>已完成</span>
                        <span class='bs-stepper-label'>{TakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
            ";
                    break;
                case "已完成":
                    orderStatusHtml = $@"
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>1</span>
                        <span class='bs-stepper-label'>待製作</span>
                        <span class='bs-stepper-label'>{DateTime.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>2</span>
                        <span class='bs-stepper-label'>製作中</span>
                        <span class='bs-stepper-label'>{CookTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {disabledHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='information-part' id='information-part-trigger' aria-selected='false' disabled>
                        <span class='bs-stepper-circle'>3</span>
                        <span class='bs-stepper-label'>待取餐</span>
                        <span class='bs-stepper-label'>{MakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
                <div class='line'></div>
                {activeHtml}
                    <button type='button' class='step-trigger' role='tab' aria-controls='logins-part' id='logins-part-trigger' aria-selected='true'>
                        <span class='bs-stepper-circle'>4</span>
                        <span class='bs-stepper-label'>已完成</span>
                        <span class='bs-stepper-label'>{TakeTime?.ToString("HH:mm:ss") ?? "N/A"}</span>
                    </button>
                </div>
            ";
                    break;
                default:
                    break;
            }

            return orderStatusHtml;
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