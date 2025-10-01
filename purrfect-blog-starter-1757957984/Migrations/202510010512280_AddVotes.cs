namespace purrfect_blog_starter_1757957984.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        VoterUsername = c.String(nullable: false, maxLength: 100),
                        Value = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => new { t.PostId, t.VoterUsername }, unique: true, name: "IX_Post_Voter");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Votes", "IX_Post_Voter");
            DropTable("dbo.Votes");
        }
    }
}
