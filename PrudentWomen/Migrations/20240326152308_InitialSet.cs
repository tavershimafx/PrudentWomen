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
                    DOB = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Prudent.BankTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    _Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsIdentified = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.BankTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.BankTransactions_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.BankTransactions_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
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
                name: "Prudent.SyncLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NumberOfRecords = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.SyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.SyncLogs_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.SyncLogs_Mono.Users_UpdatedById",
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

            migrationBuilder.CreateTable(
                name: "Prudent.Loans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmountRequested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountGranted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PecentInterest = table.Column<int>(type: "int", nullable: false),
                    Tenure = table.Column<int>(type: "int", nullable: false),
                    DateApplied = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApproverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BalanceAtApproval = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateApproved = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Repaid = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDisbursed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DisbursementAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankNIPCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guarantors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportingDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Prudent.Loans_Prudent.UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "Prudent.UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prudent.UserTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserAccountId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Prudent.UserTransactions_Prudent.UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "Prudent.UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Prudent.LoanGuarantors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LoanId = table.Column<long>(type: "bigint", nullable: false),
                    AmountToVouch = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountRequested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prudent.LoanGuarantors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prudent.LoanGuarantors_Mono.Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.LoanGuarantors_Mono.Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.LoanGuarantors_Mono.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Mono.Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prudent.LoanGuarantors_Prudent.Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Prudent.Loans",
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
                name: "IX_Prudent.ApplicationSettings_CreatedById",
                table: "Prudent.ApplicationSettings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.ApplicationSettings_UpdatedById",
                table: "Prudent.ApplicationSettings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.BankTransactions_CreatedById",
                table: "Prudent.BankTransactions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.BankTransactions_UpdatedById",
                table: "Prudent.BankTransactions",
                column: "UpdatedById");

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

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanGuarantors_CreatedById",
                table: "Prudent.LoanGuarantors",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanGuarantors_LoanId",
                table: "Prudent.LoanGuarantors",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanGuarantors_UpdatedById",
                table: "Prudent.LoanGuarantors",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.LoanGuarantors_UserId",
                table: "Prudent.LoanGuarantors",
                column: "UserId");

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
                name: "IX_Prudent.Loans_UserAccountId",
                table: "Prudent.Loans",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.RegCodes_CreatedById",
                table: "Prudent.RegCodes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.RegCodes_UpdatedById",
                table: "Prudent.RegCodes",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.SyncLogs_CreatedById",
                table: "Prudent.SyncLogs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.SyncLogs_UpdatedById",
                table: "Prudent.SyncLogs",
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

            migrationBuilder.CreateIndex(
                name: "IX_Prudent.UserTransactions_UserAccountId",
                table: "Prudent.UserTransactions",
                column: "UserAccountId");
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
                name: "Prudent.ApplicationSettings");

            migrationBuilder.DropTable(
                name: "Prudent.BankTransactions");

            migrationBuilder.DropTable(
                name: "Prudent.LoanDisbursements");

            migrationBuilder.DropTable(
                name: "Prudent.LoanGuarantors");

            migrationBuilder.DropTable(
                name: "Prudent.RegCodes");

            migrationBuilder.DropTable(
                name: "Prudent.SyncLogs");

            migrationBuilder.DropTable(
                name: "Prudent.UserTransactions");

            migrationBuilder.DropTable(
                name: "Mono.Roles");

            migrationBuilder.DropTable(
                name: "Prudent.Loans");

            migrationBuilder.DropTable(
                name: "Prudent.UserAccounts");

            migrationBuilder.DropTable(
                name: "Mono.Users");
        }
    }
}
