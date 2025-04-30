using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToeGame.Migrations
{
    /// <inheritdoc />
    public partial class Init_Project_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TicTacToe");

            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicTacToeMatches",
                schema: "TicTacToe",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    BoardData = table.Column<byte[]>(type: "VARBINARY(MAX)", nullable: false),
                    Player1Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Player2Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsPlayer1Turn = table.Column<bool>(type: "bit", nullable: false),
                    TicTacToeMatchHistoryId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicTacToeMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicTacToeMatches_Users_Player1Id",
                        column: x => x.Player1Id,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicTacToeMatches_Users_Player2Id",
                        column: x => x.Player2Id,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicTacToeMatchHistories",
                schema: "TicTacToe",
                columns: table => new
                {
                    TicTacToeMatchId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicTacToeMatchHistories", x => x.TicTacToeMatchId);
                    table.ForeignKey(
                        name: "FK_TicTacToeMatchHistories_TicTacToeMatches_TicTacToeMatchId",
                        column: x => x.TicTacToeMatchId,
                        principalSchema: "TicTacToe",
                        principalTable: "TicTacToeMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicTacToeMatches_Player1Id",
                schema: "TicTacToe",
                table: "TicTacToeMatches",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TicTacToeMatches_Player2Id",
                schema: "TicTacToe",
                table: "TicTacToeMatches",
                column: "Player2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicTacToeMatchHistories",
                schema: "TicTacToe");

            migrationBuilder.DropTable(
                name: "TicTacToeMatches",
                schema: "TicTacToe");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Identity");
        }
    }
}
