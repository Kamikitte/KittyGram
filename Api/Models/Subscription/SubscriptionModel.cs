namespace Api.Models.Subscription;

public sealed class SubscriptionModel
{
    public Guid SubscriberId { get; set; }
    public Guid PublisherId { get; set; }
}