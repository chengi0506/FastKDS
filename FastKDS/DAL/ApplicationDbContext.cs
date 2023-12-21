using FastKDS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace FastKDS.DAL
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            // 新增 InvoNo 欄位
            modelBuilder.Entity<Orders>()
                .Property(o => o.InvoNo)
                .HasMaxLength(20);
        }
    }

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // 可以在這裡加入種子資料
        }
    }

}