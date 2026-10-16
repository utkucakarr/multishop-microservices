using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiShop.Comment.Migrations
{
    /// <inheritdoc />
    public partial class CommentModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elle eklendi: sütunlar daraltılmadan önce sınırı aşan eski değerler kırpılır; yoksa ALTER COLUMN
            // "String or binary data would be truncated" ile durur. bit → int dönüşümü veri korur:
            // false → 0 (Pending), true → 1 (Approved).
            migrationBuilder.Sql("UPDATE UserComments SET ProductId = LEFT(ProductId, 50) WHERE LEN(ProductId) > 50");
            migrationBuilder.Sql("UPDATE UserComments SET NameSurname = LEFT(NameSurname, 100) WHERE LEN(NameSurname) > 100");
            migrationBuilder.Sql("UPDATE UserComments SET Email = LEFT(Email, 256) WHERE LEN(Email) > 256");
            migrationBuilder.Sql("UPDATE UserComments SET CommentDetail = LEFT(CommentDetail, 1000) WHERE LEN(CommentDetail) > 1000");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "UserComments",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "UserComments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameSurname",
                table: "UserComments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserComments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CommentDetail",
                table: "UserComments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserComments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserComments_ProductId_Status",
                table: "UserComments",
                columns: new[] { "ProductId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserComments_ProductId_Status",
                table: "UserComments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserComments");

            // Elle eklendi: int → bit'te 2 (Rejected) 1'e (onaylı) dönerdi; reddedilenler onaysız kalsın.
            migrationBuilder.Sql("UPDATE UserComments SET Status = 0 WHERE Status = 2");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "UserComments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "UserComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NameSurname",
                table: "UserComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CommentDetail",
                table: "UserComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
