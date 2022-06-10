namespace Connect4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cambiato_Proprità_Move : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Moves", "Player", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Moves", "Player", c => c.Int(nullable: false));
        }
    }
}
