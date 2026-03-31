using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Common.Interfaces;

public interface IProgramEnrolmentRepository
{
    Task<ProgramEnrolment?> FindByIdAsync(Guid id);
    Task<bool> IsMemberEnrolledOrWaitlistedAsync(Guid programId, Guid memberId);
    Task<int> CountEnrolledAsync(Guid programId);
    Task<int> GetMaxWaitlistPositionAsync(Guid programId);
    Task<ProgramEnrolment?> GetFirstWaitlistedAsync(Guid programId);
    Task<List<ProgramEnrolment>> GetWaitlistedAfterPositionAsync(Guid programId, int afterPosition);
    Task AddAsync(ProgramEnrolment enrolment);
    Task SaveChangesAsync();
}
