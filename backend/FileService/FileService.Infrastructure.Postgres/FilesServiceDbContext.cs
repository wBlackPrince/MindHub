using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres;

public class FilesServiceDbContext: DbContext
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
}