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
using Microsoft.EntityFrameworkCore;

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

    public async Task<PhotoDto[]> GetUnapprovedPhotosAsync()
    {
      var query = _context.Photos.Include(x => x.AppUser).Where(p => !p.IsApproved);

      var photos = query.Select( photo => new PhotoDto
      {
          Id = photo.Id,
          Url = photo.Url,
          IsMain = photo.IsMain,
          IsApproved = photo.IsApproved,
          KnownAs = photo.AppUser.KnownAs
      });


      return _mapper.Map<PhotoDto[]>(await query.ToListAsync());
    }

    public void RemovePhoto(Photo photo)
    {
      _context.Photos.Remove(photo);
    }
  }
}