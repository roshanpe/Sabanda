using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sabanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToFamilyAndMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "members",
                type: "text",
                nullable: false,
                defaultValue: "000000");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "families",
                type: "text",
                nullable: false,
                defaultValue: "000000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "members");

            migrationBuilder.DropColumn(
                name: "code",
                table: "families");
        }
    }
}
