namespace CallCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSurveyFormBuilderTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SurveyForm",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyTemplateId = c.Int(nullable: false),
                    Title = c.String(nullable: false, maxLength: 200),
                    Category = c.String(nullable: false, maxLength: 100),
                    Description = c.String(maxLength: 500),
                    CreatedBy = c.String(maxLength: 100),
                    CreatedDate = c.DateTime(nullable: false),
                    ModifiedBy = c.String(maxLength: 100),
                    ModifiedDate = c.DateTime(),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyTemplateType", t => t.SurveyTemplateId, cascadeDelete: false)
                .Index(t => t.SurveyTemplateId);

            CreateTable(
                "dbo.SurveyQuestion",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyFormId = c.Int(nullable: false),
                    QuestionText = c.String(nullable: false, maxLength: 1000),
                    QuestionType = c.String(nullable: false, maxLength: 50),
                    DisplayOrder = c.Int(nullable: false),
                    IsRequired = c.Boolean(nullable: false),
                    CreatedDate = c.DateTime(nullable: false),
                    MinValue = c.Int(),
                    MaxValue = c.Int(),
                    MinLabel = c.String(maxLength: 100),
                    MaxLabel = c.String(maxLength: 100),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyForm", t => t.SurveyFormId, cascadeDelete: true)
                .Index(t => t.SurveyFormId);

            CreateTable(
                "dbo.SurveyQuestionOption",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyQuestionId = c.Int(nullable: false),
                    OptionText = c.String(nullable: false, maxLength: 500),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyQuestion", t => t.SurveyQuestionId, cascadeDelete: true)
                .Index(t => t.SurveyQuestionId);

            CreateTable(
                "dbo.SurveyGridRow",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyQuestionId = c.Int(nullable: false),
                    RowText = c.String(nullable: false, maxLength: 500),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyQuestion", t => t.SurveyQuestionId, cascadeDelete: true)
                .Index(t => t.SurveyQuestionId);

            CreateTable(
                "dbo.SurveyGridColumn",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyQuestionId = c.Int(nullable: false),
                    ColumnText = c.String(nullable: false, maxLength: 500),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyQuestion", t => t.SurveyQuestionId, cascadeDelete: true)
                .Index(t => t.SurveyQuestionId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.SurveyGridColumn", "SurveyQuestionId", "dbo.SurveyQuestion");
            DropForeignKey("dbo.SurveyGridRow", "SurveyQuestionId", "dbo.SurveyQuestion");
            DropForeignKey("dbo.SurveyQuestionOption", "SurveyQuestionId", "dbo.SurveyQuestion");
            DropForeignKey("dbo.SurveyQuestion", "SurveyFormId", "dbo.SurveyForm");
            DropForeignKey("dbo.SurveyForm", "SurveyTemplateId", "dbo.SurveyTemplateType");
            DropIndex("dbo.SurveyGridColumn", new[] { "SurveyQuestionId" });
            DropIndex("dbo.SurveyGridRow", new[] { "SurveyQuestionId" });
            DropIndex("dbo.SurveyQuestionOption", new[] { "SurveyQuestionId" });
            DropIndex("dbo.SurveyQuestion", new[] { "SurveyFormId" });
            DropIndex("dbo.SurveyForm", new[] { "SurveyTemplateId" });
            DropTable("dbo.SurveyGridColumn");
            DropTable("dbo.SurveyGridRow");
            DropTable("dbo.SurveyQuestionOption");
            DropTable("dbo.SurveyQuestion");
            DropTable("dbo.SurveyForm");
        }
    }
}
