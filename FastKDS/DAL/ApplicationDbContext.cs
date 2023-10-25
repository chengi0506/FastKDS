using FastKDS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FastKDS.DAL
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }

}