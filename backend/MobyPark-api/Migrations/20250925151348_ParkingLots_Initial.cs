using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobyPark_api.Migrations
{
    /// <inheritdoc />
    public partial class ParkingLots_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingLots",
                table: "ParkingLots");

            migrationBuilder.RenameTable(
                name: "ParkingLots",
                newName: "parking_lots");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "parking_lots",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "parking_lots",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "parking_lots",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "parking_lots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "DayTariff",
                table: "parking_lots",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tariff",
                table: "parking_lots",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "coordinates_lat",
                table: "parking_lots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "coordinates_lng",
                table: "parking_lots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_parking_lots",
                table: "parking_lots",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_parking_lots",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "DayTariff",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "Tariff",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "coordinates_lat",
                table: "parking_lots");

            migrationBuilder.DropColumn(
                name: "coordinates_lng",
                table: "parking_lots");

            migrationBuilder.RenameTable(
                name: "parking_lots",
                newName: "ParkingLots");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "ParkingLots",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ParkingLots",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingLots",
                table: "ParkingLots",
                column: "Id");
        }
    }
}
