using JohnForteLibrary.Domain;
using JohnForteLibrary.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JohnForteLibrary.Repositories.Data
{
    public class JohnForteLibraryDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Patron> LibraryCards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
<<<<<<< HEAD
            optionsBuilder.UseSqlServer(@"Server=CODYREEVES0FAB;Database=JohnForteLibrary;Trusted_Connection=True;");
=======
<<<<<<< HEAD
            optionsBuilder.UseSqlServer(@"Server=BRADENCOONEC25F;Database=library;Trusted_Connection=True;");
=======
            optionsBuilder.UseSqlServer(@"Server=CODYREEVES0FAB;Database=JohnForteLibrary;Trusted_Connection=True;");
>>>>>>> a7271422b3ad4392f68a75b0f27289a0de4252b8
>>>>>>> 7f2cad861951d110cf2838c2e4e2ff0d141ea715
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Book>()
                .Property(i => i.Id).HasColumnName("BookId");

            modelBuilder.Entity<Book>()
                .Property(p => p.ISBN)
                    .HasConversion(p => p.Value, p => ISBN.Create(p).Value);

            modelBuilder.Entity<Book>()
            .HasOne<Patron>(b => b.Patron)
            .WithMany(p => p.CheckedOutBooks)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Author>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Author>()
                .Property(i => i.Id).HasColumnName("AuthorId");

            modelBuilder.Entity<Book>()
                .HasMany(a => a.Authors)
                .WithMany(b => b.Books)
                .UsingEntity(join => join.ToTable("BookAuthor"));

            modelBuilder.Entity<Patron>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Patron>()
                .Property(x => x.Id).HasColumnName("PatronId");

            modelBuilder.Entity<Patron>()
                .OwnsOne(p => p.Name, p =>
                {
                    p.Property(pp => pp.FirstName).HasColumnName("FirstName");
                    p.Property(pp => pp.LastName).HasColumnName("LastName");
                });

            modelBuilder.Entity<Patron>().OwnsOne(p => p.Address, p =>
            {
                p.Property(pp => pp.StreetName).HasColumnName("StreetAddress");
                p.Property(pp => pp.City).HasColumnName("City");
                p.Property(pp => pp.State).HasConversion<string>().HasColumnName("State");
                p.Property(pp => pp.ZipCode).HasColumnName("ZipCode");
            });
            modelBuilder.Entity<Patron>().Property(p => p.Email).HasConversion(p => p.Value, p => EmailAddress.Create(p).Value);
            modelBuilder.Entity<Patron>().Property(p => p.PhoneNumber).HasConversion(p => p.Value, p => PhoneNumber.Create(p).Value);

            modelBuilder.Entity<Patron>().OwnsOne(p => p.Card, p =>
            {
                p.Property(pp => pp.CardNumber).HasColumnName("CardNumber");
            });

        }
    }
}
