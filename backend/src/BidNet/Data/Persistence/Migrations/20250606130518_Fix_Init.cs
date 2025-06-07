using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidNet.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "Roles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Roles",
                table: "Users",
                newName: "Role");
        }
    }
}
