using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.DTOs.Tenants;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;
using CoreAPI.Models.Enums;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Test;

[TestFixture]
public class TransactionServiceTest
{
    private TransactionService _transactionService;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<ITransactionRepository> _transactionRepoMock;
    private Mock<ITenantRepository> _tenantRepoMock;
    private Mock<ICustomerRepository> _customerRepoMock;
    private Mock<IAccountTypeRepository> _accountTypeRepoMock;
    private Mock<ICurrentUserProvider> _currentUserProviderMock;
    private Mock<ILogger<TransactionService>> _loggerMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ITransactionTypeService> _transactionTypeServiceMock;
    private Mock<IDbContextTransaction> _dbTransactionMock;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _accountTypeRepoMock = new Mock<IAccountTypeRepository>();
        _currentUserProviderMock = new Mock<ICurrentUserProvider>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TransactionService>>();
        _transactionTypeServiceMock = new Mock<ITransactionTypeService>();
        _dbTransactionMock = new Mock<IDbContextTransaction>();

        _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TenantRepository).Returns(_tenantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CustomerRepository).Returns(_customerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.AccountTypeRepository).Returns(_accountTypeRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbTransactionMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _dbTransactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _dbTransactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _dbTransactionMock.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        _customerRepoMock.Setup(r => r.ExistsInTenantAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _transactionService = new TransactionService(
            _unitOfWorkMock.Object,
            _currentUserProviderMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _transactionTypeServiceMock.Object);

        // Setup default user
        _currentUserProviderMock.Setup(p => p.UserId).Returns("test-user-id");

        // Setup Mapper default (can be overridden in tests if needed)
        _mapperMock.Setup(m => m.Map<TransactionDto>(It.IsAny<Transaction>()))
            .Returns((Transaction t) => new TransactionDto { Id = "mapped-trans-id", Amount = t.Amount });
        _mapperMock.Setup(m => m.Map<TenantDto>(It.IsAny<Tenant>()))
            .Returns((Tenant t) => new TenantDto(t.Id, t.Name, t.Status, null, DateTimeOffset.UtcNow, null));
    }

    [Test]
    public async Task PostTransactionAsync_ShouldSucceed_WhenValidRequest_AndAccountExists()
    {
        // Arrange
        var customerId = "cust1";
        var tenantId = "tenant1";
        var accountTypeId = "type1";
        var slug = "earn";
        var dto = new PostTransactionDto(100m, "Test Reason", "REF123", DateTimeOffset.UtcNow);

        var tenant = new Tenant(tenantId, "Test Tenant", "tenant-slug");
        var customer = new Customer(customerId, "user1", "creator");

        customer.CreateAccount(tenantId, accountTypeId);
        var account = customer.Accounts.First();

        var transTypeDto = new TransactionTypeDto("tt1", slug, "Earn", null, 1, false);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _customerRepoMock.Setup(r => r.GetByIdForCustomerAsync(customerId, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transTypeDto);

        // Act
        var result = await _transactionService.PostTransactionAsync(customerId, tenantId, accountTypeId, slug, dto);

        // Assert
        Assert.That(result.balance, Is.EqualTo(100m));
        Assert.That(account.Balance, Is.EqualTo(100m));
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dbTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostTransactionAsync_ShouldCreateAccount_WhenAccountDoesNotExist_AndAmountIsPositive()
    {
        // Arrange
        var customerId = "cust2";
        var tenantId = "tenant2";
        var accountTypeId = "type2";
        var slug = "earn";
        var dto = new PostTransactionDto(50m, "Opening Balance", "REF456", DateTimeOffset.UtcNow);

        var tenant = new Tenant(tenantId, "Test Tenant 2", "tenant-slug-2");
        var customer = new Customer(customerId, "user2", "creator");
        // No account created initially

        var transTypeDto = new TransactionTypeDto("tt2", slug, "Earn", null, 1, false);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _customerRepoMock.Setup(r => r.GetByIdForCustomerAsync(customerId, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transTypeDto);

        _accountTypeRepoMock.Setup(r => r.ExistsAsync(accountTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _transactionService.PostTransactionAsync(customerId, tenantId, accountTypeId, slug, dto);

        // Assert
        Assert.That(result.balance, Is.EqualTo(50m));
        Assert.That(customer.Accounts.Count, Is.EqualTo(1));
        Assert.That(customer.Accounts.First().AccountTypeId, Is.EqualTo(accountTypeId));
        Assert.That(customer.Accounts.First().Balance, Is.EqualTo(50m));
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenTransactionTypeNotFound()
    {
        // Arrange
        var customerId = "cust1";
        var tenantId = "tenant1";
        var slug = "invalid-slug";
        var dto = new PostTransactionDto(100m, "Test", null, null);

        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionTypeDto?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _transactionService.PostTransactionAsync(customerId, tenantId, "type1", slug, dto));

        Assert.That(ex.Message, Does.Contain("Invalid Transaction Type"));
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Rollback is called in catch block
        _dbTransactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenNegativeAmountNotAllowed()
    {
        // Arrange
        var customerId = "cust1";
        var slug = "earn";
        var dto = new PostTransactionDto(-50m, "Negative", null, null);
        var transType = new TransactionTypeDto("tt1", slug, "Earn", null, 1, false);

        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transType);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _transactionService.PostTransactionAsync(customerId, "tenant1", "type1", slug, dto));

        Assert.That(ex.Message, Does.Contain("Negative amounts are not allowed"));
        _dbTransactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenTenantNotFound()
    {
        // Arrange
        var tenantId = "invalid-tenant";
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var dto = new PostTransactionDto(100m, "Test", null, null);
        var transType = new TransactionTypeDto("tt1", "earn", "Earn", null, 1, false);
        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync("earn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transType);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _transactionService.PostTransactionAsync("cust1", tenantId, "type1", "earn", dto));

        Assert.That(ex.Message, Does.Contain($"No Tenant was found with id: {tenantId}"));
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenTenantNotActive()
    {
        // Arrange
        var tenantId = "tenant-suspended";
        var tenant = new Tenant(tenantId, "Suspended", "suspended-slug");
        tenant.Deactivate(); // Set to Inactive

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var dto = new PostTransactionDto(100m, "Test", null, null);
        var transType = new TransactionTypeDto("tt1", "earn", "Earn", null, 1, false);
        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync("earn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transType);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _transactionService.PostTransactionAsync("cust1", tenantId, "type1", "earn", dto));

        Assert.That(ex.Message, Does.Contain("Tenant is not active"));
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = "invalid-cust";
        var tenantId = "tenant1";
        var tenant = new Tenant(tenantId, "Tenant", "slug");

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _customerRepoMock.Setup(r => r.GetByIdForCustomerAsync(customerId, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var dto = new PostTransactionDto(100m, "Test", null, null);
        var transType = new TransactionTypeDto("tt1", "earn", "Earn", null, 1, false);
        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync("earn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transType);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _transactionService.PostTransactionAsync(customerId, tenantId, "type1", "earn", dto));

        Assert.That(ex.Message, Does.Contain($"No Customer was found with id: {customerId}"));
    }

    [Test]
    public void PostTransactionAsync_ShouldThrow_WhenAccountNotFound_AndNoCreationConditionMet()
    {
        // Condition: Account doesn't exist, and (AccountType doesn't exist OR Amount isn't sufficient to trigger creation?)
        // Case: Amount is positive, but AccountType does not exist.
        var customerId = "cust1";
        var tenantId = "tenant1";
        var accountTypeId = "invalid-type";
        var dto = new PostTransactionDto(100m, "Test", null, null);

        var tenant = new Tenant(tenantId, "Tenant", "slug");
        var customer = new Customer(customerId, "user1", "creator"); // No accounts
        var transTypeDto = new TransactionTypeDto("tt1", "earn", "Earn", null, 1, false);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _customerRepoMock.Setup(r => r.GetByIdForCustomerAsync(customerId, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync("earn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transTypeDto);

        // Mock AccountTypeRepo to return false
        _accountTypeRepoMock.Setup(r => r.ExistsAsync(accountTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _transactionService.PostTransactionAsync(customerId, tenantId, accountTypeId, "earn", dto));

        Assert.That(ex.Message, Does.Contain($"No account was found for customer: {customerId}"));
    }

    [Test]
    public async Task PostTransactionAsync_ShouldRetry_OnConcurrencyException()
    {
        // Test Retry Logic
        var customerId = "cust1";
        var tenantId = "tenant1";
        var accountTypeId = "type1";
        var slug = "earn";
        var dto = new PostTransactionDto(100m, "Retry Test", null, null);

        var tenant = new Tenant(tenantId, "Tenant", "slug");
        var customer = new Customer(customerId, "user1", "creator");
        customer.CreateAccount(tenantId, accountTypeId);

        var transType = new TransactionTypeDto("tt1", slug, "Earn", null, 1, false);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _customerRepoMock.Setup(r => r.GetByIdForCustomerAsync(customerId, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _transactionTypeServiceMock.Setup(s => s.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transType);

        // Mock UnitOfWork.CompleteAsync to throw DbUpdateConcurrencyException first time, then succeed
        var callCount = 0;
        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                callCount++;
                if (callCount == 1) throw new DbUpdateConcurrencyException("Conflict", new List<Microsoft.EntityFrameworkCore.Update.IUpdateEntry>());
            })
            .ReturnsAsync(1);

        // Act
        var result = await _transactionService.PostTransactionAsync(customerId, tenantId, accountTypeId, slug, dto);

        // Assert
        Assert.That(callCount, Is.EqualTo(2)); // Called twice (1 failure + 1 success)
        Assert.That(result.balance, Is.EqualTo(100m));

        // Verify Rollback called for the first failure (retry logic)
        // Verify Commit called for the success
        _dbTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}