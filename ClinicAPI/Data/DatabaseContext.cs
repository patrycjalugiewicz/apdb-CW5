using ClinicAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        base.OnModelCreating(modelBuilder);

    
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasKey(pm => new { pm.IdPrescription, pm.IdMedicament });


        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany(p => p.Prescriptions)
            .HasForeignKey(p => p.IdPatient)
            .OnDelete(DeleteBehavior.Restrict);
        
         modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Doctor)
            .WithMany(p => p.Prescriptions)
            .HasForeignKey(p => p.IdDoctor)
                .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Medicament)
            .WithMany(m => m.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdMedicament)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Prescription)
            .WithMany(p => p.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdPrescription)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Doctor>().HasData(new List<Doctor>
        {
            new Doctor { IdDoctor = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan.kowalski@clinic.pl" },
            new Doctor { IdDoctor = 2, FirstName = "Anna", LastName = "Nowak", Email = "anna.nowak@clinic.pl" },
            new Doctor { IdDoctor = 3, FirstName = "Piotr", LastName = "Wiśniewski", Email = "piotr.wisniewski@clinic.pl" }
        });
    

        modelBuilder.Entity<Patient>().HasData(new List<Patient>
        {
            new Patient { IdPatient = 1, FirstName = "Adam", LastName = "Mickiewicz", Birthdate = new DateTime(1990, 5, 15) },
            new Patient { IdPatient = 2, FirstName = "Ewa", LastName = "Kowalska", Birthdate = new DateTime(1985, 10, 22) },
            new Patient { IdPatient = 3, FirstName = "Maria", LastName = "Dąbrowska", Birthdate = new DateTime(2000, 2, 28) }
        });
    

        modelBuilder.Entity<Medicament>().HasData(new List<Medicament>
        {
            new Medicament { IdMedicament = 1, Name = "Apap", Description = "Przeciwbólowy", Type = "Tabletki" },
             new Medicament { IdMedicament = 2, Name = "Ibuprom", Description = "Przeciwzapalny", Type = "Tabletki" },
            new Medicament { IdMedicament = 3, Name = "Rutinoscorbin", Description = "Witamina C", Type = "Tabletki" },
            new Medicament { IdMedicament = 4, Name = "Amoksiklav", Description = "Antybiotyk", Type = "Syrop" }
         });
    }

}
