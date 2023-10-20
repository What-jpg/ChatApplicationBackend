using RestSharp;
using ChatApplicationApi.Contexts;
using Newtonsoft.Json;
using ChatApplicationApi.ModelClasses;
using ChatApplicationApi.DBModels;
using Microsoft.EntityFrameworkCore;

namespace ChatApplicationApi.Components
{
    public class UserInDbCheck
    {
        public UserInDbCheck (ChatApplicationDataContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ChatApplicationDataContext dbContext;

        public User GetOrCreateUserInDb(Auth0UserInfo auth0User)
        {
            User? user = dbContext.Users.Include(u => u.Chats).Where(u => u.Id == auth0User.Sub).FirstOrDefault();

            Console.WriteLine(JsonConvert.SerializeObject(auth0User));

            if (user == null)
            {
                user = new User { Picture = auth0User.Picture, Id = auth0User.Sub, Name = auth0User.Name };

                dbContext.Add(user);
                dbContext.SaveChanges();
            }

            return user;
        }
    }
}
