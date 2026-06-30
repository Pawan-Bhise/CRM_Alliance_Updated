namespace CallCenter.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddSurveyCategoryToSurveyFormResponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SurveyFormResponse", "SurveyCategoryId", c => c.Int());
        }

        public override void Down()
        {
            DropColumn("dbo.SurveyFormResponse", "SurveyCategoryId");
        }
    }
}