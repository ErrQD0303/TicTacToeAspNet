using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToeGame.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Tic_Tac_Toe_Match_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicTacToeMatchHistoryId",
                schema: "TicTacToe",
                table: "TicTacToeMatches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicTacToeMatchHistoryId",
                schema: "TicTacToe",
                table: "TicTacToeMatches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
