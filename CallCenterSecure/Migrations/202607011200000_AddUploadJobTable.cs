namespace CallCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUploadJobTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UploadJobs",
                c => new
                    {
                        UploadJobId = c.Int(nullable: false, identity: true),
                        FileName = c.String(nullable: false),
                        FilePath = c.String(nullable: false),
                        Status = c.String(nullable: false),
                        Message = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        StartedOn = c.DateTime(),
                        CompletedOn = c.DateTime(),
                        ProcessedRows = c.Int(),
                    })
                .PrimaryKey(t => t.UploadJobId);
        }
        
        public override void Down()
        {
            DropTable("dbo.UploadJobs");
        }
    }
}
