using Sabanda.Application.Common.Interfaces;

namespace Sabanda.Infrastructure.Services;

public class CodeGenerator : ICodeGenerator
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;

    public CodeGenerator(IFamilyRepository familyRepository, IMemberRepository memberRepository)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
    }

    public async Task<string> GenerateFamilyCodeAsync(Guid tenantId)
    {
        const string prefix = "F";
        return await GenerateUniqueCodeAsync(tenantId, prefix, _familyRepository.ExistsByCodeAsync);
    }

    public async Task<string> GenerateMemberCodeAsync(Guid tenantId)
    {
        const string prefix = "M";
        return await GenerateUniqueCodeAsync(tenantId, prefix, _memberRepository.ExistsByCodeAsync);
    }

    private async Task<string> GenerateUniqueCodeAsync(Guid tenantId, string prefix, Func<Guid, string, Task<bool>> existsFunc)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;
        do
        {
            var suffix = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            code = prefix + suffix;
        } while (await existsFunc(tenantId, code));
        return code;
    }
}