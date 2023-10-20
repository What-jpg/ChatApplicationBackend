using ChatApplicationApi.Contexts;
using ChatApplicationApi.DBModels;
using ChatApplicationApi.ModelClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ChatApplicationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatApplicationDataContext dbContext;

        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger, ChatApplicationDataContext dbContext)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        [Authorize]
        [HttpGet(Name = "SearchForUser")]
        [Route("searchforuser/{searchString}")]
        public ActionResult<string> SearchForUserGet(string searchString)
        {
            Auth0UserInfo userFromAuth0 = JsonConvert.DeserializeObject<Auth0UserInfo>(HttpContext.Request.Headers["User"]);
            var users = dbContext.Users;

            var matchingUsers = users.Where(u => (u.Name.Contains(searchString) || u.Id.Contains(searchString)) && u.Id != userFromAuth0.Sub)
                .Take(6)
                .Select(u => new { u.Id, u.Name })
                .ToArray();

            return Ok(JsonConvert.SerializeObject(matchingUsers));
        }

        [HttpGet(Name = "getUser")]
        [Route("getuser/{id}")]
        public ActionResult<string> GetUserGet(string id)
        {
            var users = dbContext.Users;

            User? userFromDb = users.Where(u => u.Id.Contains(id)).FirstOrDefault();

            if (userFromDb == null)
            {
                return BadRequest("Invalid id no such a user found");
            } else
            {
                return Ok(JsonConvert.SerializeObject(userFromDb));
            }
        }

        [Authorize]
        [HttpGet(Name = "getChatBetween2")]
        [Route("getchatbetween2/{otherUserId}")]
        public ActionResult<string?> GetChatBetween2Get(string otherUserId)
        {
            var users = dbContext.Users;
            var chats = dbContext.Chats;

            Auth0UserInfo userFromAuth0 = JsonConvert.DeserializeObject<Auth0UserInfo>(HttpContext.Request.Headers["User"]);

            Chat? chat = chats.Include((c) => c.Users).Where(
                        (c) => c.Users.Any((u) => u.Id == otherUserId) && c.Users.Any((u) => u.Id == userFromAuth0.Sub) && c.Users.Count == 2
                    ).FirstOrDefault();

            if (chat == null)
            {
                return Ok(null);
            }

            chat.Users.ForEach((user) => user.Chats = null);

            return Ok(JsonConvert.SerializeObject(chat));
        }

        [Authorize]
        [HttpGet(Name = "getMessages")]
        [Route("getmessages/{chatId}")]
        public ActionResult<string> GetMessagesGet(long chatId)
        {
            var messages = dbContext.Messages;
            var chats = dbContext.Chats;

            Auth0UserInfo thisUser = JsonConvert.DeserializeObject<Auth0UserInfo>(HttpContext.Request.Headers["User"]);

            Console.WriteLine("cock");
            var chat = chats.Include((chat) => chat.Users).Where((c) => c.Id == chatId).First();
            Console.WriteLine(chat.Users[0].Id);
            Console.WriteLine(chat.Users[1].Id);
            Console.WriteLine(thisUser.Sub);

            if (chats
                .Include((chat) => chat.Users)
                .Any((chat) => chat.Id == chatId && chat.Users.Any((user) => user.Id == thisUser.Sub))
                ) {
                var chatMessages = messages.Where((m) => m.ChatId == chatId).ToArray();
                ClientMessage[] clientMessages = new ClientMessage[chatMessages.Length];

            for (int i = 0; i < chatMessages.Length; i++)
            {
                    var item = chatMessages[i];

                clientMessages[i] = new ClientMessage { Content = /*Encoding.UTF8.GetString*/Convert.ToBase64String(item.Content), Type = item.Type, ChatId = item.ChatId, UserIdWhoSend = item.UserIdWhoSend, DateTime = item.DateTime };
            }

                return Ok(JsonConvert.SerializeObject(clientMessages));
            } else
            {
                return BadRequest("Invalid chatId");
            }
        }

        [Authorize]
        [HttpGet(Name = "getChats")]
        [Route("getchats")]
        public ActionResult<string> GetChatsGet()
        {
            var users = dbContext.Users;

            Auth0UserInfo thisUser = JsonConvert.DeserializeObject<Auth0UserInfo>(HttpContext.Request.Headers["User"]);

            User? thisUserFromDb = users.Where((user) => user.Id == thisUser.Sub).Include((user) => user.Chats).ThenInclude((chat) => chat.Users).FirstOrDefault();

            if (thisUserFromDb != null)
            {
                var thisUserChats = thisUserFromDb.Chats;

                foreach (var item in thisUserChats.First().Users)
                {
                    Console.WriteLine("User: " + item.Name);
                }

                /*foreach (var item in thisUserChats)
                {
                    if (!item.Users.Any((user) => user.Id == thisUserFromDb.Id))
                    {
                        item.Users.Add(thisUserFromDb);
                    }
                }*/

                thisUserChats.ForEach((chat) => chat.Users.ForEach((user) => user.Chats = null));

                return Ok(JsonConvert.SerializeObject(thisUserChats));
            } else
            {
                return BadRequest("User is invalid");
            }
        }
    }
}
