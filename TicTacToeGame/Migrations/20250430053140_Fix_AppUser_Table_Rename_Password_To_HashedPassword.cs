using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToeGame.Migrations
{
    /// <inheritdoc />
    public partial class Fix_AppUser_Table_Rename_Password_To_HashedPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                schema: "Identity",
                table: "Users",
                newName: "HashedPassword");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HashedPassword",
                schema: "Identity",
                table: "Users",
                newName: "Password");
        }
    }
}
