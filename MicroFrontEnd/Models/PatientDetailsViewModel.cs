using MicroFrontEnd.Models.DTOs;

namespace MicroFrontEnd.Models
{
    public class PatientDetailsViewModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Genre { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }

        public List<NoteDTO>? Notes { get; set; } = new();
        public string NiveauRisque { get; set; }
    }
}
