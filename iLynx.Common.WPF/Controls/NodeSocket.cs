using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    public interface IConnectable
    {
        void Connect(IConnectable to);
        void Disconnect(IConnectable item);
        event EventHandler<ConnectedEventArgs> Connected;
        event EventHandler<ConnectedEventArgs> Disconnected;
    }

    public class NodeSocket : Control, IPositionable, IConnectable
    {
        public static readonly DependencyProperty GlobalPositionProperty = DependencyProperty.Register("GlobalPosition", typeof(Point), typeof(NodeSocket), new PropertyMetadata(default(Point)));

        private readonly List<Connector> connectors = new List<Connector>();
        private NodePanel rootPanel;

        public NodeSocket()
        {
            DefaultStyleKey = typeof(NodeSocket);
        }

        public bool IsConnected
        {
            get { return null != connectors && connectors.Any(); }
        }

        public Point Position
        {
            get { return (Point)GetValue(GlobalPositionProperty); }
            set { SetValue(GlobalPositionProperty, value); }
        }

        public Node Node { get; private set; }

        public event EventHandler PositionChanged;
        private Action<Connector> addConnectorCallback;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if (null != Node) Node.LayoutUpdated -= NodeOnLayoutUpdated;
            Node = this.FindVisualParent<Node>();
            rootPanel = this.FindVisualParent<NodePanel>();
            if (null == rootPanel) return;
            if (null == Node) return;
            addConnectorCallback = x => rootPanel.AddConnector(x);
            Node.LayoutUpdated += NodeOnLayoutUpdated;
            SetGlobalPosition();
        }

        private void NodeOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            SetGlobalPosition();
        }

        private void SetGlobalPosition()
        {
            if (null == rootPanel) return;
            SetValue(GlobalPositionProperty, TransformToAncestor(rootPanel).Transform(new Point(ActualWidth / 2d, ActualHeight / 2d)));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (GlobalPositionProperty == e.Property)
                OnGlobalPositionChanged();
        }

        protected virtual void OnGlobalPositionChanged()
        {
            var handler = PositionChanged;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        public SocketType SocketType
        {
            get { return null == Node ? SocketType.Unknown : Node.GetSocketType(this); }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton != MouseButton.Left) return;
            if (null == Node) return;
            if (null == addConnectorCallback) return;
            var type = SocketType;
            if (SocketType.Unknown == type) return;
            if (SocketType.Input == type)
                DragInputConnector(e);
            else
                DragOutputConnector(e);
            e.Handled = true;
        }

        private void DragInputConnector(MouseButtonEventArgs e)
        {
            if (connectors.Any())
                connectors.First().DragOutputEndPoint(e);
            else
            {
                var connector = new Connector();
                addConnectorCallback(connector);
                connectors.Add(connector);
                connector.ConnectOutputEndPoint(this);
                connector.DragInputEndPoint(e);
            }
        }

        private void DragOutputConnector(MouseButtonEventArgs e)
        {
            var connector = new Connector();
            addConnectorCallback(connector);
            connectors.Add(connector);
            connector.ConnectInputEndPoint(this);
            connector.DragOutputEndPoint(e);
        }

        #region Implementation of IConnectable

        public void Connect(IConnectable to)
        {
            var con = to as Connector;
            if (null == con) return;
            if (connectors.Contains(con))
            {
                OnConnected(to);
                return;
            }
            var type = SocketType;
            if (SocketType.Unknown == type) return;
            if (SocketType.Input == type)
                Disconnect();

            connectors.Add(con);
            con.Connect(this);
            OnConnected(to);
        }

        private void Disconnect()
        {
            foreach (var connector in connectors.ToArray())
                Disconnect(connector);
        }

        public void Disconnect(IConnectable item)
        {
            if (ReferenceEquals(item, Node))
            {
                DisconnectAll();
                if (null != Node)
                    Node.LayoutUpdated -= NodeOnLayoutUpdated;
            }
            else
            {
                var connector = connectors.FirstOrDefault(x => ReferenceEquals(item, x));
                if (null == connector) return;
                connectors.Remove(connector);
                connector.Disconnect(this);
            }
            OnDisconnected(item);
        }

        private void DisconnectAll()
        {
            foreach (var connector in connectors.ToArray())
                connector.Disconnect(this);
        }

        protected virtual void OnConnected(IConnectable to)
        {
            var handler = Connected;
            if (null == handler) return;
            handler(this, new ConnectedEventArgs(to));
        }

        protected virtual void OnDisconnected(IConnectable from)
        {
            var handler = Disconnected;
            if (null == handler) return;
            handler(this, new ConnectedEventArgs(from));
        }

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ConnectedEventArgs> Disconnected;

        #endregion

        public IEnumerable<Node> GetTargetNodes()
        {
            return connectors.Select(x => (SocketType == SocketType.Input ? x.InputNode : x.OutputNode));
        }
    }
}