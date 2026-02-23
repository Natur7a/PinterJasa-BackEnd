using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinterJasa.API.DTOs.Services;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId)
    {
        var services = await _serviceService.GetActiveServicesAsync(categoryId);
        return Ok(services);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        return Ok(service);
    }

    [HttpPost]
    [Authorize(Roles = "provider")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var service = await _serviceService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "provider")]
    public async Task<IActionResult> GetMine()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var services = await _serviceService.GetMyServicesAsync(userId);
        return Ok(services);
    }
}
