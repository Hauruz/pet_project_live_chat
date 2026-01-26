using Chat.Application.DTOs;
using Chat.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("Unable to get user ID from claims");
    }

    [HttpPost("rooms")]
    public async Task<ActionResult<ChatRoomDto>> CreateChatRoom(CreateChatRoomRequest request)
    {
        try
        {
            var userId = GetUserId();
            var chatRoom = await _chatService.CreateChatRoomAsync(request, userId);
            return CreatedAtAction(nameof(GetChatRoom), new { id = chatRoom.Id }, chatRoom);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<List<ChatRoomDto>>> GetUserChatRooms()
    {
        try
        {
            var userId = GetUserId();
            var chatRooms = await _chatService.GetUserChatRoomsAsync(userId);
            return Ok(chatRooms);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("rooms/{id}")]
    public async Task<ActionResult<ChatRoomDto>> GetChatRoom(Guid id)
    {
        try
        {
            var chatRoom = await _chatService.GetChatRoomAsync(id);
            return Ok(chatRoom);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("rooms/{id}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetChatMessages(Guid id, [FromQuery] int limit = 50)
    {
        try
        {
            var messages = await _chatService.GetChatMessagesAsync(id, limit);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("rooms/{id}")]
    public async Task<ActionResult> DeleteChatRoom(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.DeleteChatRoomAsync(id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("rooms/{id}/invite")]
    public async Task<ActionResult> InviteUserToChatRoom(Guid id, [FromBody] InviteUserRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.AddUserToChatRoomAsync(id, request.Username, userId);
            return Ok(new { message = "User invited successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class InviteUserRequest
{
    public string Username { get; set; } = null!;
}
