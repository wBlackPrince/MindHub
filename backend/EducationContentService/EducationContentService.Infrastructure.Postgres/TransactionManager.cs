using EducationContentService.Core.Database;

namespace EducationContentService.Infrastructure.Postgres;

public class TransactionManager(EducationDbContext dbContext): ITransactionManager
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}