using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobyPark_api.Migrations
{
    /// <inheritdoc />
    public partial class discounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "sessions",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DiscountId",
                table: "ParkingLots",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<float>(type: "real", nullable: false),
                    @operator = table.Column<int>(name: "operator", type: "int", nullable: false),
                    start = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    end = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ParkingLotIds = table.Column<long[]>(type: "bigint[]", nullable: true),
                    discounttype = table.Column<int>(name: "discount-type", type: "int", nullable: false),
                    typespecificdata = table.Column<string>(name: "type-specific-data", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discount-parking-lot",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discountid = table.Column<long>(name: "discount-id", type: "bigint", nullable: false),
                    parkinglotid = table.Column<long>(name: "parking-lot-id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount-parking-lot", x => x.id);
                    table.ForeignKey(
                        name: "FK_discount-parking-lot_Discounts_discount-id",
                        column: x => x.discountid,
                        principalTable: "Discounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_discount-parking-lot_ParkingLots_parking-lot-id",
                        column: x => x.parkinglotid,
                        principalTable: "ParkingLots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingLots_DiscountId",
                table: "ParkingLots",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_discount-parking-lot_discount-id",
                table: "discount-parking-lot",
                column: "discount-id");

            migrationBuilder.CreateIndex(
                name: "IX_discount-parking-lot_parking-lot-id",
                table: "discount-parking-lot",
                column: "parking-lot-id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingLots_Discounts_DiscountId",
                table: "ParkingLots",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingLots_Discounts_DiscountId",
                table: "ParkingLots");

            migrationBuilder.DropTable(
                name: "discount-parking-lot");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_ParkingLots_DiscountId",
                table: "ParkingLots");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "ParkingLots");

            migrationBuilder.AlterColumn<float>(
                name: "Cost",
                table: "sessions",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
