using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BookStoreAPI.Data.Entities;
using BookStoreAPI.Dto.User.Request;
using System.Net;
using BookStoreAPI.Helpers;
using BookStoreAPI.Dto.User.Response;
using BookStoreAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BookStoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IConfiguration config;
        private readonly BookStoreContext _context;

        public UserController(ILogger<UserController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration config,
            BookStoreContext context)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.config = config;
            this._context = context;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<UserResponse>> SignUp([FromBody] SignUpDto model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await this.userManager.FindByEmailAsync(model.Email);
                if (existingUser == null)
                {
                    User user = new User();
                    user.FullName = model.FullName;
                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.State = model.State;
                    user.PostalCode = model.PostalCode;

                    IdentityResult result = userManager.CreateAsync(user, model.Password).Result;

                    if (result.Succeeded)
                    {
                        try
                        {
                            CreateCartForCurrentUser(user.Id);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Create user cart failed!");
                        }

                        Response.Headers.Add("Authorization", JWTHelper.generateToken(user, this.config));
                        return Ok(new UserResponse
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Address = user.Address,
                            City = user.City,
                            State = user.State,
                            PostalCode = user.PostalCode,
                        });
                    }
                    else
                    {
                        string errors = "";
                        foreach (IdentityError e in result.Errors)
                        {
                            errors += $"{e.Description} ";
                        }
                        errors = errors.TrimEnd();
                        return Problem(detail: errors, statusCode: (int) HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Problem(detail: "User already exists.", statusCode: (int) HttpStatusCode.Conflict);
                }

            }

            return BadRequest();
        }

        [HttpPost("signin")]
        public async Task<ActionResult<UserResponse>> SignIn([FromBody] SignInDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var passwordCheck = await this.signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (passwordCheck.Succeeded)
                    {
                        Response.Headers.Add("Authorization", JWTHelper.generateToken(user, this.config));
                        return Ok(new UserResponse
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Address = user.Address,
                            City = user.City,
                            State = user.State,
                            PostalCode = user.PostalCode,
                        });
                    }
                    else
                    {
                        return Unauthorized("Your password is incorrect.");
                    }
                }
                else
                {
                    return NotFound("User not exist.");
                }
            }

            return BadRequest();
        }

        //[Authorize]
        //[HttpPost("signout")]
        //public async Task<ActionResult> LogOut()
        //{
        //    await this.signInManager.SignOutAsync();
        //    return Ok();
        //}

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UserDto model)
        {
            if (ModelState.IsValid)
            {
                var email = User.Identity.Name;
                var user = await this.userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.State = model.State;
                    user.PostalCode = model.PostalCode;

                    await this.userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    var phoneNumberToken = await this.userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                    IdentityResult changePhoneNumberResult = await this.userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, phoneNumberToken);

                    if (!changePhoneNumberResult.Succeeded)
                    {
                        logger.LogInformation("Cannot change phone number.");
                    }

                    var emailToken = await this.userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                    IdentityResult changeEmailResult = await this.userManager.ChangeEmailAsync(user, model.Email, emailToken);

                    if (changeEmailResult.Succeeded)
                    {
                        await this.userManager.SetUserNameAsync(user, model.Email);
                    }
                    else
                    {
                        logger.LogInformation("Cannot change email.");
                    }

                    _context.Entry(user).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
                    }

                    return Ok(new UserResponse
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        PostalCode = user.PostalCode,
                    });
                }
                else
                {
                    return NotFound("User not exist.");
                }

            }

            return BadRequest();
        }

        private async void CreateCartForCurrentUser(string userId)
        {
            try
            {
                var cart = await _context.Carts
                    .SingleOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    Cart cartToAdd = new()
                    {
                        UserId = userId,
                        TotalPrice = 0,
                    };
                    _context.Carts.Add(cartToAdd);
                    await _context.SaveChangesAsync();
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
