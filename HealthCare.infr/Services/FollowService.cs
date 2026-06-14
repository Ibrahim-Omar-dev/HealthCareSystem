
using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infreastructure.Services
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;

        public FollowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> SendRequestByEmailAsync(
            Guid senderId, string receiverEmail)
        {
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == receiverEmail.Trim().ToLower());

            if (receiver is null)
                return (false, "No user found with this email address.");

            if (receiver.Id == senderId)
                return (false, "You cannot send a follow request to yourself.");

            var existing = await _context.FollowRequests
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == receiver.Id);

            if (existing is not null)
            {
                return existing.Status switch
                {
                    FollowRequestStatus.Pending => (false, "You already sent a request to this user. Waiting for their response."),
                    FollowRequestStatus.Accepted => (false, "You are already following this user."),
                    FollowRequestStatus.Rejected => (false, "Your previous request was rejected by this user."),
                    _ => (false, "A request already exists.")
                };
            }

            var request = new FollowRequest
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                Status = FollowRequestStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            _context.FollowRequests.Add(request);
            await _context.SaveChangesAsync();

            return (true, $"Follow request sent to {receiver.Email}. Waiting for their approval.");
        }

        /// <summary>
        /// Remove a follow relationship (unfollow someone or remove a follower).
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveFollowAsync(Guid senderId, Guid receiverId)
        {
            var request = await _context.FollowRequests
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                    (f.SenderId == receiverId && f.ReceiverId == senderId));

            if (request is null)
                return (false, "No follow relationship found.");

            _context.FollowRequests.Remove(request);
            await _context.SaveChangesAsync();

            return (true, "Follow relationship removed successfully.");
        }

        public async Task<IEnumerable<FollowRequestResponseDto>> GetSentRequestsAsync(Guid senderId)
        {
            var requests = await _context.FollowRequests
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Where(f => f.SenderId == senderId)
                .OrderByDescending(f => f.SentAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }
        public async Task<IEnumerable<FollowerDto>> GetWhoIFollowAsync(Guid senderId)
        {
            var accepted = await _context.FollowRequests
                .Include(f => f.Receiver)
                .Where(f => f.SenderId == senderId && f.Status == FollowRequestStatus.Accepted)
                .OrderByDescending(f => f.RespondedAt)
                .ToListAsync();

            return accepted.Select(f => new FollowerDto
            {
                UserId = f.ReceiverId,
                DisplayName = f.Receiver!.DisplayName,
                Email = f.Receiver!.Email!,
                FollowingSince = f.RespondedAt!.Value
            });
        }
        public async Task<(bool Success, string Message)> AcceptRequestAsync(Guid requestId, Guid receiverId)
        {
            var request = await _context.FollowRequests
                .FirstOrDefaultAsync(f => f.Id == requestId && f.ReceiverId == receiverId);

            if (request is null)
                return (false, "Request not found.");

            if (request.Status != FollowRequestStatus.Pending)
                return (false, "This request has already been responded to.");

            request.Status = FollowRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Request accepted. This user can now view your health data.");
        }
        public async Task<(bool Success, string Message)> RejectRequestAsync(Guid requestId, Guid receiverId)
        {
            var request = await _context.FollowRequests
                .FirstOrDefaultAsync(f => f.Id == requestId && f.ReceiverId == receiverId);

            if (request is null)
                return (false, "Request not found.");

            if (request.Status != FollowRequestStatus.Pending)
                return (false, "This request has already been responded to.");

            request.Status = FollowRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Request rejected.");
        }

        public async Task<IEnumerable<FollowRequestResponseDto>> GetMyPendingRequestsAsync(Guid receiverId)
        {
            var requests = await _context.FollowRequests
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Where(f => f.ReceiverId == receiverId && f.Status == FollowRequestStatus.Pending)
                .OrderByDescending(f => f.SentAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }
        public async Task<IEnumerable<FollowerDto>> GetMyFollowersAsync(Guid receiverId)
        {
            var followers = await _context.FollowRequests
                .Include(f => f.Sender)
                .Where(f => f.ReceiverId == receiverId && f.Status == FollowRequestStatus.Accepted)
                .OrderByDescending(f => f.RespondedAt)
                .ToListAsync();

            return followers.Select(f => new FollowerDto
            {
                UserId = f.SenderId,
                DisplayName = f.Sender!.DisplayName,
                Email = f.Sender!.Email!,
                FollowingSince = f.RespondedAt!.Value
            });
        }

        public async Task<bool> IsFollowingAsync(Guid senderId, Guid receiverId)
        {
            return await _context.FollowRequests
                .AnyAsync(f =>
                    f.SenderId == senderId &&
                    f.ReceiverId == receiverId &&
                    f.Status == FollowRequestStatus.Accepted);
        }


        private static FollowRequestResponseDto MapToDto(FollowRequest f) => new()
        {
            Id = f.Id,
            SenderId = f.SenderId,
            SenderName = f.Sender?.DisplayName ?? string.Empty,
            SenderEmail = f.Sender?.Email ?? string.Empty,
            ReceiverEmail= f.Receiver?.Email ?? string.Empty,
            ReceiverId = f.ReceiverId,
            ReceiverName = f.Receiver?.DisplayName ?? string.Empty,
            Status = f.Status.ToString(),
            SentAt = f.SentAt,
            RespondedAt = f.RespondedAt
        };
    }
}