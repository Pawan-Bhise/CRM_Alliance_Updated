using System;
using System.Data.Entity.Migrations;

namespace CallCenterSecure.Migrations
{
    public partial class AddUploadJobTrackingFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UploadJobs", "UploadedBy", c => c.String());
            AddColumn("dbo.UploadJobs", "BatchTag", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.UploadJobs", "BatchTag");
            DropColumn("dbo.UploadJobs", "UploadedBy");
        }
    }
}
