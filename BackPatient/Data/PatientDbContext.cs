using BackPatient.Models;
using Microsoft.EntityFrameworkCore;

namespace BackPatient.Data
{

    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; }
    }
}
