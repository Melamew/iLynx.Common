namespace iLynx.Chatter.Infrastructure.Services
{
    public interface ITabRegionService
    {
        int RegisterView(object view, string header);
        void RemoveView(int view);
        void ActivateView(int view);
    }
}
