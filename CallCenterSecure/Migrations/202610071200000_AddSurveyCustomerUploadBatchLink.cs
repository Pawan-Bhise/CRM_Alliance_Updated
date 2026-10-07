namespace CallCenter.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddSurveyCustomerUploadBatchLink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UploadJobs", "SurveyTemplateTypeId", c => c.Int());
            AddColumn("dbo.SurveyCustomerData", "UploadJobId", c => c.Int());
            CreateIndex("dbo.UploadJobs", "SurveyTemplateTypeId");
            CreateIndex("dbo.SurveyCustomerData", "UploadJobId");
            AddForeignKey("dbo.UploadJobs", "SurveyTemplateTypeId", "dbo.SurveyTemplateType", "Id");
            AddForeignKey("dbo.SurveyCustomerData", "UploadJobId", "dbo.UploadJobs", "UploadJobId");
        }

        public override void Down()
        {
            DropForeignKey("dbo.SurveyCustomerData", "UploadJobId", "dbo.UploadJobs");
            DropForeignKey("dbo.UploadJobs", "SurveyTemplateTypeId", "dbo.SurveyTemplateType");
            DropIndex("dbo.SurveyCustomerData", new[] { "UploadJobId" });
            DropIndex("dbo.UploadJobs", new[] { "SurveyTemplateTypeId" });
            DropColumn("dbo.SurveyCustomerData", "UploadJobId");
            DropColumn("dbo.UploadJobs", "SurveyTemplateTypeId");
        }
    }
}