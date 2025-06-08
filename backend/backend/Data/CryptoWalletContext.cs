using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class CryptoWalletContext : DbContext
    {
        public CryptoWalletContext(DbContextOptions<CryptoWalletContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Cryptocurrency> Cryptocurrencies { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Cryptocurrency>().ToTable("Cryptocurrencies");
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<WalletBalance>().ToTable("WalletBalances");
        }
    }
}
