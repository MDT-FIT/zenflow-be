using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechStatsPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountIdsInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "users",
                newName: "account_ids");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_ids",
                table: "users",
                newName: "account_id");
        }
    }
}
