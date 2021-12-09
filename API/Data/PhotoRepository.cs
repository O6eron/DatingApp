using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Data
{
  public class PhotoRepository : IPhotoRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PhotoRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<Photo> GetPhotoById(int photoId)
    {
      return await _context.Photos.FindAsync(photoId);
    }

    public async Task<PagedList<PhotoDto>> GetUnapprovedPhotosAsync()
    {
      var query = _context.Photos.Where(p => !p.IsApproved);

      var paginationParams = new PaginationParams();

      var photos = query.Select( photo => new PhotoDto
      {
          Id = photo.Id,
          Url = photo.Url,
          IsMain = photo.IsMain,
          IsApproved = photo.IsApproved,
          KnownAs = photo.AppUser.KnownAs
      });

      return await PagedList<PhotoDto>.CreateAsync(photos, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public void RemovePhoto(Photo photo)
    {
      _context.Photos.Remove(photo);
    }
  }
}