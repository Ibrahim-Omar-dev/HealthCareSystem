using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Domain.Interface
{
    public interface IFollowService
    {
        // ── Sender actions ────────────────────────────────────────────────────
        Task<(bool Success, string Message)> SendRequestByEmailAsync(Guid senderId, string receiverEmail);
        Task<(bool Success, string Message)> RemoveFollowAsync(Guid senderId, Guid receiverId);
        Task<IEnumerable<FollowRequestResponseDto>> GetSentRequestsAsync(Guid senderId);
        Task<IEnumerable<FollowerDto>> GetWhoIFollowAsync(Guid senderId);

        // ── Receiver actions ──────────────────────────────────────────────────
        Task<(bool Success, string Message)> AcceptRequestAsync(Guid requestId, Guid receiverId);
        Task<(bool Success, string Message)> RejectRequestAsync(Guid requestId, Guid receiverId);
        Task<IEnumerable<FollowRequestResponseDto>> GetMyPendingRequestsAsync(Guid receiverId);
        Task<IEnumerable<FollowerDto>> GetMyFollowersAsync(Guid receiverId);

        // ── Shared ────────────────────────────────────────────────────────────
        Task<bool> IsFollowingAsync(Guid senderId, Guid receiverId);
    }
}
