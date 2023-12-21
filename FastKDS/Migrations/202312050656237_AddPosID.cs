namespace FastKDS.DAL
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPosID : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "PosID", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "PosID", c => c.Long(nullable: false));
        }
    }
}
