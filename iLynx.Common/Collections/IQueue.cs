namespace iLynx.Common.Collections
{
    public interface IQueue<TQueue>
    {
        TQueue Dequeue();
        void Enqueue(TQueue value);
        int Count { get; }
    }
}