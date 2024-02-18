using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrudentWomen.Migrations
{
    public partial class InitialSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mono.Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mono.Users_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mono.Users_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mono.Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mono.Roles_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mono.Roles_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mono.UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mono.UserClaims_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mono.UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_Mono.UserLogins_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mono.UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_Mono.UserTokens_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prudent.RegCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.RegCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.RegCodes_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.RegCodes_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mono.RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mono.RoleClaims_Mono.Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Mono.Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mono.UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mono.UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Mono.UserRoles_Mono.Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Mono.Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mono.UserRoles_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mono.RoleClaims_RoleId",
                table: "Mono.RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.Roles_CreatedById",
                table: "Mono.Roles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.Roles_UpdatedById",
                table: "Mono.Roles",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Mono.Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.UserClaims_UserId",
                table: "Mono.UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.UserLogins_UserId",
                table: "Mono.UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.UserRoles_RoleId",
                table: "Mono.UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Mono.Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.Users_CreatedById",
                table: "Mono.Users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Mono.Users_UpdatedById",
                table: "Mono.Users",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Mono.Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.RegCodes_CreatedById",
                table: "Prudent.RegCodes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.RegCodes_UpdatedById",
                table: "Prudent.RegCodes",
                column: "UpdatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mono.RoleClaims");

            migrationBuilder.DropTable(
                name: "Mono.UserClaims");

            migrationBuilder.DropTable(
                name: "Mono.UserLogins");

            migrationBuilder.DropTable(
                name: "Mono.UserRoles");

            migrationBuilder.DropTable(
                name: "Mono.UserTokens");

            migrationBuilder.DropTable(
                name: "Prudent.RegCodes");

            migrationBuilder.DropTable(
                name: "Mono.Roles");

            migrationBuilder.DropTable(
                name: "Mono.Users");
        }
    }
}
