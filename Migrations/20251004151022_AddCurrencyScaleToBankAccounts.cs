using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechStatsPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyScaleToBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "users",
                newName: "account_ids");

            migrationBuilder.AlterColumn<long>(
                name: "balance",
                table: "bank_accounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<short>(
                name: "currency_scale",
                table: "bank_accounts",
                type: "smallint",
                nullable: false,
                defaultValue: (short)2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency_scale",
                table: "bank_accounts");

            migrationBuilder.RenameColumn(
                name: "account_ids",
                table: "users",
                newName: "account_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "balance",
                table: "bank_accounts",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);
        }
    }
}
