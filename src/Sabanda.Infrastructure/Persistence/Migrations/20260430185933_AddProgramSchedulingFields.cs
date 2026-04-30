using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sabanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "age_group",
                table: "programs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "day",
                table: "programs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "frequency",
                table: "programs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "time",
                table: "programs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "venue",
                table: "programs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "age_group",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "day",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "frequency",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "time",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "venue",
                table: "programs");
        }
    }
}
