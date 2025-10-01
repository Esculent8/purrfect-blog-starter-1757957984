namespace purrfect_blog_starter_1757957984.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedTestPosts : DbMigration
    {
        public override void Up()
        {
            Sql(@"
INSERT INTO dbo.Posts (Title, Content, Category, AuthorUsername, CreatedAt) VALUES
('Welcome to Purrfect Blog','Thank you for trying out this website :).','General','tester', GETUTCDATE()),
('Cats & Coffee','Second test post content.','Lifestyle','tester', DATEADD(MINUTE,-10,GETUTCDATE())),
('Pro Tips','Third test post content.','Tips','tester', DATEADD(MINUTE,-20,GETUTCDATE()))
");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM dbo.Posts WHERE AuthorUsername = 'tester' AND Title IN ('Welcome to Purrfect Blog','Cats & Coffee','Pro Tips')");
        }
    }
}
