namespace TicketDesk.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Td25022 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tickets", "TargetDate", c => c.DateTimeOffset());
            AddColumn("dbo.Tickets", "ResolutionDate", c => c.DateTimeOffset());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tickets", "ResolutionDate");
            DropColumn("dbo.Tickets", "TargetDate");
        }
    }
}
