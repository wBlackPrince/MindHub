using FileService.Core;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FilesServiceDbContext: DbContext, IReadDbContext
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public IQueryable<MediaAsset> MediaAssetsQuery => MediaAssetsQuery.AsQueryable().AsNoTracking();
}