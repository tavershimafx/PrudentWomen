using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class UserAccountLoans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prudent.ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Prudent.BankTransactions",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsIdentified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.BankTransactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Prudent.Loans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmountRequested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountGranted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PecentInterest = table.Column<int>(type: "int", nullable: false),
                    DateApplied = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ApproverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BalanceAtApproval = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateApproved = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Repaid = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDisbursed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.Loans_Mono.Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.Loans_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.Loans_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prudent.UserAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastTransaction = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.UserAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.UserAccounts_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.UserAccounts_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.UserAccounts_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prudent.UserTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.UserTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.UserTransactions_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.UserTransactions_Mono.Users_UpdatedById",
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

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.Loans_ApproverId",
                table: "Prudent.Loans",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.Loans_CreatedById",
                table: "Prudent.Loans",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.Loans_UpdatedById",
                table: "Prudent.Loans",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserAccounts_CreatedById",
                table: "Prudent.UserAccounts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserAccounts_UpdatedById",
                table: "Prudent.UserAccounts",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserAccounts_UserId",
                table: "Prudent.UserAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserTransactions_CreatedById",
                table: "Prudent.UserTransactions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserTransactions_UpdatedById",
                table: "Prudent.UserTransactions",
                column: "UpdatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prudent.ApplicationSettings");

            migrationBuilder.DropTable(
                name: "Prudent.BankTransactions");

            migrationBuilder.DropTable(
                name: "Prudent.Loans");

            migrationBuilder.DropTable(
                name: "Prudent.UserAccounts");

            migrationBuilder.DropTable(
                name: "Prudent.UserTransactions");
        }
    }
}
