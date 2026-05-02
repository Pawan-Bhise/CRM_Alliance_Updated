namespace CallCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyCustomerDataTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SurveyCustomerData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientName = c.String(nullable: false, maxLength: 200),
                        Gender = c.String(maxLength: 20),
                        CustomerCode = c.String(nullable: false, maxLength: 50),
                        MobileNumber1 = c.String(maxLength: 30),
                        MobileNumber2 = c.String(maxLength: 30),
                        Region = c.String(maxLength: 100),
                        Branch = c.String(maxLength: 100),
                        Location = c.String(maxLength: 100),
                        LoanProduct = c.String(maxLength: 100),
                        Age = c.Int(),
                        BusinessCategory = c.String(maxLength: 100),
                        ActivitiesSector = c.String(maxLength: 200),
                        LevelOfEducation = c.String(maxLength: 50),
                        IncomeLevel = c.String(maxLength: 100),
                        HouseholdAssets = c.String(maxLength: 200),
                        PovertyScore = c.Int(),
                        SurveyTemplateTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SurveyTemplateType", t => t.SurveyTemplateTypeId)
                .Index(t => t.SurveyTemplateTypeId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SurveyCustomerData", "SurveyTemplateTypeId", "dbo.SurveyTemplateType");
            DropIndex("dbo.SurveyCustomerData", new[] { "SurveyTemplateTypeId" });
            DropTable("dbo.SurveyCustomerData");
        }
    }
}