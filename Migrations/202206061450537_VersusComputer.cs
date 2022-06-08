namespace Connect4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VersusComputer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "VersusComputer", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Matches", "VersusComputer");
        }
    }
}
