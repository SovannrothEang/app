# Unused/Useless Code Analysis Report

**Generated**: 2026-01-16  
**Project**: CoreAPI

---

## Summary

| Category | Count | Action |
|----------|-------|--------|
| Potentially Unused | 2 | Review for removal |
| Partially Used | 2 | Refactor or extend usage |
| Interface Mismatch | 1 | Align naming |

---

## 🔴 Unused/Useless Components

### 1. `IRepository<T>` Interface (Partially Unused)

**File**: `Repositories/Interfaces/IRepository.cs`

**Status**: Interface exists but **no concrete implementation directly injects it**.

**Details**:
- Extended by `IUserRepository`, `ITenantRepository`, `ICustomerRepository`
- However, the DI container registers concrete repositories directly (e.g., `IUserRepository → UserRepository`)
- The generic `IRepository<T>` is never injected anywhere

**Recommendation**: Remove if not needed for polymorphic repository access, or leverage it for a proper generic repository pattern.

---

### 2. Abstract `Repository<TEntity>` Base Class (Underutilized)

**File**: `Repositories/Repository.cs`

**Status**: Only **2 out of 8 repositories** inherit from this base class.

| Repository | Inherits `Repository<T>`? |
|------------|---------------------------|
| `TenantRepository` | ✅ Yes |
| `CustomerRepository` | ✅ Yes |
| `AccountRepository` | ❌ No |
| `UserRepository` | ❌ No |
| `TransactionRepository` | ❌ No |
| `TransactionTypeRepository` | ❌ No |
| `AccountTypeRepository` | ❌ No |

**Recommendation**: Either extend base class usage to all repositories for consistency, or remove it and inline the logic.

---

## 🟡 Interface/Implementation Naming Mismatch

### 3. `IUserService` vs `AuthService`

**Files**: 
- `Services/Interfaces/IUserService.cs`
- `Services/AuthService.cs`

**DI Registration** (line 52):
```csharp
builder.Services.AddScoped<IUserService, AuthService>();
```

**Issue**: The interface is named `IUserService` but the implementation is `AuthService`. This creates confusion about the service's purpose.

**Recommendation**: Rename either:
- `IUserService` → `IAuthService`, or
- `AuthService` → `UserService`

---

## 🟢 Confirmed In-Use Components

All the following are properly registered and consumed:

### Services
| Service | Registered | Used In |
|---------|------------|---------|
| `IAccountService` | ✅ | `TenantsController` |
| `ICustomerService` | ✅ | `CustomersController`, `TenantsController` |
| `ITenantService` | ✅ | `TenantsController` |
| `ITransactionService` | ✅ | `TenantsController`, `CustomersController`, `AccountService` |
| `ITransactionTypeService` | ✅ | `TenantsController`, `TransactionService` |
| `ITokenService` | ✅ | `AuthService` |
| `IRoleService` | ✅ | `RolesController` |
| `ICurrentUserProvider` | ✅ | Multiple services/repositories |

### Repositories
| Repository | Registered | Used Via |
|------------|------------|----------|
| `IAccountRepository` | ✅ | `UnitOfWork` |
| `IAccountTypeRepository` | ✅ | `UnitOfWork` |
| `ICustomerRepository` | ✅ | `UnitOfWork` |
| `ITenantRepository` | ✅ | `UnitOfWork` |
| `ITransactionRepository` | ✅ | `UnitOfWork` |
| `ITransactionTypeRepository` | ✅ | `UnitOfWork` |
| `IUserRepository` | ✅ | `UnitOfWork` |

### Mapper Profiles
All 6 profiles are active via AutoMapper assembly scanning:
- `AccountProfile` ✅
- `CustomerProfile` ✅
- `IdentityProfile` ✅
- `TenantProfile` ✅
- `TransactionProfile` ✅
- `TransactionTypeProfile` ✅

---

## Recommendations

1. **Remove or refactor** `IRepository<T>` and `Repository<T>` for consistency
2. **Rename** `IUserService` ↔ `AuthService` to match
3. **Audit** any controllers/services that import but don't use injected dependencies

---

*End of Report*
