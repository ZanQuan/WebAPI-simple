using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI_simple.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexBookAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_Authors_BookId",
                table: "Books_Authors");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Authors_BookId_AuthorId",
                table: "Books_Authors",
                columns: new[] { "BookId", "AuthorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_Authors_BookId_AuthorId",
                table: "Books_Authors");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Authors_BookId",
                table: "Books_Authors",
                column: "BookId");
        }
    }
}
