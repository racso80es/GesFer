using GesFer.Admin.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Application.Common.Interfaces;

public interface IAdminDbContext
{
    DbSet<AdminUser> AdminUsers { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Log> Logs { get; }
    DbSet<Company> Companies { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
