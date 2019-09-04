namespace Prj_LineBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdAndNameInUploadImageTb : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UploadImages", "userId", c => c.String());
            AddColumn("dbo.UploadImages", "userName", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UploadImages", "userName");
            DropColumn("dbo.UploadImages", "userId");
        }
    }
}
