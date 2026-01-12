using AutoMapper;
using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    public class AccountTypesController(
        IAccountTypeRepository accountTypeRepository,
        IMapper mapper,
        ICurrentUserProvider currentUserProvider) : ControllerBase
    {
        private readonly IAccountTypeRepository _accountTypeRepository = accountTypeRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

        [HttpGet]
        public async Task<IActionResult> GetAccountTypes(CancellationToken cancellationToken = default)
        {
            var types =  await _accountTypeRepository.GetAccountTypesAsync(cancellationToken);
            return Ok(types);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountType(
            [FromBody] AccountTypeCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var type = _mapper.Map<AccountType>(dto);
            await _accountTypeRepository.CreateAccountTypeAsync(type, cancellationToken);
            return Ok(_mapper.Map<AccountType, AccountTypeDto>(type));
            // return Ok(type);
        }
    }
}
