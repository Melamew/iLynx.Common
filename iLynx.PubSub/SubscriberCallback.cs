namespace iLynx.PubSub
{
    public delegate void SubscriberCallback<in T>(T message);
}