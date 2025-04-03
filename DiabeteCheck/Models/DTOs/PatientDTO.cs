namespace DiabeteCheck.Models.DTOs
{
    public class PatientDTO
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Genre { get; set; }
        public DateTime DateNaissance { get; set; }
    }
}
