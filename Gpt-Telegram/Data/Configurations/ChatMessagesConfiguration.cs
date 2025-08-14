using Gpt_Telegram.Data.Models.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gpt_Telegram.Data.Configurations
{
    public class ChatMessagesConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Role)
                .IsRequired();

            builder.Property(cm => cm.Content)
                .IsRequired();

            builder.HasOne(cm => cm.Session)
                .WithMany(cs => cs.ChatMessages)
                .HasForeignKey(cm => cm.SessionId);

            builder.ToTable("chat_messages");
        }
    }
}
