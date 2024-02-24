using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class AppSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prudent.ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.ApplicationSettings_CreatedById",
                table: "Prudent.ApplicationSettings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.ApplicationSettings_UpdatedById",
                table: "Prudent.ApplicationSettings",
                column: "UpdatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prudent.ApplicationSettings");
        }
    }
}
