using Gpt_Telegram.Data.Models.OpenAI;
using Gpt_Telegram.Data.Models.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gpt_Telegram.Data.Configurations
{
    public class ChatSessionsConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Title)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(cs => cs.SystemPrompt)
                .HasMaxLength(2048);

            builder.HasMany(cs => cs.ChatMessages)
                .WithOne(cm => cm.Session)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.User)
                .WithMany(cm => cm.Sessions)
                .HasForeignKey(cm => cm.UserId) 
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("chat_sessions");
        }
    }
}
