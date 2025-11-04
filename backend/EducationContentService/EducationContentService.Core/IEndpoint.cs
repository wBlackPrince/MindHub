using Microsoft.AspNetCore.Routing;

namespace EducationContentService.Core;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}