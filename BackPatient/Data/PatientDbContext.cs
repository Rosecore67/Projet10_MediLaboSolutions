using BackPatient.Models;
using Microsoft.EntityFrameworkCore;

namespace BackPatient.Data
{

    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Patient>()
                .Property(p => p.DateNaissance)
                .HasColumnType("DATE");
        }
    }
}
