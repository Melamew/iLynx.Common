namespace iLynx.Common.Collections
{
    public class PriorityItem<T>
    {
        public Priority Priority { get; private set; }
        public T Item { get; private set; }

        public PriorityItem(Priority priority,
            T item)
        {
            Priority = priority;
            Item = item;
        }
    }
}