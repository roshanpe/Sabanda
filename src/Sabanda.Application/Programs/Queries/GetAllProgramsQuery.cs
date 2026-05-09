using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Programs.Commands;
using Sabanda.Application.Programs.DTOs;

namespace Sabanda.Application.Programs.Queries;

public class GetAllProgramsQueryHandler
{
    private readonly IProgramRepository _programRepository;
    private readonly ICurrentTenantService _tenant;

    public GetAllProgramsQueryHandler(
        IProgramRepository programRepository,
        ICurrentTenantService tenant)
    {
        _programRepository = programRepository;
        _tenant = tenant;
    }

    public async Task<List<ProgramResponse>> HandleAsync()
    {
        var programs = await _programRepository.GetAllByTenantAsync(_tenant.TenantId);
        return programs.Select(p => CreateProgramCommandHandler.ToResponse(p)).ToList();
    }
}