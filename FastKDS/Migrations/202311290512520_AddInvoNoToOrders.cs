namespace FastKDS.DAL
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvoNoToOrders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(nullable: false),
                        Name = c.String(),
                        Quantity = c.Int(nullable: false),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderID, cascadeDelete: true)
                .Index(t => t.OrderID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        Note = c.String(maxLength: 50),
                        State = c.String(maxLength: 10),
                        CookTime = c.DateTime(),
                        MakeTime = c.DateTime(),
                        TakeTime = c.DateTime(),
                        InvoNo = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.OrderID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders");
            DropIndex("dbo.OrderDetails", new[] { "OrderID" });
            DropTable("dbo.Orders");
            DropTable("dbo.OrderDetails");
        }
    }
}
