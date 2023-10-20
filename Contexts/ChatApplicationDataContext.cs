using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ChatApplicationApi.DBModels;

namespace ChatApplicationApi.Contexts
{
    public class ChatApplicationDataContext : DbContext
    {
        public ChatApplicationDataContext(DbContextOptions<ChatApplicationDataContext> options): base(options)
        { 
        
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<User>()
                .HasMany(user => user.Chats)
                .WithMany(chat => chat.Users);

            modelBuilder.Entity<Message>()
                .HasOne(message => message.UserWhoSend)
                .WithMany()
                .HasForeignKey(message => message.UserIdWhoSend);

            modelBuilder.Entity<Message>()
                .HasOne(message => message.Chat)
                .WithMany()
                .HasForeignKey(message => message.ChatId);
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
