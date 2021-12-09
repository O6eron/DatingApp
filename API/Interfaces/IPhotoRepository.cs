using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<PagedList<PhotoDto>> GetUnapprovedPhotosAsync();
        Task<Photo> GetPhotoById(int photoId);
        void RemovePhoto(Photo photo);
    }
}