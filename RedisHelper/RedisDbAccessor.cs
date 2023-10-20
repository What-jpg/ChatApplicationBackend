using StackExchange.Redis;

namespace ChatApplicationApi.RedisHelper
{
    public class RedisDBAccessor
    {
        private static IDatabase db = ConnectionMultiplexer.Connect(WebApplication.CreateBuilder().Configuration.GetValue<string>("REDIS_CONNECTION_STRING")).GetDatabase();

        public KeyAccessor WebsocketIdToUserId = new KeyAccessor("websocketIdToUserId", db);
        public KeyAccessor UserIdToWebsocketId = new KeyAccessor("userIdToWebsocketId", db);
    }
}
