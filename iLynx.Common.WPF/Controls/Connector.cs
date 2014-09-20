using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace iLynx.Common.WPF.Controls
{
    public class Connector : Control, IConnectable
    {
        public Connector()
        {
            DefaultStyleKey = typeof(Connector);
        }

        private NodePanel root;
        private const double SnapDistance = 10d;
        private Point dragInit;
        private enum DragEndPoint
        {
            Input,
            Output,
            Unknown,
        }
        private DragEndPoint dragType = DragEndPoint.Unknown;
        private NodeSocket snappedSocket;

        public void DragInputEndPoint(MouseButtonEventArgs e)
        {
            BeginDrag(e, DragEndPoint.Input);
            if (null == inputSocket) return;
            inputSocket.Disconnect(this);
            OnDisconnected(inputSocket);
            inputSocket = null;
            InputNode = null;
        }

        public void DragOutputEndPoint(MouseButtonEventArgs e)
        {
            BeginDrag(e, DragEndPoint.Output);
            if (null == outputSocket) return;
            outputSocket.Disconnect(this);
            OnDisconnected(outputSocket);
            outputSocket = null;
            OutputNode = null;
        }

        private void BeginDrag(MouseButtonEventArgs e, DragEndPoint endPoint)
        {
            if (null == root) return;
            if (MouseButton.Left != e.ChangedButton) return;
            CaptureMouse();
            dragType = endPoint;
            dragInit = (Point)(e.GetPosition(root) - (dragType == DragEndPoint.Input ? StartPoint : StopPoint));
            e.Handled = true;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            root = this.FindVisualParent<NodePanel>();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (DragEndPoint.Unknown == dragType) return;
            var rootRelative = e.GetPosition(root);
            var pos = (Point)(SnapIfPossible(rootRelative) - (dragType == DragEndPoint.Input ? StartPoint : StopPoint));
            if (DragEndPoint.Input == dragType)
                StartPoint = StartPoint + (pos - dragInit);
            else
                StopPoint = (StopPoint - dragInit) + pos;
            SnapIfPossible(rootRelative);
            UpdateCentrePoints();
            e.Handled = true;
        }

        private Point SnapIfPossible(Point rootRelativePoint)
        {
            snappedSocket = null;
            var nearestNode = root.GetNearestNode(rootRelativePoint);
            if (null == nearestNode) return rootRelativePoint;

            var nearestConnector = dragType == DragEndPoint.Input
                ? nearestNode.GetNearestFreeOutputSocket(rootRelativePoint, null == outputSocket ? null : Node.GetValidType(outputSocket))
                : nearestNode.GetNearestFreeInputSocket(rootRelativePoint, null == inputSocket ? null : Node.GetValidType(inputSocket));
            if (null == nearestConnector) return rootRelativePoint;

            var distance = (rootRelativePoint - nearestConnector.Position).Length;
            if (distance <= SnapDistance)
            {
                if (dragType == DragEndPoint.Input && !nearestNode.QueryCanConnectOutput(OutputNode) ||
                    dragType == DragEndPoint.Output && !nearestNode.QueryCanConnectInput(InputNode))
                    return rootRelativePoint;

                snappedSocket = nearestConnector;
                return nearestConnector.Position;
            }
            return rootRelativePoint;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            var type = dragType;
            dragType = DragEndPoint.Unknown;
            ReleaseMouseCapture();
            if (null != snappedSocket)
            {
                if (DragEndPoint.Input == type)
                    ConnectInputEndPoint(snappedSocket);
                else
                    ConnectOutputEndPoint(snappedSocket);
            }
            if (null == inputSocket || null == outputSocket)
                Delete();
            snappedSocket = null;
            e.Handled = true;
        }

        private bool deleted;
        public void Delete()
        {
            if (deleted) return;
            deleted = true;
            if (null != inputSocket)
                inputSocket.Disconnect(this);
            if (null != outputSocket)
                outputSocket.Disconnect(this);
            root.RemoveConnector(this);
        }

        #region Dependency Property Logic

        public static readonly DependencyProperty InputNodeProperty =
            DependencyProperty.Register("InputNode", typeof(Node), typeof(Connector), new PropertyMetadata(default(Node), OnInputNodeChanged));

        public static readonly DependencyProperty OutputNodeProperty =
            DependencyProperty.Register("OutputNode", typeof(Node), typeof(Connector), new PropertyMetadata(default(Node), OnOutputNodeChanged));

        public Node OutputNode
        {
            get { return (Node)GetValue(OutputNodeProperty); }
            set { SetValue(OutputNodeProperty, value); }
        }

        public Node InputNode
        {
            get { return (Node)GetValue(InputNodeProperty); }
            set { SetValue(InputNodeProperty, value); }
        }

        public static readonly DependencyPropertyKey StartPointProperty =
            DependencyProperty.RegisterReadOnly("StartPoint", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

        public static readonly DependencyPropertyKey StopPointProperty =
            DependencyProperty.RegisterReadOnly("StopPoint", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

        public static readonly DependencyPropertyKey TopCentreProperty =
            DependencyProperty.RegisterReadOnly("TopCentre", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

        public static readonly DependencyPropertyKey CentreProperty =
            DependencyProperty.RegisterReadOnly("Centre", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

        public static readonly DependencyPropertyKey BottomCentreProperty =
            DependencyProperty.RegisterReadOnly("BottomCentre", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

        public Point BottomCentre
        {
            get { return (Point)GetValue(BottomCentreProperty.DependencyProperty); }
            private set { SetValue(BottomCentreProperty, value); }
        }

        public Point Centre
        {
            get { return (Point)GetValue(CentreProperty.DependencyProperty); }
            private set { SetValue(CentreProperty, value); }
        }

        public Point TopCentre
        {
            get { return (Point)GetValue(TopCentreProperty.DependencyProperty); }
            private set { SetValue(TopCentreProperty, value); }
        }

        public Point StopPoint
        {
            get { return (Point)GetValue(StopPointProperty.DependencyProperty); }
            private set { SetValue(StopPointProperty, value); }
        }

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty.DependencyProperty); }
            private set { SetValue(StartPointProperty, value); }
        }

        private NodeSocket inputSocket;
        private NodeSocket outputSocket;

        public void ConnectInputEndPoint(NodeSocket to)
        {
            inputSocket = to;
            InputNode = to.Node;
            OnConnected(to);
            UpdateStartPoint();
            if (null == outputSocket)
                StopPoint = StartPoint;
            UpdateCentrePoints();
        }

        public void ConnectOutputEndPoint(NodeSocket to)
        {
            outputSocket = to;
            OutputNode = to.Node;
            OnConnected(to);
            UpdateStopPoint();
            if (null == inputSocket)
                StartPoint = StopPoint;
            UpdateCentrePoints();
        }

        private static void OnInputNodeChanged(DependencyObject dependencyObject,
                                               DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var connector = dependencyObject as Connector;
            if (null == connector) return;
            UnsubscribeInputSocket(connector);
            var node = dependencyPropertyChangedEventArgs.NewValue as Node;
            if (null == node) return;
            connector.inputSocket = (null == connector.inputSocket || !ReferenceEquals(connector.inputSocket.Node, node)) ? node.GetFreeOutputSocket() : connector.inputSocket;
            SubscribeInputSocket(connector);
            connector.UpdateStartPoint();
        }

        private static void OnOutputNodeChanged(DependencyObject dependencyObject,
                                                DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var connector = dependencyObject as Connector;
            if (null == connector) return;
            UnsubscribeOutputSocket(connector);
            var node = dependencyPropertyChangedEventArgs.NewValue as Node;
            if (null == node) return;
            connector.outputSocket = (null == connector.outputSocket || !ReferenceEquals(connector.outputSocket.Node, node)) ? node.GetFreeInputSocket() : connector.outputSocket;
            SubscribeOutputSocket(connector);
            connector.UpdateStopPoint();
        }

        private void UpdateStopPoint()
        {
            StopPoint = null == outputSocket ? StartPoint : outputSocket.Position;
            UpdateCentrePoints();
        }

        private void UpdateStartPoint()
        {
            StartPoint = null == inputSocket ? StopPoint : inputSocket.Position;
            UpdateCentrePoints();
        }

        private void UpdateCentrePoints()
        {
            var distance = StopPoint - StartPoint;
            Centre = StartPoint + (distance / 2d);
            TopCentre = new Point(StartPoint.X + Math.Abs(distance.X) / 2d, StartPoint.Y);
            BottomCentre = new Point(StopPoint.X - Math.Abs(distance.X) / 2d, StopPoint.Y);
        }

        private static void UnsubscribeOutputSocket(Connector connector)
        {
            if (null == connector.outputSocket) return;
            connector.outputSocket.PositionChanged -= connector.OnOutputSocketPositionChanged;
        }

        private static void SubscribeOutputSocket(Connector connector)
        {
            if (null == connector.outputSocket) return;
            connector.outputSocket.PositionChanged += connector.OnOutputSocketPositionChanged;
            connector.outputSocket.Connect(connector);
        }

        private static void SubscribeInputSocket(Connector connector)
        {
            if (null == connector.inputSocket) return;
            connector.inputSocket.PositionChanged += connector.OnInputSocketPositionChanged;
            connector.inputSocket.Connect(connector);
        }

        private static void UnsubscribeInputSocket(Connector connector)
        {
            if (null == connector.inputSocket) return;
            connector.inputSocket.PositionChanged -= connector.OnInputSocketPositionChanged;
            connector.inputSocket.Connect(null);
        }

        private void OnInputSocketPositionChanged(object sender, EventArgs e)
        {
            UpdateStartPoint();
        }

        private void OnOutputSocketPositionChanged(object sender, EventArgs e)
        {
            UpdateStopPoint();
        }

        #endregion Dependency Property Logic

        #region Implementation of IConnectable

        public void Connect(IConnectable to)
        {
            var socket = to as NodeSocket;
            if (null == socket) return;
            var type = socket.SocketType;
            switch (type)
            {
                case SocketType.Input:
                    ConnectOutputEndPoint(socket);
                    break;
                case SocketType.Output:
                    ConnectInputEndPoint(socket);
                    break;
            }
        }

        public void Disconnect(IConnectable item)
        {
            if (!ReferenceEquals(item, InputNode) &&
                !ReferenceEquals(item, OutputNode) &&
                !ReferenceEquals(item, inputSocket) &&
                !ReferenceEquals(item, outputSocket)) return;
            if (dragType == DragEndPoint.Unknown)
            {
                Delete();
                OnDisconnected(inputSocket);
                OnDisconnected(outputSocket);
            }
            else if (ReferenceEquals(item, inputSocket))
                OnDisconnected(inputSocket);
            else if (ReferenceEquals(item, outputSocket))
                OnDisconnected(outputSocket);
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
    }
}