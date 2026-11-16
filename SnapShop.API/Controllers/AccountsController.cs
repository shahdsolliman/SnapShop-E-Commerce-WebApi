using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.API.Extensions;
using SnapShop.Core.Entities.Identity;
using SnapShop.Core.Services.Contract;
using System.Security.Claims;

namespace SnapShop.API.Controllers
{
    public class AccountsController : BaseAPIController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("login")]  // /api/Accounts/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401));
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
                return Unauthorized(new ApiResponse(401, "Account is temporarily locked out due to multiple failed login attempts. Please try again later."));
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user, _userManager)
            });
        }

        [HttpPost("register")] // POST: /api/Accounts/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest(new ApiValidationErrorResponse() { Errors = new[] { "This email address is already registered." } });

            var user = new AppUser()
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserName = registerDto.Email.Split("@")[0]
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new ApiValidationErrorResponse() { Errors = errors });
            }

            return StatusCode(StatusCodes.Status201Created, new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user, _userManager)
            });
        }


        [Authorize]
        [HttpGet] //GET: /api/Accounts
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user, _userManager)
            });
        }

        [Authorize]
        [HttpPut("address")] //PUT: /api/Accounts/address
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto updatedAddress)
        {
            var address = _mapper.Map<AddressDto, Address>(updatedAddress);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var appUser = await _userManager.FindByEmailAsync(email);

            appUser.Address = address;
            var result = await _userManager.UpdateAsync(appUser);
            if (! result.Succeeded)
                return BadRequest(new ApiResponse(400));
            return Ok(_mapper.Map<Address, AddressDto>(appUser.Address));

        }

        [Authorize]
        [HttpGet("address")] //GET: /api/Accounts/address
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            var appUser = await _userManager.FindUserWithAddressByEmailAsync(User);
            return Ok(_mapper.Map<Address, AddressDto>(appUser.Address));
        }

        [HttpGet("emailexists")] //GET: /api/Accounts/emailexists?email=xxx
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
