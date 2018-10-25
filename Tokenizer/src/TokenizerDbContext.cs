
namespace Tokenizer.src
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using Tokenizer.src.Models;

    public class TokenizerDbContext : DbContext
    {
        public DbSet<TokenModel> Tokens { get; set; }

        public TokenizerDbContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"Server=.;Database=Tokenizer;Trusted_Connection=True;MultipleActiveResultSets=true");
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Establish composite key.
            modelBuilder.Entity<TokenModel>().HasKey(table => new
            {
                table.Word, table.DocumentId
            });
        }

    }
}
