using ListWish.DTOs.Request;
using ListWish.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ListWish.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController] 
    public class Account : ControllerBase
    {
        private readonly UserManager<ListUser> userManager;
        private readonly RoleManager<ListRole> roleManager;

        public Account(UserManager<ListUser> userManager, RoleManager<ListRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginRequestDTO req,
           [FromServices] IOptions<JWTOption> jwtOpt)
        {
            ListUser user = await userManager.FindByNameAsync(req.UserName);
            if (user is null)
            {
                return NotFound("User does not exist");
            }
            var match = await userManager.CheckPasswordAsync(user, req.PassWord);
            if (!match)
            {
                return BadRequest("Username or password incorrect");
            }
            var claims = new List<Claim>();
            var idClaim = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            claims.Add(idClaim);
            var roles = await userManager.GetRolesAsync(user);
            foreach(var role in roles)
            {
                var roleClaim = new Claim(ClaimTypes.Role, role);
                claims.Add(roleClaim);
            }
            string jwtToken = BuildToken(claims, jwtOpt.Value);
            return Ok(jwtToken);
        }

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterReqDTO  req)
        {
            if (!WC.Roles.Contains(req.role))
            {
                return BadRequest();
            }
            ListUser newUser = new ListUser() { UserName = req.UserName };
            var result =  await userManager.CreateAsync(newUser,req.PassWord);
            if (!result.Succeeded)
            {
                return BadRequest(result.ToString());
            }
            var exist = await roleManager.FindByNameAsync(req.role);
            if(exist is null)
            {
                await roleManager.CreateAsync(new ListRole() { Name = req.role });
            }
            var isAdded = await userManager.AddToRoleAsync(newUser, req.role);
            if (!isAdded.Succeeded)
            {
                return BadRequest();
            }
            return Ok();
        }

        private static string BuildToken(IEnumerable<Claim> claims, JWTOption opt)
        {
            DateTime expires = DateTime.Now.AddSeconds(opt.ExpireSeconds);
            byte[] keyBytes = Encoding.UTF8.GetBytes(opt.SigningKey);
            var secKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(expires: expires, signingCredentials: credentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
