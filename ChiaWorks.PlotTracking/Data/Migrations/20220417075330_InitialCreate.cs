using ChiaWorks.PlotTracking.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChiaWorks.PlotTracking.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreatePlotInfoTable(migrationBuilder);
            CreateUserDeviceTable(migrationBuilder);
        }

        private void CreateUserDeviceTable(MigrationBuilder migrationBuilder)
        {
            var tableName = "UserDevices";

            migrationBuilder.CreateTable(
                name: tableName,
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(maxLength: 50,nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true,defaultValue:DateTime.Now),
                    IpAddress = table.Column<string>(maxLength: 50,nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey($"PK_{tableName}", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: $"IX_{tableName}_{nameof(UserDevice.UserId)}",
                table: tableName,
                column: nameof(UserDevice.UserId));

            migrationBuilder.CreateIndex(
                name: $"IX_{tableName}_{nameof(UserDevice.IpAddress)}",
                table: tableName,
                column: nameof(UserDevice.IpAddress));
        }

        private void CreatePlotInfoTable(MigrationBuilder migrationBuilder)
        {
            var tableName = "Plots";
            migrationBuilder.CreateTable(
                name: tableName,
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true,defaultValue:DateTime.Now),
                    IpAddress = table.Column<string>(maxLength: 50,nullable: true)
                },
                constraints: table => { table.PrimaryKey($"PK_{tableName}", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: $"IX_{tableName}_{nameof(PlotInfo.IpAddress)}",
                table: tableName,
                column: nameof(PlotInfo.IpAddress));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}