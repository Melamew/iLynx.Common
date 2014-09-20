using System.Net;

namespace iLynx.Networking.Interfaces
{
    public interface IEndPointWrapper : IEndPointDescriptor
    {
        EndPoint EndPoint { get; }
    }
}