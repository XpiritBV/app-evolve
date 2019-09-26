namespace XamarinEvolve.Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFloorLevelAndCoordinatesToSponsor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sponsors", "FloorLevel", c => c.Int(nullable:true));
            AddColumn("dbo.Sponsors", "XCoordinate", c => c.Int(nullable: true));
            AddColumn("dbo.Sponsors", "YCoordinate", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sponsors", "YCoordinate");
            DropColumn("dbo.Sponsors", "XCoordinate");
            DropColumn("dbo.Sponsors", "FloorLevel");
        }
    }
}
