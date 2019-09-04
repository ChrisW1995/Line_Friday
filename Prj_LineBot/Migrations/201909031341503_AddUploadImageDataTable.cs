namespace Prj_LineBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUploadImageDataTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UploadImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        imageUrl = c.String(),
                        flg = c.Boolean(nullable: false),
                        addTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UploadImages");
        }
    }
}
