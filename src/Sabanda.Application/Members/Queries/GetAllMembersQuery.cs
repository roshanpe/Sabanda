using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Members.Commands;
using Sabanda.Application.Members.DTOs;

namespace Sabanda.Application.Members.Queries;

public class GetAllMembersQueryHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentTenantService _tenant;

    public GetAllMembersQueryHandler(
        IMemberRepository memberRepository,
        ICurrentTenantService tenant)
    {
        _memberRepository = memberRepository;
        _tenant = tenant;
    }

    public async Task<List<MemberResponse>> HandleAsync()
    {
        var members = await _memberRepository.GetAllByTenantAsync(_tenant.TenantId);
        return members.Select(m => CreateMemberCommandHandler.ToResponse(m)).ToList();
    }
}
