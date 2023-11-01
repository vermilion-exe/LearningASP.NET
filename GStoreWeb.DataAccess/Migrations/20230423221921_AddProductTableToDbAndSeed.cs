using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GStoreWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTableToDbAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Seller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Seller" },
                values: new object[,]
                {
                    { 1, 1, "An Nvidia RTX 20 series base graphics card", "Nvidia RTX 2060", 280.0, "AMD" },
                    { 2, 1, "An Nvidia RTX 30 series base graphics card", "Nvidia RTX 3060", 380.0, "Gigabyte" },
                    { 3, 1, "An Nvidia RTX 30 series powerful graphics card", "Nvidia RTX 3080", 1200.0, "MSI" },
                    { 4, 1, "The Ti version of the RTX 3080 graphics card", "Nvidia RTX 3080 Ti", 1450.0, "Gigabyte" },
                    { 5, 1, "An Nvidia RTX 30 series most powerful graphics card", "Nvidia RTX 3090", 1780.0, "Gigabyte" },
                    { 6, 1, "An Nvidia RTX 40 series base graphic card", "Nvidia RTX 4070", 690.0, "ASUS" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
