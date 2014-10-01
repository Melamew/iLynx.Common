using iLynx.Common.WPF;

namespace iLynx.Chatter.WPF
{
    public class  ContainerItemsViewModel : ItemsViewModel<ContainerViewModel>
    {
        public ContainerItemsViewModel(IDispatcher dispatcher) : base(dispatcher)
        {
        }
    }
}