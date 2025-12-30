using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetsRepository(
    ILogger<MediaAssetsRepository> logger,
    FilesServiceDbContext dbContext): IMediaAssetsRepository
{
    public async Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        dbContext.MediaAssets.Add(mediaAsset);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return mediaAsset.Id;
        }
        catch (DbUpdateException ex) when(ex.InnerException is PostgresException pgEx)
        {
            logger.LogError(ex, "Database update error while creating media asset");

            return GeneralErrors.Failure();

        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while creating media asset");

            return GeneralErrors.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating media asset");

            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<MediaAsset, Error>> GetByAsync(
        Expression<Func<MediaAsset, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        MediaAsset? mediaAsset = await dbContext.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);

        if (mediaAsset is null)
            return GeneralErrors.NotFound(null, "media file");

        return mediaAsset;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}