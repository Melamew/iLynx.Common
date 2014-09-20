using System;

namespace iLynx.Common.WPF.Controls
{
    public interface IConnectable
    {
        void Connect(IConnectable to);
        void Disconnect(IConnectable item);
        event EventHandler<ConnectedEventArgs> Connected;
        event EventHandler<ConnectedEventArgs> Disconnected;
    }
}