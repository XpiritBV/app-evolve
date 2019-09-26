namespace XamarinEvolve.Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SessionAugmentation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sessions", "SpeakerIdString", c => c.String(nullable: true));
            AddColumn("dbo.Sessions", "RoomIdString", c => c.String(nullable: true));
            AddColumn("dbo.Sessions", "AudioStreamWebUrl", c => c.String(nullable: true));
            AddColumn("dbo.Sessions", "AudioStreamAppUrl", c => c.String(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sessions", "AudioStreamAppUrl");
            DropColumn("dbo.Sessions", "AudioStreamWebUrl");
            DropColumn("dbo.Sessions", "RoomIdString");
            DropColumn("dbo.Sessions", "SpeakerIdString");
        }
    }
}
