using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechStatsPlatform.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bank_accounts_banks_bank_id",
                table: "bank_accounts");

            migrationBuilder.DropIndex(
                name: "IX_bank_accounts_bank_id",
                table: "bank_accounts");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "bank_accounts",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "bank_id",
                table: "bank_accounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "bank_accounts",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bank_id",
                table: "bank_accounts",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_bank_id",
                table: "bank_accounts",
                column: "bank_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bank_accounts_banks_bank_id",
                table: "bank_accounts",
                column: "bank_id",
                principalTable: "banks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
