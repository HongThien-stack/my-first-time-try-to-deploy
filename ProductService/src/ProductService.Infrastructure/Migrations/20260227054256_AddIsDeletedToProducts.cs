using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    original_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    cost_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    weight = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    volume = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    quantity_per_unit = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    min_order_quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_order_quantity = table.Column<int>(type: "int", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    shelf_life_days = table.Column<int>(type: "int", nullable: true),
                    storage_instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_perishable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_new = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_on_sale = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    meta_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    meta_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    meta_keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_categories_status",
                table: "categories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_products_barcode",
                table: "products",
                column: "barcode",
                unique: true,
                filter: "[barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_products_brand",
                table: "products",
                column: "brand");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_created_at",
                table: "products",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_available",
                table: "products",
                column: "is_available");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_deleted",
                table: "products",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_featured",
                table: "products",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_on_sale",
                table: "products",
                column: "is_on_sale");

            migrationBuilder.CreateIndex(
                name: "IX_products_name",
                table: "products",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_products_price",
                table: "products",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "IX_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_slug",
                table: "products",
                column: "slug",
                unique: true,
                filter: "[slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
