using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class RefineDisbursement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.AlterColumn<long>(
                name: "UserAccountId",
                table: "Prudent.UserTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankNIPCode",
                table: "Prudent.Loans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guarantors",
                table: "Prudent.Loans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingDocuments",
                table: "Prudent.Loans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DOB",
                table: "Mono.Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "Prudent.LoanDisbursements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentGatewayReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisbursementAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDisbursed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.LoanDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.LoanDisbursements_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.LoanDisbursements_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.LoanDisbursements_Prudent.Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Prudent.Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanDisbursements_CreatedById",
                table: "Prudent.LoanDisbursements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanDisbursements_LoanId",
                table: "Prudent.LoanDisbursements",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanDisbursements_UpdatedById",
                table: "Prudent.LoanDisbursements",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions",
                column: "UserAccountId",
                principalTable: "Prudent.UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.DropTable(
                name: "Prudent.LoanDisbursements");

            migrationBuilder.DropColumn(
                name: "BankNIPCode",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "Guarantors",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "SupportingDocuments",
                table: "Prudent.Loans");

            migrationBuilder.DropColumn(
                name: "DOB",
                table: "Mono.Users");

            migrationBuilder.AlterColumn<long>(
                name: "UserAccountId",
                table: "Prudent.UserTransactions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "AccountId",
                table: "Prudent.UserTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions",
                column: "UserAccountId",
                principalTable: "Prudent.UserAccounts",
                principalColumn: "Id");
        }
    }
}
