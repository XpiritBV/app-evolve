namespace XamarinEvolve.Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFloorLevelAndCoordinatesToRoom : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "FloorLevel", c => c.Int(nullable:true));
            AddColumn("dbo.Rooms", "XCoordinate", c => c.Int(nullable: true));
            AddColumn("dbo.Rooms", "YCoordinate", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Rooms", "YCoordinate");
            DropColumn("dbo.Rooms", "XCoordinate");
            DropColumn("dbo.Rooms", "FloorLevel");
        }
    }
}
