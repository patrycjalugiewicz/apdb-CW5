using ClinicAPI.Data;
using ClinicAPI.DTOs;
using ClinicAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Services;

public interface IPrescriptionService
{
    Task<int> AddPrescriptionAsync(AddPrescriptionRequest request);
    Task<PatientDetailsDto> GetPatientDetailsAsync(int id);
}

public class PrescriptionService : IPrescriptionService
{
    private readonly DatabaseContext _context;
    
    public PrescriptionService(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task<int> AddPrescriptionAsync(AddPrescriptionRequest request)
    {

        if (request.DueDate < request.Date)
        {
            throw new ArgumentException("Data ważności (DueDate) nie może być wcześniejsza niż data wystawienia recepty.");
        }
        
        if (request.Medicaments.Count > 10)
        {
            throw new ArgumentException("Recepta może zawierać maksymalnie 10 leków.");
        }
        
        var patient = request.Patient.IdPatient.HasValue 
            ? await _context.Patients.FindAsync(request.Patient.IdPatient.Value)
            : null;
            
        if (patient == null)
        {
            patient = new Patient
            {
                FirstName = request.Patient.FirstName,
                LastName = request.Patient.LastName,
                Birthdate = request.Patient.Birthdate
            };
            
            _context.Patients.Add(patient);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new Exception("Błąd zapisu do zmiany danych", e);
            }
        }
        
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdDoctor == request.IdDoctor);
        if (doctor == null)
        {
            throw new ArgumentException($"Lekarz o ID={request.IdDoctor} nie istnieje.");
        }
        
        var medicamentIds = request.Medicaments.Select(m => m.IdMedicament).ToList();
        var existingMedicaments = await _context.Medicaments
            .Where(m => medicamentIds.Contains(m.IdMedicament))
            .Select(m => m.IdMedicament)
            .ToListAsync();
            
        if (existingMedicaments.Count != medicamentIds.Count)
        {
            throw new ArgumentException("Jeden lub więcej leków nie istnieje w bazie danych.");
        }
        
        var prescription = new Prescription
        {
            Date = request.Date,
            DueDate = request.DueDate,
            IdPatient = patient.IdPatient,
            IdDoctor = request.IdDoctor,
            PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Details = m.Description ?? "Stosować według zaleceń lekarza"
            }).ToList()
        };
        
        _context.Prescriptions.Add(prescription);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new Exception("Błąd zapisu do zmiany danych", e);
        }
 
        
        return prescription.IdPrescription;
    }
    
    public async Task<PatientDetailsDto> GetPatientDetailsAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Prescriptions)
                .ThenInclude(p => p.Doctor)
            .Include(p => p.Prescriptions)
                .ThenInclude(p => p.PrescriptionMedicaments)
                    .ThenInclude(pm => pm.Medicament)
            .FirstOrDefaultAsync(p => p.IdPatient == id);
            
        if (patient == null)
        {
            return null;
        }
        
        return new PatientDetailsDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions
                .OrderBy(p => p.DueDate)
                .Select(p => new PrescriptionDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorDto
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName,
                        LastName = p.Doctor.LastName,
                        Email = p.Doctor.Email
                    },
                    Medicaments = p.PrescriptionMedicaments
                        .Select(pm => new MedicamentDto
                        {
                            IdMedicament = pm.IdMedicament,
                            Description = pm.Details,
                            Dose = pm.Dose
                        }).ToList()
                }).ToList()
        };
    }
}
