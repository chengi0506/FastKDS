namespace FastKDS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FastKDS : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Quantity = c.Int(nullable: false),
                        Note = c.String(),
                        Orders_OrderID = c.Int(),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Orders", t => t.Orders_OrderID)
                .Index(t => t.Orders_OrderID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        Note = c.String(maxLength: 50),
                        State = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.OrderID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetails", "Orders_OrderID", "dbo.Orders");
            DropIndex("dbo.OrderDetails", new[] { "Orders_OrderID" });
            DropTable("dbo.Orders");
            DropTable("dbo.OrderDetails");
        }
    }
}
