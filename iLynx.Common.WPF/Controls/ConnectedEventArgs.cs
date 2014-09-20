using System;

namespace iLynx.Common.WPF.Controls
{
    public class ConnectedEventArgs : EventArgs
    {
        public IConnectable Other { get; set; }

        public ConnectedEventArgs()
        {

        }

        public ConnectedEventArgs(IConnectable other)
        {
            Other = other;
        }
    }
}