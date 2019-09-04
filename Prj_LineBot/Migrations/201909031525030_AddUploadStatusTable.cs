namespace Prj_LineBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUploadStatusTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UploadStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommandStr = c.String(maxLength: 25),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UploadStatus");
        }
    }
}
