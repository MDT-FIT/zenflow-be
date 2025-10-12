using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechStatsPlatform.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailPasswordUsernameFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "email", table: "users");

            migrationBuilder.DropColumn(name: "password", table: "users");

            migrationBuilder.DropColumn(name: "username", table: "users");

            migrationBuilder.AlterColumn<List<string>>(
                name: "account_id",
                table: "users",
                type: "varchar(100)[]",
                nullable: false,
                defaultValueSql: "'{}'::varchar[]",
                oldClrType: typeof(string[]),
                oldType: "varchar(100)[]",
                oldDefaultValue: Array.Empty<string>()
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "account_id",
                table: "users",
                type: "varchar(100)[]",
                nullable: false,
                defaultValue: Array.Empty<string>(),
                oldClrType: typeof(List<string>),
                oldType: "varchar(100)[]",
                oldDefaultValueSql: "'{}'::varchar[]"
            );

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "users",
                type: "varchar(200)",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
