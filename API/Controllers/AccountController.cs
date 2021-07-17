using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Infrastructure.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        //initializes JWT Token service
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly EmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            TokenService tokenService, IConfiguration config, EmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _config = config;
            _emailSender = emailSender;
            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri("https://graph.facebook.com")
            };
        }

        [AllowAnonymous]    //allows endpoints in Account controller in order to allow user to login :)
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            //cannot eagerly load photos using .FindByEmailAsync(). Need to use .Include()
            var user = await _userManager.Users.Include(p => p. Photos)
                .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null) return Unauthorized("Invalid email");
            
            //validate test user "bob" since in development we cannot send email confirmations
            if (user.UserName == "bob") user.EmailConfirmed = true;

            if (!user.EmailConfirmed) return Unauthorized("Email not confirmed yet.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                //setting the refreshToken
                await SetRefreshToken(user);
                return CreateUserObject(user);
            }

            return Unauthorized("Invalid password");
        }

        [AllowAnonymous]    //allows endpoints in Account controller in order to allow user to login :)
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                ModelState.AddModelError("email","Email already has a registered account");
                return ValidationProblem(ModelState);
            }
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username","Username already taken");
                return ValidationProblem(ModelState);
            }

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest("Probnlem registering user.");

            var origin = Request.Headers["origin"];
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //encoding token bc it gets back in HTML so need to protect it being modified en route to client
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";
            var message =
                $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";

            await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Registration success - please verify email");
        }

        [AllowAnonymous]
        [HttpPost("verifyEmail")]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();
            
            //token will be encoded when it's sent down to the client and we need to decode it
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded) return BadRequest("Could not verify email address.");

            return Ok("Email confirmed - now you can login");
        }

        [AllowAnonymous]
        [HttpGet("resendEmailConfirmationLink")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized();
            
            var origin = Request.Headers["origin"];
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //encoding token bc it gets back in HTML so need to protect it being modified en route to client
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";
            var message =
                $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email</a></p>";

            await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Email verification link resent.");
        }
        
        

        [Authorize] //need [Authorize] here because we have carved out exception for AccountController to [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            //cannot eagerly load photos using .FindByEmailAsync(). Need to use .Include()
            var user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));
            
            //setting the refreshToken - debatable whether it needs to be in the GetCurrentUser() method b/c sends every time user refreshes browser
            await SetRefreshToken(user);
            return CreateUserObject(user);
        }

        [AllowAnonymous]    //allows endpoints in Account controller in order to allow user to login :)
        [HttpPost("fbLogin")]
        public async Task<ActionResult<UserDTO>> FacebookLogin(string accessToken)
        {
            //need to verify accessToken is valid for our application
            var fbVerifyKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:AppSecret"];
            var verifyToken = await _httpClient
                .GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}");

            if (!verifyToken.IsSuccessStatusCode) return Unauthorized();

            var fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";

            var response = await _httpClient.GetAsync(fbUrl);

            if (!response.IsSuccessStatusCode) return Unauthorized();

            var fbInfo = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            var username = (string) fbInfo.id;

            var user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == username);

            if (user != null) return CreateUserObject(user);

            user = new AppUser
            {
                DisplayName = (string) fbInfo.name,
                Email = (string) fbInfo.email,
                UserName = (string) fbInfo.id,
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Id = "fb_" + (string)fbInfo.id,
                        Url = (string)fbInfo.picture.data.url,
                        IsMain = true

                    }
                }
            };
            //anyone who logged in with their Facebook account won't need to confirm their email since FB has already taken care of it.
            user.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded) return BadRequest("Problem creating Facebook user account.");

            //setting the refreshToken
            await SetRefreshToken(user);
            return CreateUserObject(user);
        }

        //Authorize prohibits renewing token if JWT token has expired
        [Authorize]
        [HttpPost("refreshToken")]
        public async Task<ActionResult<UserDTO>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var user = await _userManager.Users
                .Include(r => r.RefreshTokens)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == User.FindFirstValue(ClaimTypes.Name));

            if (user == null) return Unauthorized();

            var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

            if (oldToken != null && !oldToken.IsActive) return Unauthorized();

            //if (oldToken != null) oldToken.Revoked = DateTime.UtcNow;
            
            return CreateUserObject(user);
        }

        //done whenever user logins in or relogins they get a new refreshToken added to their account
        private async Task SetRefreshToken(AppUser user)
        {
            var refreshToken = _tokenService.GenerateRefreshToken();
            
            user.RefreshTokens.Add(refreshToken);
            
            //save token to database so when it comes time for user to refresh token they can compare new one against one that's stored in DB
            await _userManager.UpdateAsync(user);
            
            //pass token inside a cookie
            var cookieOptions = new CookieOptions
            {
                //refreshtoken not accessible via JavaScript
                HttpOnly = true,
                //when token saved in cookie, client sends token up to server w/ every request
                //token valid for 7 days
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
        
        //helper method for GetCurrentUser() method
        private UserDTO CreateUserObject(AppUser user)
        {
            return new UserDTO
            {
                DisplayName = user.DisplayName,
                //question marks added to keep us safe from getting in error in case no user or photo
                //eager loading of photos when want to return user and image
                Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }


    }
}