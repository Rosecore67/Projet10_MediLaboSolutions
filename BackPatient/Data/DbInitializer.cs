using BackPatient.Models;

namespace BackPatient.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PatientDbContext context)
        {
            if (context.Patients.Any())
                return;

            var patients = new Patient[]
            {
                new() { Nom = "TestNone", Prenom = "Test", DateNaissance = new DateTime(1966, 12, 31), Genre = "F", Adresse = "1 Brookside St", Telephone = "100-222-3333" },
                new() { Nom = "TestBorderline", Prenom = "Test", DateNaissance = new DateTime(1945, 6, 24), Genre = "M", Adresse = "2 High St", Telephone = "200-333-4444" },
                new() { Nom = "TestInDanger", Prenom = "Test", DateNaissance = new DateTime(2004, 6, 18), Genre = "M", Adresse = "3 Club Road", Telephone = "300-444-5555" },
                new() { Nom = "TestEarlyOnset", Prenom = "Test", DateNaissance = new DateTime(2002, 6, 28), Genre = "F", Adresse = "4 Valley Dr", Telephone = "400-555-6666" },
            };

            context.Patients.AddRange(patients);
            context.SaveChanges();
        }
    }
}
