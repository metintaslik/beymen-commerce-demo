using Microsoft.EntityFrameworkCore.Migrations;

namespace Beymen.Demo.Infrastructure.Persistance.Data.Migrations;

public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                title = table.Column<string>(maxLength: 200, nullable: false),
                content = table.Column<string>(nullable: false),
                type = table.Column<string>(nullable: false),
                recipient = table.Column<string>(maxLength: 200, nullable: false),
                created_at = table.Column<DateTime>(nullable: false),
                status = table.Column<string>(nullable: false),
                updated_at = table.Column<DateTime>(nullable: true),
                is_deleted = table.Column<bool>(nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_notifications", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_notifications_status_created_at_updated_at_is_deleted",
            table: "notifications",
            columns: ["status", "created_at", "updated_at", "is_deleted"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "notifications");
    }
}
