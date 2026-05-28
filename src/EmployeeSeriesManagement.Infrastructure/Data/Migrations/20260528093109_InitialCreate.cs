using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeSeriesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecondName = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthPlace = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfileImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    OrganizationalUnit = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Code = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AddressTypeId = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MailboxNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Building = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_AddressType_AddressTypeId",
                        column: x => x.AddressTypeId,
                        principalTable: "AddressType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeesIdCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EmployeesExternalId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Validity = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeesIdCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeesIdCards_Employees_EmployeesExternalId",
                        column: x => x.EmployeesExternalId,
                        principalTable: "Employees",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeesSeries",
                columns: table => new
                {
                    EmployeesExternalId = table.Column<int>(type: "int", nullable: false),
                    SeriesCode = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeesSeries", x => new { x.EmployeesExternalId, x.SeriesCode, x.StartDate });
                    table.ForeignKey(
                        name: "FK_EmployeesSeries_Employees_EmployeesExternalId",
                        column: x => x.EmployeesExternalId,
                        principalTable: "Employees",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeesSeries_Series_SeriesCode",
                        column: x => x.SeriesCode,
                        principalTable: "Series",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeesAddresses",
                columns: table => new
                {
                    EmployeesExternalId = table.Column<int>(type: "int", nullable: false),
                    AddressesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeesAddresses", x => new { x.EmployeesExternalId, x.AddressesId });
                    table.ForeignKey(
                        name: "FK_EmployeesAddresses_Addresses_AddressesId",
                        column: x => x.AddressesId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeesAddresses_Employees_EmployeesExternalId",
                        column: x => x.EmployeesExternalId,
                        principalTable: "Employees",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressTypeId",
                table: "Addresses",
                column: "AddressTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesAddresses_AddressesId",
                table: "EmployeesAddresses",
                column: "AddressesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesIdCards_EmployeesExternalId",
                table: "EmployeesIdCards",
                column: "EmployeesExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeesSeries_SeriesCode",
                table: "EmployeesSeries",
                column: "SeriesCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeesAddresses");

            migrationBuilder.DropTable(
                name: "EmployeesIdCards");

            migrationBuilder.DropTable(
                name: "EmployeesSeries");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "AddressType");
        }
    }
}
