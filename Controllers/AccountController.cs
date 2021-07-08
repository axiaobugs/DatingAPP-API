using System.Linq;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Interfaces;

namespace DatingApp.Controllers
{
    public class AccountController:BaseApiController
    {
        public readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context,ITokenService tokenService,IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(registerDTO);
            user.UserName = registerDTO.Username.ToLower();

            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Include(p=>p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDTO.Username);

            if (user == null) return Unauthorized("Invalid username");

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(p=>p.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());


        }
    }
}
