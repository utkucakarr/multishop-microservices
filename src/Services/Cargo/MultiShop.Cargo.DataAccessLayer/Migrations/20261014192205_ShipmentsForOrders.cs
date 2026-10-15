using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiShop.Cargo.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ShipmentsForOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargoDetails_CargoCompanies_CargoCompanyId",
                table: "CargoDetails");

            // Elle düzenlendi: EF SenderCustomer → District, ReceiverCustomer → Country yeniden adlandırması önermişti (yanlış).
            // District ve Country yeni sütun; eski alıcı adı aşağıda ReceiverName'e taşınır, eski iki sütun en sonda silinir.
            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Hareket barkodu nvarchar(max) → nvarchar(32): daha uzun eski değer daraltmayı bozmasın.
            migrationBuilder.Sql("UPDATE CargoOperations SET Barcode = LEFT(Barcode, 32)");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "CargoOperations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CargoDetailId",
                table: "CargoOperations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CargoCompanyId",
                table: "CargoDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "CargoDetails",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CargoDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "CargoDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchedAt",
                table: "CargoDetails",
                type: "datetime2",
                nullable: true);

            // Elle düzenlendi: önce nullable; eski kayıtlara aşağıda NEWID() verilir (OrderNumber unique).
            migrationBuilder.AddColumn<Guid>(
                name: "OrderNumber",
                table: "CargoDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderingId",
                table: "CargoDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverEmail",
                table: "CargoDetails",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "CargoDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverPhone",
                table: "CargoDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CargoDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CargoDetails",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "CargoCompanies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveCount = table.Column<int>(type: "int", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Consumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnqueueTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            // Elle eklendi: KT-13 öncesi serbest metinli kargo kayıtları. Siparişe bağlı değiller; teslim edilmiş sayılırlar,
            // alıcı adı ReceiverCustomer'dan alınır, her birine benzersiz bir sipariş numarası verilir.
            migrationBuilder.Sql(@"UPDATE CargoDetails
                SET OrderNumber = NEWID(), ReceiverName = LEFT(ReceiverCustomer, 200), Status = 2, CreatedAt = SYSUTCDATETIME()
                WHERE OrderNumber IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderNumber",
                table: "CargoDetails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // CAR-01: hareketler eskiden gönderiye yalnızca barkod metniyle bağlıydı; eşleşenler FK ile bağlanır.
            migrationBuilder.Sql(@"UPDATE o SET CargoDetailId = d.CargoDetailId
                FROM CargoOperations o JOIN CargoDetails d ON o.Barcode = d.Barcode");

            // Eski sayısal barkodlar tekrar edebilir; unique index'ten önce tekrar edenlere kayıt id'si eklenir (ör. 123-7).
            migrationBuilder.Sql(@"UPDATE d SET Barcode = CONCAT(d.Barcode, '-', d.CargoDetailId)
                FROM CargoDetails d
                WHERE EXISTS (SELECT 1 FROM CargoDetails d2 WHERE d2.Barcode = d.Barcode AND d2.CargoDetailId < d.CargoDetailId)");

            migrationBuilder.DropColumn(
                name: "SenderCustomer",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "ReceiverCustomer",
                table: "CargoDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CargoOperations_CargoDetailId",
                table: "CargoOperations",
                column: "CargoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoDetails_Barcode",
                table: "CargoDetails",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargoDetails_OrderNumber",
                table: "CargoDetails",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true,
                filter: "[InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true,
                filter: "[OutboxId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");

            migrationBuilder.AddForeignKey(
                name: "FK_CargoDetails_CargoCompanies_CargoCompanyId",
                table: "CargoDetails",
                column: "CargoCompanyId",
                principalTable: "CargoCompanies",
                principalColumn: "CargoCompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoOperations_CargoDetails_CargoDetailId",
                table: "CargoOperations",
                column: "CargoDetailId",
                principalTable: "CargoDetails",
                principalColumn: "CargoDetailId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargoDetails_CargoCompanies_CargoCompanyId",
                table: "CargoDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_CargoOperations_CargoDetails_CargoDetailId",
                table: "CargoOperations");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxState");

            migrationBuilder.DropIndex(
                name: "IX_CargoOperations_CargoDetailId",
                table: "CargoOperations");

            migrationBuilder.DropIndex(
                name: "IX_CargoDetails_Barcode",
                table: "CargoDetails");

            migrationBuilder.DropIndex(
                name: "IX_CargoDetails_OrderNumber",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "CargoDetailId",
                table: "CargoOperations");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "DispatchedAt",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "OrderingId",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "ReceiverEmail",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "ReceiverPhone",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "CargoCompanies");

            // Elle düzenlendi (Up ile simetrik). Down kayıplıdır: alıcı adresleri ve harf içeren barkodlar geri dönmez.
            migrationBuilder.DropColumn(
                name: "District",
                table: "CargoDetails");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "CargoDetails");

            migrationBuilder.AddColumn<string>(
                name: "SenderCustomer",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverCustomer",
                table: "CargoDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "CargoOperations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<int>(
                name: "CargoCompanyId",
                table: "CargoDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Barcode",
                table: "CargoDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoDetails_CargoCompanies_CargoCompanyId",
                table: "CargoDetails",
                column: "CargoCompanyId",
                principalTable: "CargoCompanies",
                principalColumn: "CargoCompanyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
