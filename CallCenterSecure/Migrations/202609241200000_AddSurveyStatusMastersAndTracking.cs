namespace CallCenter.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddSurveyStatusMastersAndTracking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SurveyCallStatusMaster",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.SurveyFormStatusMaster",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.SurveyCustomerFormTracking",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SurveyCustomerDataId = c.Int(nullable: false),
                    SurveyTemplateTypeId = c.Int(nullable: false),
                    SurveyFormId = c.Int(nullable: false),
                    CallStatusId = c.Int(),
                    FormStatusId = c.Int(),
                    CallRemarks = c.String(maxLength: 1000),
                    ModifiedBy = c.String(maxLength: 100),
                    ModifiedDate = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyCustomerData", t => t.SurveyCustomerDataId)
                .ForeignKey("dbo.SurveyTemplateType", t => t.SurveyTemplateTypeId)
                .ForeignKey("dbo.SurveyForm", t => t.SurveyFormId)
                .ForeignKey("dbo.SurveyCallStatusMaster", t => t.CallStatusId)
                .ForeignKey("dbo.SurveyFormStatusMaster", t => t.FormStatusId)
                .Index(t => new { t.SurveyCustomerDataId, t.SurveyTemplateTypeId, t.SurveyFormId }, unique: true)
                .Index(t => t.CallStatusId)
                .Index(t => t.FormStatusId);

            Sql("INSERT INTO dbo.SurveyCallStatusMaster (Name, IsActive) VALUES ('Connected', 1), ('No Answer', 1), ('Busy', 1), ('Wrong Number', 1), ('Out of Service', 1), ('Declined', 1), ('Call Back Late', 1)");
            Sql("INSERT INTO dbo.SurveyFormStatusMaster (Name, IsActive) VALUES ('Not Started', 1), ('Partially Completed', 1), ('Completed', 1)");
        }

        public override void Down()
        {
            DropForeignKey("dbo.SurveyCustomerFormTracking", "FormStatusId", "dbo.SurveyFormStatusMaster");
            DropForeignKey("dbo.SurveyCustomerFormTracking", "CallStatusId", "dbo.SurveyCallStatusMaster");
            DropForeignKey("dbo.SurveyCustomerFormTracking", "SurveyFormId", "dbo.SurveyForm");
            DropForeignKey("dbo.SurveyCustomerFormTracking", "SurveyTemplateTypeId", "dbo.SurveyTemplateType");
            DropForeignKey("dbo.SurveyCustomerFormTracking", "SurveyCustomerDataId", "dbo.SurveyCustomerData");
            DropIndex("dbo.SurveyCustomerFormTracking", new[] { "FormStatusId" });
            DropIndex("dbo.SurveyCustomerFormTracking", new[] { "CallStatusId" });
            DropIndex("dbo.SurveyCustomerFormTracking", new[] { "SurveyCustomerDataId", "SurveyTemplateTypeId", "SurveyFormId" });
            DropTable("dbo.SurveyCustomerFormTracking");
            DropTable("dbo.SurveyFormStatusMaster");
            DropTable("dbo.SurveyCallStatusMaster");
        }
    }
}