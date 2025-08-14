using Gpt_Telegram.Data.Models.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gpt_Telegram.Data.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username).HasMaxLength(64);

            builder.Property(u => u.FirstName).HasMaxLength(64);

            builder.Property(u => u.LastName).HasMaxLength(64);

            builder.HasMany(u => u.Sessions)
                .WithOne(cs => cs.User)
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.ActiveSession)
                .WithMany()
                .HasForeignKey(u => u.ActiveSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable("users");
        }
    }
}
