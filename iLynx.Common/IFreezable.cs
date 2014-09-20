namespace iLynx.Common
{
    public interface IFreezable
    {
        void Freeze();
        bool IsFrozen { get; }
        void UnFreeze();
    }
}