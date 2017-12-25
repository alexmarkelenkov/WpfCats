namespace WpfCats.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPlateMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlateNumbers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        Count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PlateNumbers");
        }
    }
}
