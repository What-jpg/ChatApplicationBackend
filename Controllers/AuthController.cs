using ChatApplicationApi.Components;
using ChatApplicationApi.Contexts;
using ChatApplicationApi.DBModels;
using ChatApplicationApi.ModelClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ChatApplicationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ChatApplicationDataContext dbContext;

        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, ChatApplicationDataContext dbContext)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost(Name = "GetOrSetUser")]
        [Authorize]
        [Route("setorgetuser")]
        public ActionResult<string> GetOrSetUserPost()
        {
            var auth0User = JsonConvert.DeserializeObject<Auth0UserInfo>(HttpContext.Request.Headers["User"]);

            Console.WriteLine(auth0User);
    
            var userInDbCheck = new UserInDbCheck(dbContext);
            User userToReturn = userInDbCheck.GetOrCreateUserInDb(auth0User);

            userToReturn.Chats.ForEach(
                (chat) => chat.Users.ForEach((user) => user.Chats = null)
            );

            string result = JsonConvert.SerializeObject(userInDbCheck.GetOrCreateUserInDb(auth0User));

            return Ok(result);
        }
    }
}
