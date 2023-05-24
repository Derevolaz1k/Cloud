using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloud.Migrations
{
    /// <inheritdoc />
    public partial class UploadsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeleteAfterDownload",
                table: "Uploads",
                newName: "DeleteAfterView");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeleteAfterView",
                table: "Uploads",
                newName: "DeleteAfterDownload");
        }
    }
}
