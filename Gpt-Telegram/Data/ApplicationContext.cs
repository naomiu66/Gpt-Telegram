using Gpt_Telegram.Data.Configurations;
using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Models.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Gpt_Telegram.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessagesConfiguration());
            modelBuilder.ApplyConfiguration(new ChatSessionsConfiguration());

            base.OnModelCreating(modelBuilder);
        }

    }
}
