namespace Connect4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cambiato_Proprità_Move_Aggiunta_NextTurnPLayer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Moves", "NextTurnPlayer", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Moves", "NextTurnPlayer");
        }
    }
}
