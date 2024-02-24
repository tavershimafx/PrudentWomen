using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class DropAppSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prudent.ApplicationSettings");

            migrationBuilder.AddColumn<string>(
                name: "DisbursementAccount",
                table: "Prudent.Loans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tenure",
                table: "Prudent.Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "UserAccountId",
                table: "Prudent.Loans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.Loans_UserAccountId",
                table: "Prudent.Loans",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prudent.Loans_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.Loans",
                column: "UserAccountId",
                principalTable: "Prudent.UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prudent.Loans_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.Loans");

            migrationBuilder.DropIndex(
                name: "IX_Prudent.Loans_UserAccountId",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "DisbursementAccount",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "Tenure",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Prudent.Loans");

            migrationBuilder.CreateTable(
                name: "Prudent.ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.ApplicationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.ApplicationSettings_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.ApplicationSettings_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.ApplicationSettings_CreatedById",
                table: "Prudent.ApplicationSettings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.ApplicationSettings_UpdatedById",
                table: "Prudent.ApplicationSettings",
                column: "UpdatedById");
        }
    }
}
