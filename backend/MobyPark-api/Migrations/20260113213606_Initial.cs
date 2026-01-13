using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobyPark_api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                name: "licenseplates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    license_plate_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_licenseplates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_data",
                columns: table => new
                {
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    issuer = table.Column<string>(type: "text", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_data", x => x.transaction_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTimeOffset>(name: "created-at", type: "timestamptz", nullable: false),
                    birthyear = table.Column<short>(name: "birth-year", type: "smallint", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parking_lots",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    capacity = table.Column<long>(type: "bigint", nullable: false),
                    tariff = table.Column<decimal>(type: "numeric", nullable: true),
                    day_tariff = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    coordinates = table.Column<string>(type: "text", nullable: true),
                    DiscountId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_lots", x => x.id);
                    table.ForeignKey(
                        name: "FK_parking_lots_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParkingLotId = table.Column<long>(type: "bigint", nullable: false),
                    LicensePlateId = table.Column<long>(type: "bigint", nullable: true),
                    Started = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stopped = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<short>(type: "smallint", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric", nullable: true),
                    PlateText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_licenseplates_LicensePlateId",
                        column: x => x.LicensePlateId,
                        principalTable: "licenseplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_transaction_data_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transaction_data",
                        principalColumn: "transaction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    license_plate = table.Column<string>(type: "text", nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_discount-parking-lot_parking_lots_parking-lot-id",
                        column: x => x.parkinglotid,
                        principalTable: "parking_lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    parking_lot_id = table.Column<long>(type: "bigint", nullable: false),
                    license_plate = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservations_parking_lots_parking_lot_id",
                        column: x => x.parking_lot_id,
                        principalTable: "parking_lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discount-parking-lot_discount-id",
                table: "discount-parking-lot",
                column: "discount-id");

            migrationBuilder.CreateIndex(
                name: "IX_discount-parking-lot_parking-lot-id",
                table: "discount-parking-lot",
                column: "parking-lot-id");

            migrationBuilder.CreateIndex(
                name: "IX_licenseplates_license_plate_name",
                table: "licenseplates",
                column: "license_plate_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parking_lots_DiscountId",
                table: "parking_lots",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction_id",
                table: "payments",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservations_parking_lot_id",
                table: "reservations",
                column: "parking_lot_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_LicensePlateId",
                table: "sessions",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_user_id",
                table: "vehicles",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discount-parking-lot");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "transaction_data");

            migrationBuilder.DropTable(
                name: "parking_lots");

            migrationBuilder.DropTable(
                name: "licenseplates");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "Discounts");
        }
    }
}
