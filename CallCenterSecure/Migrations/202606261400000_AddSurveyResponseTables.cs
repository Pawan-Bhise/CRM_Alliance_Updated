namespace CallCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSurveyResponseTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SurveyFormResponse",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyFormId = c.Int(nullable: false),
                    SurveyCustomerDataId = c.Int(),
                    RespondentName = c.String(maxLength: 100),
                    RespondentMobile = c.String(maxLength: 50),
                    SubmittedBy = c.String(nullable: false, maxLength: 100),
                    SubmittedDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyForm", t => t.SurveyFormId, cascadeDelete: true)
                .ForeignKey("dbo.SurveyCustomerData", t => t.SurveyCustomerDataId)
                .Index(t => t.SurveyFormId)
                .Index(t => t.SurveyCustomerDataId);

            CreateTable(
                "dbo.SurveyFormAnswer",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyFormResponseId = c.Int(nullable: false),
                    SurveyQuestionId = c.Int(nullable: false),
                    AnswerText = c.String(maxLength: 2000),
                    SelectedOption = c.String(maxLength: 500),
                    SelectedOptionsCsv = c.String(maxLength: 1000),
                    FileName = c.String(maxLength: 255),
                    FilePath = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyFormResponse", t => t.SurveyFormResponseId, cascadeDelete: true)
                .ForeignKey("dbo.SurveyQuestion", t => t.SurveyQuestionId)
                .Index(t => t.SurveyFormResponseId)
                .Index(t => t.SurveyQuestionId);

            CreateTable(
                "dbo.SurveyFormGridAnswer",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyFormAnswerId = c.Int(nullable: false),
                    RowText = c.String(nullable: false, maxLength: 500),
                    SelectedColumnText = c.String(maxLength: 500),
                    SelectedColumnTextsCsv = c.String(maxLength: 1000),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyFormAnswer", t => t.SurveyFormAnswerId, cascadeDelete: true)
                .Index(t => t.SurveyFormAnswerId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.SurveyFormGridAnswer", "SurveyFormAnswerId", "dbo.SurveyFormAnswer");
            DropForeignKey("dbo.SurveyFormAnswer", "SurveyQuestionId", "dbo.SurveyQuestion");
            DropForeignKey("dbo.SurveyFormAnswer", "SurveyFormResponseId", "dbo.SurveyFormResponse");
            DropForeignKey("dbo.SurveyFormResponse", "SurveyCustomerDataId", "dbo.SurveyCustomerData");
            DropForeignKey("dbo.SurveyFormResponse", "SurveyFormId", "dbo.SurveyForm");
            DropIndex("dbo.SurveyFormGridAnswer", new[] { "SurveyFormAnswerId" });
            DropIndex("dbo.SurveyFormAnswer", new[] { "SurveyQuestionId" });
            DropIndex("dbo.SurveyFormAnswer", new[] { "SurveyFormResponseId" });
            DropIndex("dbo.SurveyFormResponse", new[] { "SurveyCustomerDataId" });
            DropIndex("dbo.SurveyFormResponse", new[] { "SurveyFormId" });
            DropTable("dbo.SurveyFormGridAnswer");
            DropTable("dbo.SurveyFormAnswer");
            DropTable("dbo.SurveyFormResponse");
        }
    }
}
