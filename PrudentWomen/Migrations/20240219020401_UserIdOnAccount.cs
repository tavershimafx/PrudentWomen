using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class UserIdOnAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccountId",
                table: "Prudent.UserTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserAccountId",
                table: "Prudent.UserTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserTransactions_UserAccountId",
                table: "Prudent.UserTransactions",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions",
                column: "UserAccountId",
                principalTable: "Prudent.UserAccounts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Prudent.UserTransactions_UserAccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Prudent.UserTransactions");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Prudent.UserTransactions");
        }
    }
}
