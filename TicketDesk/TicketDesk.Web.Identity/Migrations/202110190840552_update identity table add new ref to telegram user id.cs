namespace TicketDesk.Web.Identity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateidentitytableaddnewreftotelegramuserid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.IdentityUsers", "TelegramUserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.IdentityUsers", "TelegramUserId");
        }
    }
}
