namespace BackPatient.Models.DTOs
{
    public class PatientCreateDto
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Genre { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
    }
}
