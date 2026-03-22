using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpPost("suppliers/add")]
    public async Task<ActionResult> AddNewSupplier([FromBody] SupplierRequest supplierRequest)
    {
        await _supplierService.AddNewSupplierAsync(supplierRequest);
        return Ok("Supplier added successfully.");
    }

    [HttpGet("suppliers")]
    public async Task<ActionResult<List<Supplier>>> GetAllSuppliers()
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        return Ok(suppliers);
    }

    [HttpGet("suppliers/{supplierId}")]
    public async Task<ActionResult<Supplier>> GetSupplierBySupplierId([FromRoute] Guid supplierId)
    {
        var supplier = await _supplierService.GetSupplierBySupplierIdAsync(supplierId);
        return Ok(supplier);
    }

    [HttpPatch("suppliers/{supplierId}")]
    public async Task<ActionResult> UpdateSupplierAsync([FromRoute] Guid supplierId, [FromBody] SupplierRequest supplierRequest)
    {
        var supplier = await _supplierService.GetSupplierBySupplierIdAsync(supplierId);
        if (supplier != null)
        {
            await _supplierService.UpdateSupplierAsync(supplier, supplierRequest);
        }
        return Ok("Supplier updated successfully.");
    }

    [HttpDelete("suppliers/{supplierId}")]
    public async Task<ActionResult> DisableSupplierAsync([FromRoute] Guid supplierId)
    {
        var supplier = await _supplierService.GetSupplierBySupplierIdAsync(supplierId);
        if (supplier != null)
        {
            await _supplierService.DisableSupplierAsync(supplier);
        }
        return Ok("Supplier disabled successfully.");
    }
}
