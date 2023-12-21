namespace FastKDS.DAL
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrderIDType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders");
            DropIndex("dbo.OrderDetails", new[] { "OrderID" });
            DropPrimaryKey("dbo.Orders");
            AlterColumn("dbo.OrderDetails", "OrderID", c => c.Long(nullable: false));
            AlterColumn("dbo.Orders", "OrderID", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.Orders", "OrderID");
            CreateIndex("dbo.OrderDetails", "OrderID");
            AddForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders", "OrderID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders");
            DropIndex("dbo.OrderDetails", new[] { "OrderID" });
            DropPrimaryKey("dbo.Orders");
            AlterColumn("dbo.Orders", "OrderID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.OrderDetails", "OrderID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Orders", "OrderID");
            CreateIndex("dbo.OrderDetails", "OrderID");
            AddForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders", "OrderID", cascadeDelete: true);
        }
    }
}
