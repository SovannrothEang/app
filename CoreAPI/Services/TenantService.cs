using System.Linq.Expressions;
using AutoMapper;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class TenantService(IUnitOfWork unitOfWork, IMapper mapper) : ITenantService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITransactionTypeRepository  _transactionTypeRepository = unitOfWork.TransactionTypeRepository;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TenantDto>> GetAllAsync(Expression<Func<Tenant, bool>>? filtering = null, CancellationToken cancellationToken = default)
    {
        var tenants = await _tenantRepository.GetAllAsync(filtering, cancellationToken);
        return tenants.Select(e => _mapper.Map<TenantDto>(e)).ToList();
    }

    public async Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct);
        return _mapper.Map<TenantDto>(tenant);
    }

    // Tenant Creation is in AuthService
    
    public async Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        _mapper.Map(dto, exist);
        await _tenantRepository.Update(exist);
        await _tenantRepository.SaveChangeAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var exist = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        await _tenantRepository.Remove(exist);
        await _tenantRepository.SaveChangeAsync(ct);
    }

    public async Task ActivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");

        tenant.Activate();
        await _tenantRepository.SaveChangeAsync(ct);
    }
    
    public async Task DeactivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}.");
        
        tenant.Deactivate();
        await _tenantRepository.SaveChangeAsync(ct);
    }
}