using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultiShop.Notification.Migrations
{
    /// <inheritdoc />
    public partial class OrderNotificationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationRules",
                columns: new[] { "Type", "DisplayName", "EmailEnabled", "InAppEnabled", "SmsEnabled" },
                values: new object[,]
                {
                    { "new-order", "Yeni sipariş (admin'e)", false, true, false },
                    { "order-paid", "Sipariş onayı (müşteriye)", true, false, false },
                    { "order-shipped", "Kargoya verildi (müşteriye)", true, false, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationRules",
                keyColumn: "Type",
                keyValue: "new-order");

            migrationBuilder.DeleteData(
                table: "NotificationRules",
                keyColumn: "Type",
                keyValue: "order-paid");

            migrationBuilder.DeleteData(
                table: "NotificationRules",
                keyColumn: "Type",
                keyValue: "order-shipped");
        }
    }
}
