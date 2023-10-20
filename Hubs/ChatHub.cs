using ChatApplicationApi.Contexts;
using ChatApplicationApi.DBModels;
using ChatApplicationApi.ModelClasses;
using ChatApplicationApi.RedisHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatApplicationApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatApplicationDataContext dbContext;
        private readonly RedisDBAccessor redisHelper;

        public ChatHub(ChatApplicationDataContext dbContext)
        {
            this.dbContext = dbContext;
            redisHelper = new RedisDBAccessor();

        }

        public override Task OnConnectedAsync()
        {
            Auth0UserInfo user = JsonConvert.DeserializeObject<Auth0UserInfo>(Context.GetHttpContext().Request.Headers["User"]);

            redisHelper.UserIdToWebsocketId.SetValue(user.Sub, Context.ConnectionId);
            redisHelper.WebsocketIdToUserId.SetValue(Context.ConnectionId, user.Sub);

            Console.WriteLine($"Adding websocket: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public async Task SendFirstMessageToUser(string userId, string messageContent, string messageType)
        {
            var users = dbContext.Users;
            var chats = dbContext.Chats;

            Auth0UserInfo user = JsonConvert.DeserializeObject<Auth0UserInfo>(Context.GetHttpContext().Request.Headers["User"]);

            if (user.Sub != userId)
            {
                //byte[] messageContentInBytes = Encoding.UTF8.GetBytes(messageContent);
                byte[] messageContentInBytes = Convert.FromBase64String(messageContent);

                if (messageType != "PlainText")
                {
                    messageType = "File.-." + messageType;
                }

                Auth0UserInfo userFromAuth0 = JsonConvert.DeserializeObject<Auth0UserInfo>(Context.GetHttpContext().Request.Headers["User"]);

                User? userSendTo = users.Where((u) => u.Id == userId).Include((u) => u.Chats).FirstOrDefault();
                User thisUserFromDb = users.Where((u) => u.Id == userFromAuth0.Sub).Include((u) => u.Chats).First();

                if (userSendTo != null && messageContent != "")
                {
                    Chat? chat = chats.Include((c) => c.Users).Where(
                            (c) => c.Users.Any((u) => u.Id == userSendTo.Id) && c.Users.Any((u) => u.Id == thisUserFromDb.Id) && c.Users.Count == 2
                        ).FirstOrDefault();

                    if (chat == null)
                    {
                        chat = new Chat { Users = new List<User> { userSendTo, thisUserFromDb } };

                        dbContext.Attach(userSendTo);
                        dbContext.Attach(thisUserFromDb);

                        userSendTo.Chats.Add(chat);
                        thisUserFromDb.Chats.Add(chat);

                        dbContext.Add(chat);

                        dbContext.SaveChanges();

                        Message message = new Message { Content = messageContentInBytes, Type = messageType, Chat = chat, ChatId = chat.Id, UserWhoSend = thisUserFromDb, UserIdWhoSend = thisUserFromDb.Id };
                        ClientMessage clientMessage = new ClientMessage { Content = messageContent, Type = message.Type, ChatId = message.ChatId, UserIdWhoSend = message.UserIdWhoSend, DateTime = message.DateTime };

                        dbContext.Add(message);
                        dbContext.SaveChanges();

                        chat.Users.ForEach((u) => u.Chats = null);

                        string chatString = JsonConvert.SerializeObject(chat);
                        string messageString = JsonConvert.SerializeObject(clientMessage);

                        await Clients.Caller.SendAsync("RecieveMessageForFirstTime", chatString, messageString);

                        string? otherClientWebsocketId = redisHelper.UserIdToWebsocketId.GetValue(userSendTo.Id);

                        if (otherClientWebsocketId != null)
                        {
                            await Clients.Client(otherClientWebsocketId).SendAsync("RecieveMessageForFirstTime", chatString, messageString);
                        }
                    }
                }
            }
        }

        public async Task SendMessage(long chatId, string messageContent, string messageType)
        {
            var chats = dbContext.Chats;
            var users = dbContext.Users;
            // byte[] messageContentInBytes = Encoding.UTF8.GetBytes(messageContent);
            byte[] messageContentInBytes = Convert.FromBase64String(messageContent);

            Console.WriteLine("Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh Consul loh");

            if (messageType != "PlainText")
            {
                messageType = "File.-." + messageType;
            }

            Chat? chat = chats.Where((c) => c.Id == chatId).Include((c) => c.Users).FirstOrDefault();

            Auth0UserInfo auth0User = JsonConvert.DeserializeObject<Auth0UserInfo>(Context.GetHttpContext().Request.Headers["User"]);

            User thisUserFromDb = users.Where((u) => u.Id == auth0User.Sub).First();

            if (chat != null && chat.Users.Any((u) => u.Id == auth0User.Sub))
            {
                Message message = new Message { Content = messageContentInBytes, Type = messageType/*, Chat = chat*/, ChatId = chat.Id/*, UserWhoSend = thisUserFromDb*/, UserIdWhoSend = thisUserFromDb.Id };
                ClientMessage clientMessage = new ClientMessage { Content = messageContent, Type = message.Type, ChatId = message.ChatId, UserIdWhoSend = message.UserIdWhoSend, DateTime = message.DateTime };

                Console.WriteLine("message.Content: " + message.Content);

                dbContext.Add(message);
                dbContext.SaveChanges();

                message.Chat = chat;
                message.UserWhoSend = thisUserFromDb;

                chat.Users.ForEach((u) => u.Chats = null);

                string chatString = JsonConvert.SerializeObject(chat);
                string messageString = JsonConvert.SerializeObject(clientMessage);

                await Clients.Caller.SendAsync("RecieveMessage", chatString, messageString);

                foreach (var user in chat.Users)
                {
                    if (user.Id != thisUserFromDb.Id)
                    {
                        string? otherClientWebsocketId = redisHelper.UserIdToWebsocketId.GetValue(user.Id);

                        if (otherClientWebsocketId != null)
                        {
                            await Clients.Client(otherClientWebsocketId).SendAsync("RecieveMessage", chatString, messageString);
                        }
                    }
                }
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var redisHelper = new RedisDBAccessor();

            string userId = redisHelper.WebsocketIdToUserId.GetValue(Context.ConnectionId);

            redisHelper.UserIdToWebsocketId.DeleteValue(userId);
            redisHelper.WebsocketIdToUserId.DeleteValue(Context.ConnectionId);

            Console.WriteLine($"Deleting user: {userId}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
