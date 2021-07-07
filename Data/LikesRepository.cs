using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data
{
    public class LikesRepository:ILikesRepository
    {
        private readonly DataContext _contex;

        public LikesRepository(DataContext contex)
        {
            _contex = contex;
        }


        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _contex.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _contex.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _contex.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _contex.Likes.AsQueryable();

            if (likesParams.Predicate=="liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers =   users.Select(user => new LikeDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p=>p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }
    }
}
