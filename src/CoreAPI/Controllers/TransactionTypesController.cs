using Application;
using Application.DTOs.Transactions;
using Application.Services;
using Application.Validators.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("TransactionTypes")]
[Authorize(Constants.TenantScopeAccessPolicy)]
[RequireHttps]
public class TransactionTypesController(
    ITransactionTypeService typeService,
    ILogger<TransactionTypesController> logger) : ControllerBase
{
    private readonly ITransactionTypeService _typeService = typeService;
    private readonly ILogger<TransactionTypesController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> GetAllTransactionTypesAsync(CancellationToken ct = default)
    {
        var types = await _typeService.GetAllAsync(ct);
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionTypeByIdAsync([FromRoute] string id, CancellationToken ct = default)
    {
        var type = await _typeService.GetByIdAsync(id, ct);
        return Ok(type);
    }

    [HttpGet("slugs/{slug}")]
    public async Task<IActionResult> GetTransactionTypeBySlugAsync([FromRoute] string slug, CancellationToken ct = default)
    {
        var type = await _typeService.GetBySlugAsync(slug, ct);
        return Ok(type);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] TransactionTypeCreateDto dto,
        CancellationToken ct = default)
    {
        var result = new TransactionTypeCreateDtoValidator().Validate(dto);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Validation failed for TransactionTypeCreateDto: {Errors}", result.Errors);
            throw new BadHttpRequestException("Invalid data provided.");
        }
        var typeDto = await _typeService.CreateAsync(dto,ct);
        return Ok(typeDto);
    }

    //[HttpPut("{id}")]
    //public async Task<IActionResult> UpdateAsync(
    //    [FromRoute] string id,
    //    [FromBody] TransactionTypeUpdateDto dto,
    //    CancellationToken ct = default)
    //{
    //    var result = new TransactionTypeUpdateDtoValidator().Validate(dto);
    //    if (!result.IsValid)
    //    {
    //        if (_logger.IsEnabled(LogLevel.Warning))
    //            _logger.LogWarning("Validation failed for TransactionTypeUpdateDto: {Errors}", result.Errors);
    //        throw new BadHttpRequestException("Invalid data provided.");
    //    }
    //    var typeDto = await _typeService.UpdateAsync(id, dto, ct);
    //    return Ok(typeDto);
    //}
}
