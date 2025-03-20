using BackPatient.Models;
using BackPatient.Repository.Interface;
using BackPatient.Service.Interface;

namespace BackPatient.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Patient>> GetAllPatients() => await _repository.GetAllPatients();

        public async Task<Patient> GetPatientById(int id) => await _repository.GetPatientById(id);

        public async Task AddPatient(Patient patient) => await _repository.AddPatient(patient);

        public async Task UpdatePatient(Patient patient) => await _repository.UpdatePatient(patient);

        public async Task DeletePatient(int id) => await _repository.DeletePatient(id);
    }
}
