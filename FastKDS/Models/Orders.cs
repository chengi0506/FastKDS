using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FastKDS.Models
{
    public class Orders
    {
        [Key]
        [DisplayName("訂單編號")]
        public int OrderID { get; set; }

        [DisplayName("訂單時間")]
        public DateTime DateTime { get; set; }

        [DisplayName("備註")]
        [StringLength(50)]
        public string Note { get; set; }

        [DisplayName("狀態")]
        [StringLength(10)]
        public string State { get; set; }

        [DisplayName("製作時間")]
        public DateTime? CookTime { get; set; }

        [DisplayName("出餐時間")]
        public DateTime? MakeTime { get; set; }

        [DisplayName("取餐時間")]
        public DateTime? TakeTime { get; set; }

        [DisplayName("明細")]
        public List<OrderDetail> OrderDetail { get; set; }

        public Orders()
        {
            // 空的無參數構造函數
        }

        public Orders(int orderId, DateTime dateTime, string note, List<OrderDetail> items, string state)
        {
            OrderID = orderId;
            DateTime = dateTime;
            Note = note;
            OrderDetail = items;
            State = state;
        }
    }

    public class OrderDetail
    {
        [Key]
        [DisplayName("編號")]
        public int Id { get; set; }

        [DisplayName("訂單編號")]
        public int OrderID { get; set; }

        [DisplayName("品名")]
        public string Name { get; set; }

        [DisplayName("數量")]
        public int Quantity { get; set; }

        [DisplayName("備註")]
        public string Note { get; set; }

        public OrderDetail()
        {
            // 空的無參數構造函數
        }

        public OrderDetail(int orderid, string name, int quantity, string note)
        {
            OrderID = orderid;
            Name = name;
            Quantity = quantity;
            Note = note;
        }
    }
}