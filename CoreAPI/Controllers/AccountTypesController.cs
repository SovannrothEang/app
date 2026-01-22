using AutoMapper;
using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    public class AccountTypesController(IAccountTypeService accountTypeService) : ControllerBase
    {
        private readonly IAccountTypeService _accountTypeService = accountTypeService;

        [HttpGet]
        public async Task<IActionResult> GetAccountTypes(CancellationToken cancellationToken = default)
        {
            var types =  await _accountTypeService.GetAllAsync(cancellationToken);
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountTypeById(string id, CancellationToken cancellationToken = default)
        {
            var type = await _accountTypeService.GetByIdAsync(id, cancellationToken);
            if (type == null)
            {
                return NotFound();
            }
            return Ok(type);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountType(
            [FromBody] AccountTypeCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var type = await _accountTypeService.CreateAsync(dto, cancellationToken);
            return Ok(type);
            // return Ok(type);
        }
    }
}
