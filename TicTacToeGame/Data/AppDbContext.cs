using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicTacToeGame.Helpers;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Enum;
using TicTacToeGame.Models;

namespace TicTacToeGame.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; } = default!;
    public DbSet<TicTacToeMatch> TicTacToeMatches { get; set; } = default!;
    public DbSet<TicTacToeMatchHistory> TicTacToeMatchHistories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(AppConstants.Tables.Schema);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable(AppConstants.Tables.IdentityTables.AppUser, AppConstants.Tables.IdentityTables.Schema);
            e.HasKey(e => e.Id);

            e.Property(e => e.Id).HasMaxLength(100);
            e.Property(e => e.UserName).IsRequired().HasMaxLength(50);
            e.Property(e => e.Email).IsRequired().HasMaxLength(100);
            e.Property(e => e.Name).IsRequired().HasMaxLength(100);
            e.Property(e => e.HashedPassword).IsRequired().HasMaxLength(100);
            e.Ignore(e => e.TicTacToeMatches);
        });

        modelBuilder.Entity<TicTacToeMatch>(e =>
        {
            e.ToTable(AppConstants.Tables.TicTacToeTables.TicTacToeMatch, AppConstants.Tables.TicTacToeTables.Schema);
            e.HasKey(e => e.Id);

            e.HasOne(e => e.Player1)
                .WithMany(e => e.MatchesAsPlayer1)
                .HasForeignKey(e => e.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(e => e.Player2)
                .WithMany(e => e.MatchesAsPlayer2)
                .HasForeignKey(e => e.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(e => e.TicTacToeMatchHistory)
                .WithOne(e => e.TicTacToeMatch)
                .HasForeignKey<TicTacToeMatchHistory>(e => e.TicTacToeMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(e => e.Row).IsRequired();
            e.Property(e => e.Column).IsRequired();
            e.Ignore(e => e.Board);
            e.Property(g => g.BoardData)
                .IsRequired()
                .HasColumnType("VARBINARY(MAX)");
            e.Property(e => e.Player1Id).IsRequired().HasMaxLength(100);
            e.Property(e => e.Player2Id).IsRequired().HasMaxLength(100);
            e.Property(e => e.IsPlayer1Turn).IsRequired();
            e.Property(e => e.TicTacToeMatchHistoryId).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<TicTacToeMatchHistory>(e =>
        {
            e.ToTable(AppConstants.Tables.TicTacToeTables.TicTacToeMatchHistory, AppConstants.Tables.TicTacToeTables.Schema);
            e.HasKey(e => e.TicTacToeMatchId);

            e.Property(e => e.Result).IsRequired();
            e.Property(e => e.CreatedAt).IsRequired();
        });
    }
}