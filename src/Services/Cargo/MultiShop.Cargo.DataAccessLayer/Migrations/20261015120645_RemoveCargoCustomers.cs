using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiShop.Cargo.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCargoCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bilerek: CargoCustomer hiçbir akışta kullanılmıyordu (yalnızca Swagger'dan elle girilebiliyordu); alıcı bilgisi
            // KT-13'ten beri gönderinin kendisinde (CreateShipment). Admin'in kullanıcı adres sayfası Order adreslerini gösterir.
            migrationBuilder.DropTable(
                name: "CargoCustomers");

            // Elle eklendi: sütun daraltılmadan önce uzun firma adları kırpılır (yoksa ALTER COLUMN durur).
            migrationBuilder.Sql("UPDATE CargoCompanies SET CargoCompanyName = LEFT(CargoCompanyName, 100) WHERE LEN(CargoCompanyName) > 100");

            migrationBuilder.AlterColumn<string>(
                name: "CargoCompanyName",
                table: "CargoCompanies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CargoCompanyName",
                table: "CargoCompanies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "CargoCustomers",
                columns: table => new
                {
                    CargoCustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoCustomers", x => x.CargoCustomerId);
                });
        }
    }
}
