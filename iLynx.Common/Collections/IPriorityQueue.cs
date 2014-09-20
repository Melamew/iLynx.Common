namespace iLynx.Common.Collections
{
    public interface IPriorityQueue<TQueue> : IQueue<PriorityItem<TQueue>>
    {
        void Enqueue(TQueue item,
            Priority priority = Priority.Normal);

        TQueue RawDequeue();
    }
}