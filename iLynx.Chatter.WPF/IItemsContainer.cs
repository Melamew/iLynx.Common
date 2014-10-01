namespace iLynx.Chatter.WPF
{
    public interface IItemsContainer<TItem>
    {
        void AddItem(TItem item);
        void RemoveItem(TItem item);
        TItem SelectedItem { get; set; }
        void ClearItems();
    }
}