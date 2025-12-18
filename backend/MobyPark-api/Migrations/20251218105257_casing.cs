using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobyPark_api.Migrations
{
    /// <inheritdoc />
    public partial class casing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Reservations",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "Reservations",
                newName: "cost");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Reservations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Reservations",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "ParkingLotId",
                table: "Reservations",
                newName: "parking_lot_id");

            migrationBuilder.RenameColumn(
                name: "LicensePlate",
                table: "Reservations",
                newName: "license_plate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Reservations",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reservations",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ParkingLotId",
                table: "Reservations",
                newName: "IX_Reservations_parking_lot_id");

            migrationBuilder.RenameColumn(
                name: "Tariff",
                table: "ParkingLots",
                newName: "tariff");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ParkingLots",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "ParkingLots",
                newName: "location");

            migrationBuilder.RenameColumn(
                name: "Coordinates",
                table: "ParkingLots",
                newName: "coordinates");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "ParkingLots",
                newName: "capacity");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "ParkingLots",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ParkingLots",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DayTariff",
                table: "ParkingLots",
                newName: "day_tariff");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ParkingLots",
                newName: "created_at");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_time",
                table: "Reservations",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_time",
                table: "Reservations",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Reservations",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "coordinates",
                table: "ParkingLots",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "ParkingLots",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ParkingLots",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingLots_parking_lot_id",
                table: "Reservations",
                column: "parking_lot_id",
                principalTable: "ParkingLots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingLots_parking_lot_id",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Reservations",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "cost",
                table: "Reservations",
                newName: "Cost");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Reservations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "Reservations",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "parking_lot_id",
                table: "Reservations",
                newName: "ParkingLotId");

            migrationBuilder.RenameColumn(
                name: "license_plate",
                table: "Reservations",
                newName: "LicensePlate");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "Reservations",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Reservations",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_parking_lot_id",
                table: "Reservations",
                newName: "IX_Reservations_ParkingLotId");

            migrationBuilder.RenameColumn(
                name: "tariff",
                table: "ParkingLots",
                newName: "Tariff");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "ParkingLots",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "location",
                table: "ParkingLots",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "coordinates",
                table: "ParkingLots",
                newName: "Coordinates");

            migrationBuilder.RenameColumn(
                name: "capacity",
                table: "ParkingLots",
                newName: "Capacity");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "ParkingLots",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ParkingLots",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "day_tariff",
                table: "ParkingLots",
                newName: "DayTariff");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ParkingLots",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<string>(
                name: "Coordinates",
                table: "ParkingLots",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "ParkingLots",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ParkingLots",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotId",
                table: "Reservations",
                column: "ParkingLotId",
                principalTable: "ParkingLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
