using ClinicAPI.DTOs;
using ClinicAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    
    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var idPrescription = await _prescriptionService.AddPrescriptionAsync(request);
            return Created($"/api/Prescriptions/{idPrescription}", null);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Wystąpił błąd podczas dodawania recepty");
        }
    }
    
    [HttpGet("patient/{id}")]
    public async Task<IActionResult> GetPatientDetails(int id)
    {
        var patient = await _prescriptionService.GetPatientDetailsAsync(id);
        
        if (patient == null)
        {
            return NotFound($"Pacjent o ID={id} nie został znaleziony");
        }
        
        return Ok(patient);
    }
}