using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sabanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTargetEntityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "target_entity_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "target_entity_id",
                table: "audit_logs");
        }
    }
}
