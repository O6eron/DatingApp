using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include( r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select( u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if(user == null) return NotFound("Could not find a user");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded) return BadRequest("Failed to add the roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<PagedList<PhotoDto>>> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync();

            Response.AddPaginationHeader(photos.CurrentPage, photos.PageSize, photos.TotalCount, photos.TotalPages);

            return photos;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult<PagedList<PhotoDto>>> ApprovePhoto(int photoId)
        {
            var photoToApprove = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photoToApprove == null) return NotFound("Photo not found");

            photoToApprove.IsApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(photoToApprove.AppUserId);

            photoToApprove.IsMain = !user.Photos.Any(p => p.IsMain);

            if (!(await _unitOfWork.Complete())) return BadRequest("Failed to approve photo");

            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync();

            Response.AddPaginationHeader(photos.CurrentPage, photos.PageSize, photos.TotalCount, photos.TotalPages);

            return photos;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult<PagedList<PhotoDto>>> RejectPhoto(int photoId)
        {
            var photoToRemove = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photoToRemove == null) return NotFound("Photo not found");

            if (photoToRemove.IsMain) return BadRequest("Cannot remove main photo");

            _unitOfWork.PhotoRepository.RemovePhoto(photoToRemove);

            if (!(await _unitOfWork.Complete())) return BadRequest("Failed to remove photo");

            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync();

            Response.AddPaginationHeader(photos.CurrentPage, photos.PageSize, photos.TotalCount, photos.TotalPages);

            return photos;
        }
    }
}