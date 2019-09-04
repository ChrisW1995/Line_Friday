namespace Prj_LineBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MovieQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        M_Question = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuestionProcesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        M_Order = c.Int(nullable: false),
                        QuestionStatus = c.Boolean(nullable: false),
                        Answer = c.String(),
                        ErrorCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.QuestionProcesses");
            DropTable("dbo.MovieQuestions");
        }
    }
}
