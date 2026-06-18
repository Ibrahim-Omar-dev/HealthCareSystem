using HealthCare.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCare.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : BaseApiController
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("SendRequest")]
        public async Task<IActionResult> SendRequest([FromBody] SendFollowRequestDto dto)
        {
            if (!TryGetCurrentUserId(out Guid senderId, out IActionResult? error))
                return error!;

            if (string.IsNullOrWhiteSpace(dto.ReceiverEmail))
                return BadRequest(new { success = false, message = "Email is required." });

            var (success, message) = await _followService.SendRequestByEmailAsync(senderId, dto.ReceiverEmail);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("Remove/{targetUserId:guid}")]
        public async Task<IActionResult> Remove(Guid targetUserId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _followService.RemoveFollowAsync(userId, targetUserId);

            return success
                ? Ok(new { success = true, message })
                : NotFound(new { success = false, message });
        }

        [HttpGet("SentRequests")]
        public async Task<IActionResult> SentRequests()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var requests = await _followService.GetSentRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpGet("WhoIFollow")]
        public async Task<IActionResult> WhoIFollow()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var following = await _followService.GetWhoIFollowAsync(userId);
            return Ok(following);
        }


        [HttpGet("MyRequests")]
        public async Task<IActionResult> MyRequests()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var requests = await _followService.GetMyPendingRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPut("Accept/{requestId:guid}")]
        public async Task<IActionResult> Accept(Guid requestId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _followService.AcceptRequestAsync(requestId, userId);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("Reject/{requestId:guid}")]
        public async Task<IActionResult> Reject(Guid requestId)
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var (success, message) = await _followService.RejectRequestAsync(requestId, userId);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpGet("MyFollowers")]
        public async Task<IActionResult> MyFollowers()
        {
            if (!TryGetCurrentUserId(out Guid userId, out IActionResult? error))
                return error!;

            var followers = await _followService.GetMyFollowersAsync(userId);
            return Ok(followers);
        }



        
        public record SendFollowRequestDto(string ReceiverEmail);
    }
}