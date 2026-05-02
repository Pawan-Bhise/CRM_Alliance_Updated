namespace CallCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyTemplateTypeTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SurveyTemplateType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            // Seed initial data
            Sql("INSERT INTO SurveyTemplateType (Name) VALUES ('Exit Client Survey')");
            Sql("INSERT INTO SurveyTemplateType (Name) VALUES ('Satisfaction Survey')");
            Sql("INSERT INTO SurveyTemplateType (Name) VALUES ('Competitor Survey')");
        }
        
        public override void Down()
        {
            DropTable("dbo.SurveyTemplateType");
        }
    }
}