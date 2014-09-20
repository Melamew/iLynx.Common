using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using iLynx.Common.Interfaces;

namespace iLynx.Common.WPF.Controls
{
    public class Node : ContentControl, IPositionable, IConnectable
    {
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(Point), typeof(Node), new PropertyMetadata(default(Point), OnPositionChanged));
        private UIElement nearestParent;
        private NodePanel root;

        private static void OnPositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var node = dependencyObject as Node;
            if (null == node) return;
            if (null == node.nearestParent) return;
            Canvas.SetLeft(node.nearestParent, node.Position.X);
            Canvas.SetTop(node.nearestParent, node.Position.Y);
        }

        private DependencyObject WalkVisualTree()
        {
            var parent = VisualTreeHelper.GetParent(this);
            if (parent is NodePanel)
                return parent;
            DependencyObject p = null;
            while (parent != null && !((parent = VisualTreeHelper.GetParent(parent)) is NodePanel))
                p = parent;
            return p;
        }

        private INode contentNode;

        public static readonly DependencyPropertyKey InputSocketsPropertyKey =
            DependencyProperty.RegisterReadOnly("InputSockets", typeof(ObservableCollection<NodeSocket>), typeof(Node), new PropertyMetadata(default(ObservableCollection<NodeSocket>)));
        public static readonly DependencyPropertyKey OutputSocketsPropertyKey =
            DependencyProperty.RegisterReadOnly("OutputSockets", typeof(ObservableCollection<NodeSocket>), typeof(Node), new PropertyMetadata(default(ObservableCollection<NodeSocket>)));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(Node), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(Node), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(Node), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty CanAutoAddInputsProperty =
            DependencyProperty.Register("CanAutoAddInputs", typeof(bool), typeof(Node), new PropertyMetadata(true));

        public static readonly DependencyProperty CanAutoAddOutputsProperty =
            DependencyProperty.Register("CanAutoAddOutputs", typeof(bool), typeof(Node), new PropertyMetadata(true));

        public static readonly DependencyProperty CanMultiConnectProperty =
            DependencyProperty.Register("CanMultiConnect", typeof(bool), typeof(Node), new PropertyMetadata(default(bool)));

        public static readonly DependencyPropertyKey DeleteCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("DeleteCommand", typeof(ICommand), typeof(Node), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ValidTypeProperty =
            DependencyProperty.RegisterAttached("ValidType", typeof(Type), typeof(Node), new PropertyMetadata(default(Type)));

        public static void SetValidType(NodeSocket element, Type value)
        {
            element.SetValue(ValidTypeProperty, value);
        }

        public static Type GetValidType(NodeSocket element)
        {
            return (Type)element.GetValue(ValidTypeProperty);
        }

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandPropertyKey.DependencyProperty); }
            private set { SetValue(DeleteCommandPropertyKey, value); }
        }

        public bool CanMultiConnect
        {
            get { return (bool)GetValue(CanMultiConnectProperty); }
            set { SetValue(CanMultiConnectProperty, value); }
        }

        public bool CanAutoAddInputs
        {
            get { return (bool)GetValue(CanAutoAddInputsProperty); }
            set { SetValue(CanAutoAddInputsProperty, value); }
        }

        public bool CanAutoAddOutputs
        {
            get { return (bool)GetValue(CanAutoAddOutputsProperty); }
            set { SetValue(CanAutoAddOutputsProperty, value); }
        }

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ObservableCollection<NodeSocket> OutputSockets
        {
            get { return (ObservableCollection<NodeSocket>)GetValue(OutputSocketsPropertyKey.DependencyProperty); }
            private set { SetValue(OutputSocketsPropertyKey, value); }
        }

        public ObservableCollection<NodeSocket> InputSockets
        {
            get { return (ObservableCollection<NodeSocket>)GetValue(InputSocketsPropertyKey.DependencyProperty); }
            private set { SetValue(InputSocketsPropertyKey, value); }
        }

        public Node()
        {
            DefaultStyleKey = typeof(Node);
            InputSockets = new ObservableCollection<NodeSocket>();
            OutputSockets = new ObservableCollection<NodeSocket>();
            AddInputSocket();
            AddOutputSocket();
            DeleteCommand = new DelegateCommand(OnDeleteNode);
        }

        private void OnDeleteNode()
        {
            foreach (var node in InputSockets.ToArray())
                node.Disconnect(this);
            foreach (var node in OutputSockets.ToArray())
                node.Disconnect(this);
            InputSockets.Clear();
            OutputSockets.Clear();
            if (null != root)
                root.RemoveNode(this);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            contentNode = newContent as INode;
            if (null == contentNode) return;
            foreach (var connectedOutput in GetConnectedOutputs().Select(x => x.contentNode).Where(x => null != x))
                contentNode.OnOutputConnected(connectedOutput);
            foreach (var connectedInput in GetConnectedInputs().Select(x => x.contentNode).Where(x => null != x))
                contentNode.OnOutputConnected(connectedInput);
            InputSockets.Clear();
            OutputSockets.Clear();
            if (null != contentNode.Inputs)
            {
                InputSockets.AddRange(contentNode.Inputs
                    .Select(x =>
                    {
                        var socket = GetSocket(x);
                        SubscribeInputSocket(socket);
                        return socket;
                    }));
            }
            if (null != contentNode.Outputs)
            {
                OutputSockets.AddRange(contentNode.Outputs
                    .Select(x =>
                    {
                        var socket = GetSocket(x);
                        SubscribeOutputSocket(socket);
                        return socket;
                    }));
            }
            contentNode.PropertyChanged += ContentNodeOnPropertyChanged;
            CanAutoAddInputs = contentNode.AutoAddInputs;
            CanAutoAddOutputs = contentNode.AutoAddOutputs;
            AddInputSocket();
            AddOutputSocket();
        }

        private NodeSocket GetSocket(Type forType)
        {
            var socket = new NodeSocket();
            SetValidType(socket, forType);
            return socket;
        }

        private void TransferContentProperties()
        {

        }

        private void ContentNodeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            TransferContentProperties();
        }

        private void InputSocketOnDisconnected(object sender, ConnectedEventArgs connectedEventArgs)
        {
            var socket = sender as NodeSocket;
            if (null == socket) return;
            var target = socket.GetTargetNodes().FirstOrDefault();
            if (null != target && null != contentNode && null != target.contentNode)
                contentNode.OnInputDisconnected(target.contentNode);
            if (!socket.IsConnected && CanAutoAddInputs)
                InputSockets.Remove(socket);
            AddInputSocket();
        }

        private void InputSocketOnConnected(object sender, ConnectedEventArgs connectedEventArgs)
        {
            AddInputSocket();
            var socket = sender as NodeSocket;
            if (socket == null) return;
            var target = socket.GetTargetNodes().FirstOrDefault();
            if (null != target && null != contentNode && null != target.contentNode)
                contentNode.OnInputConnected(target.contentNode);
        }

        private void AddOutputSocket()
        {
            if (!CanAutoAddOutputs || GetFreeOutputSocket() != null) return;
            var socket = new NodeSocket();
            if (null != contentNode && contentNode.Outputs != null)
                SetValidType(socket, contentNode.Outputs.LastOrDefault());
            SubscribeOutputSocket(socket);
            OutputSockets.Add(socket);
        }

        private void AddInputSocket()
        {
            if (!CanAutoAddInputs || GetFreeInputSocket() != null) return;
            var socket = new NodeSocket();
            if (null != contentNode && contentNode.Inputs != null)
                SetValidType(socket, contentNode.Inputs.LastOrDefault());
            SubscribeInputSocket(socket);
            InputSockets.Add(socket);
        }

        private void SubscribeInputSocket(IConnectable socket)
        {
            socket.Connected += InputSocketOnConnected;
            socket.Disconnected += InputSocketOnDisconnected;
        }

        private void SubscribeOutputSocket(IConnectable socket)
        {
            socket.Connected += OutputSocketOnConnected;
            socket.Disconnected += OutputSocketOnDisconnected;
        }

        private void OutputSocketOnConnected(object sender, ConnectedEventArgs connectedEventArgs)
        {
            AddOutputSocket();
            var socket = sender as NodeSocket;
            if (socket == null) return;
            var targets = socket.GetTargetNodes();
            foreach (var target in targets.Where(x => null != x && null != x.contentNode))
                contentNode.OnOutputConnected(target.contentNode);
        }

        private void OutputSocketOnDisconnected(object sender, ConnectedEventArgs connectedEventArgs)
        {
            var socket = sender as NodeSocket;
            if (null == socket) return;
            var targets = socket.GetTargetNodes().Concat(new[] { connectedEventArgs.Other as Node });
            foreach (var target in targets.Where(x => null != x && null != x.contentNode))
                contentNode.OnOutputDisconnected(target.contentNode);
            if (!socket.IsConnected && CanAutoAddOutputs)
                OutputSockets.Remove(socket);
            AddOutputSocket();
        }

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public bool InputChainContains(Node otherNode)
        {
            return InputSockets
                .Select(x => x.GetTargetNodes())
                .Where(x => null != x)
                .Any(nodes =>
                    {
                        var ns = nodes.ToArray();
                        return ns.Contains(otherNode) || ns.Any(x => x.InputChainContains(otherNode));
                    });
        }

        public bool OutputChainContains(Node otherNode)
        {
            return OutputSockets
                .Select(x => x.GetTargetNodes())
                .Where(x => null != x)
                .Any(nodes =>
                    {
                        var ns = nodes.ToArray();
                        return ns.Contains(otherNode) || ns.Any(x => x.OutputChainContains(otherNode));
                    });
        }

        public bool QueryCanConnectInput(Node toOtherNode)
        {
            if (null == toOtherNode) return false;
            return !ReferenceEquals(this, toOtherNode) && !toOtherNode.InputChainContains(this);
        }

        public bool QueryCanConnectOutput(Node toOtherNode)
        {
            if (null == toOtherNode) return false;
            return !ReferenceEquals(this, toOtherNode) && !toOtherNode.OutputChainContains(this);
        }

        public event EventHandler PositionChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (PositionProperty == e.Property)
                OnPositionChanged();
        }

        protected virtual void OnPositionChanged()
        {
            var handler = PositionChanged;
            if (null == handler) return;
            handler(this, EventArgs.Empty);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            Trace.WriteLine(string.Format("Parent changed: Old: {0}, New: {1}", oldParent, Parent));
            nearestParent = WalkVisualTree() as UIElement;
            root = this.FindVisualParent<NodePanel>();
            Trace.WriteLine(string.Format("Found nearest Parent: {0}", nearestParent));
        }

        private Point dragInit;
        private bool isMouseDown;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton != MouseButton.Right)
            {
                if (MouseButton.Left != e.ChangedButton) return;
                CaptureMouse();
                dragInit = e.GetPosition(this);
                isMouseDown = true;
                e.Handled = true;
            }
            else
            {
                if (null == ContextMenu) return;
                ContextMenu.IsOpen = true;
                ContextMenu.DataContext = this;
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!isMouseDown) return;
            Position = (Position - dragInit) + e.GetPosition(this);
            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            isMouseDown = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }

        public SocketType GetSocketType(NodeSocket socket)
        {
            if (InputSockets.Contains(socket))
                return SocketType.Input;
            return OutputSockets.Contains(socket) ? SocketType.Output : SocketType.Unknown;
        }

        private static bool IsSocketFree(NodeSocket socket)
        {
            return !socket.IsConnected;
        }

        private static bool IsCompatible(NodeSocket socket, Type type)
        {
            return (null == type || GetValidType(socket) == type);
        }

        public bool HasFreeInputSocket()
        {
            return InputSockets.Any(IsSocketFree);
        }

        public bool HasFreeOutputSocket()
        {
            return OutputSockets.Any(IsSocketFree);
        }

        public NodeSocket GetFreeOutputSocket(Type type = null)
        {
            return OutputSockets.FirstOrDefault(x => IsCompatible(x, type));
        }

        public NodeSocket GetFreeInputSocket(Type type = null)
        {
            return InputSockets.FirstOrDefault(x => IsSocketFree(x) && IsCompatible(x, type));
        }

        public NodeSocket GetNearestFreeOutputSocket(Point point, Type type = null)
        {
            return OutputSockets.GetNearestItem(point, x => IsCompatible(x, type));
        }

        public NodeSocket GetNearestFreeInputSocket(Point point, Type type = null)
        {
            return InputSockets.GetNearestItem(point, x => IsSocketFree(x) && IsCompatible(x, type));
        }

        public bool IsConnected(Node otherNode)
        {
            return GetConnectedInputs().Contains(otherNode) || GetConnectedOutputs().Contains(otherNode);
        }

        public IEnumerable<Node> GetConnectedOutputs()
        {
            return GetConnected(OutputSockets);
        }

        public IEnumerable<Node> GetConnectedInputs()
        {
            return GetConnected(InputSockets);
        }

        private IEnumerable<Node> GetConnected(IEnumerable<NodeSocket> sockets)
        {
            return sockets.Where(x => x.IsConnected).SelectMany(x => x.GetTargetNodes());
        }

        public bool HasConnectedOutputs()
        {
            return OutputSockets.Any(x => x.IsConnected);
        }

        public bool HasConnectedInputs()
        {
            return InputSockets.Any(x => x.IsConnected);
        }

        #region Implementation of IConnectable

        private void Connect(Node to)
        {
            var inputSocket = to.GetFreeInputSocket();
            var outputSocket = GetFreeOutputSocket();
            if (null == inputSocket) return;
            Connect(outputSocket, inputSocket);
        }

        private void Connect(NodeSocket from, IConnectable to)
        {
            if (!InputSockets.Contains(from) && !OutputSockets.Contains(from)) return;
            from.Connect(to);
        }

        public void Connect(IConnectable to)
        {
            var node = to as Node;
            if (null != node)
            {
                Connect(node);
                return;
            }
            var socket = to as NodeSocket;
            if (null == socket) return;
            var type = socket.SocketType;
            var other = SocketType.Input == type ? GetFreeOutputSocket() : GetFreeInputSocket();
            Connect(other, socket);
        }

        public void Disconnect(IConnectable item)
        {
        }

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<ConnectedEventArgs> Disconnected;

        #endregion
    }
}