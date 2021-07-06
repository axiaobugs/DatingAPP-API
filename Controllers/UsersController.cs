﻿using System;
using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Helpers;

namespace DatingApp.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;


        public UsersController(IUserRepository userRepository, IMapper mapper,IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            
            Console.WriteLine(userParams.ToString());
            var users = await _userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDto>> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
                return NotFound();
            
            return Ok(_mapper.Map<MemberDto>(user));
        }

        [HttpGet("{username}",Name = "GetUserByName")]
        public async Task<ActionResult<MemberDto>> GetUserByName(string username)
        {
            return Ok(await _userRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());
            _mapper.Map(memberUpdateDto, user);
            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Fail to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
                return BadRequest();
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count==0)
                photo.IsMain = true;
            
            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
                return CreatedAtRoute("GetUserByName",new { username = user.UserName} ,_mapper.Map<PhotoDto>(photo));
            
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo.IsMain) return BadRequest("This is already your main photo");
            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain!=null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Fail to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
                
            }
            user.Photos.Remove(photo);
            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to deleted the photo");
        }


    }
}