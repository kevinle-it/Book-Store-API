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
using BookStoreAPI.Dto;
using System.Net;
using BookStoreAPI.Helpers;

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

        public UserController(ILogger<UserController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration config)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.config = config;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto model)
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
                        Response.Headers.Add("Authorization", JWTHelper.generateToken(user, this.config));
                        return Ok(new
                        {
                            fullName = user.FullName,
                            email = user.Email,
                            phoneNumber = user.PhoneNumber,
                            address = user.Address,
                            city = user.City,
                            state = user.State,
                            postalCode = user.PostalCode
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
        public async Task<IActionResult> SignIn([FromBody] SignInDto model)
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
                        return Ok(new
                        {
                            fullName = user.FullName,
                            email = user.Email,
                            phoneNumber = user.PhoneNumber,
                            address = user.Address,
                            city = user.City,
                            state = user.State,
                            postalCode = user.PostalCode
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
    }
}
