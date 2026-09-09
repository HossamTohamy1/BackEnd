using loxxking_backend_clean.Domain.Entities.Support;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Support;

public class SupportConversationConfiguration : IEntityTypeConfiguration<SupportConversation>
{
    public void Configure(EntityTypeBuilder<SupportConversation> builder)
    {
        builder.Metadata.FindNavigation(nameof(SupportConversation.Messages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId);
    }
}
