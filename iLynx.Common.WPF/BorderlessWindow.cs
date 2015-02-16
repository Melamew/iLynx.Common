using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace iLynx.Common.WPF
{
    /// <summary>
    /// DockingWindow
    /// </summary>
    public class BorderlessWindow : Window
    {
        private readonly WindowInteropHelper windowInteropHelper;
        /// <summary>
        /// The header size property
        /// </summary>
        public static readonly DependencyProperty HeaderSizeProperty =
            DependencyProperty.Register("HeaderSize", typeof(GridLength), typeof(BorderlessWindow), new PropertyMetadata(new GridLength(24d), OnHeadersizeChanged));

        /// <summary>
        /// Gets or sets the size of the header.
        /// </summary>
        /// <value>
        /// The size of the header.
        /// </value>
        public GridLength HeaderSize
        {
            get { return (GridLength)GetValue(HeaderSizeProperty); }
            set { SetValue(HeaderSizeProperty, value); }
        }

        /// <summary>
        /// The header property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(BorderlessWindow), new PropertyMetadata(default(object)));

        /// <summary>
        /// The title font size property
        /// </summary>
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register("TitleFontSize", typeof(double), typeof(BorderlessWindow), new PropertyMetadata(default(double)));

        /// <summary>
        /// The title font weight property
        /// </summary>
        public static readonly DependencyProperty TitleFontWeightProperty =
            DependencyProperty.Register("TitleFontWeight", typeof(FontWeight), typeof(BorderlessWindow), new PropertyMetadata(default(FontWeight)));

        /// <summary>
        /// The title font style property
        /// </summary>
        public static readonly DependencyProperty TitleFontStyleProperty =
            DependencyProperty.Register("TitleFontStyle", typeof(FontStyle), typeof(BorderlessWindow), new PropertyMetadata(default(FontStyle)));

        /// <summary>
        /// The title font family property
        /// </summary>
        public static readonly DependencyProperty TitleFontFamilyProperty =
            DependencyProperty.Register("TitleFontFamily", typeof(FontFamily), typeof(BorderlessWindow), new PropertyMetadata(default(FontFamily)));

        /// <summary>
        /// The title font stretch property
        /// </summary>
        public static readonly DependencyProperty TitleFontStretchProperty =
            DependencyProperty.Register("TitleFontStretch", typeof(FontStretch), typeof(BorderlessWindow), new PropertyMetadata(default(FontStretch)));

        /// <summary>
        /// The is collapsible property
        /// </summary>
        public static readonly DependencyProperty IsCollapsibleProperty =
            DependencyProperty.Register("IsCollapsible", typeof(bool), typeof(BorderlessWindow), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The toggle collapsed command property
        /// </summary>
        public static readonly DependencyProperty ToggleCollapsedCommandProperty =
            DependencyProperty.Register("ToggleCollapsedCommand", typeof(ICommand), typeof(BorderlessWindow), new PropertyMetadata(new DelegateCommand<BorderlessWindow>(OnToggleCollapsed)));

        public event EventHandler ResizeBegin;
        public event EventHandler ResizeEnd;

        protected virtual void OnResizeBegin()
        {
            if (null != ResizeBegin)
                ResizeBegin(this, EventArgs.Empty);
        }

        protected virtual void OnResizeEnd()
        {
            if (null != ResizeEnd)
                ResizeEnd(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when [toggle collapsed].
        /// </summary>
        /// <param name="window">The window.</param>
        private static void OnToggleCollapsed(BorderlessWindow window)
        {
            window.IsCollapsed = !window.IsCollapsed;
        }

        private static void OnHeadersizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var win = dependencyObject as BorderlessWindow;
            if (null == win) return;
            if (win.IsCollapsed && Math.Abs(win.Height - win.HeaderSize.Value) >= double.Epsilon)
                win.Height = win.HeaderSize.Value;
        }

        private double storedHeight;

        protected virtual void Expand()
        {
            Height = storedHeight;
            OnExpanded();
        }

        protected virtual void Collapse()
        {
            storedHeight = ActualHeight;
            Height = HeaderSize.Value;
            OnCollapsed();
        }

        /// <summary>
        /// The expanded event
        /// </summary>
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded",
                                                                                            RoutingStrategy.Direct,
                                                                                            typeof(RoutedEventHandler),
                                                                                            typeof(BorderlessWindow));

        /// <summary>
        /// The collapsed event
        /// </summary>
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed",
                                                                                             RoutingStrategy.Direct,
                                                                                             typeof(RoutedEventHandler),
                                                                                             typeof(BorderlessWindow));

        /// <summary>
        /// Occurs when [expanded].
        /// </summary>
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }


        /// <summary>
        /// Occurs when [collapsed].
        /// </summary>
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }

        /// <summary>
        /// Called when [expanded].
        /// </summary>
        private void OnExpanded()
        {
            RaiseEvent(new RoutedEventArgs(ExpandedEvent));
        }

        /// <summary>
        /// Called when [collapsed].
        /// </summary>
        private void OnCollapsed()
        {
            RaiseEvent(new RoutedEventArgs(CollapsedEvent));
        }

        /// <summary>
        /// The is collapsed property
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(BorderlessWindow), new PropertyMetadata(default(bool), OnIsCollapsedChanged));

        private static void OnIsCollapsedChanged(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var win = dependencyObject as BorderlessWindow;
            if (null == win) return;
            var newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (newValue)
                win.Collapse();
            else
                win.Expand();
        }

        /// <summary>
        /// The collapsed header property
        /// </summary>
        public static readonly DependencyProperty CollapsedHeaderProperty =
            DependencyProperty.Register("CollapsedHeader", typeof(object), typeof(BorderlessWindow), new PropertyMetadata(default(object)));

        /// <summary>
        /// The collapsed height property
        /// </summary>
        public static readonly DependencyProperty CollapsedHeightProperty =
            DependencyProperty.Register("CollapsedHeight", typeof(double), typeof(BorderlessWindow), new PropertyMetadata(24d));

        /// <summary>
        /// The collapsed width property
        /// </summary>
        public static readonly DependencyProperty CollapsedWidthProperty =
            DependencyProperty.Register("CollapsedWidth", typeof(double), typeof(BorderlessWindow), new PropertyMetadata(200d));

        /// <summary>
        /// Gets or sets the width of the collapsed.
        /// </summary>
        /// <value>
        /// The width of the collapsed.
        /// </value>
        public double CollapsedWidth
        {
            get { return (double)GetValue(CollapsedWidthProperty); }
            set { SetValue(CollapsedWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the height of the collapsed.
        /// </summary>
        /// <value>
        /// The height of the collapsed.
        /// </value>
        public double CollapsedHeight
        {
            get { return (double)GetValue(CollapsedHeightProperty); }
            set { SetValue(CollapsedHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the collapsed header.
        /// </summary>
        /// <value>
        /// The collapsed header.
        /// </value>
        public object CollapsedHeader
        {
            get { return GetValue(CollapsedHeaderProperty); }
            set { SetValue(CollapsedHeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is collapsed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is collapsed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the toggle collapsed command.
        /// </summary>
        /// <value>
        /// The toggle collapsed command.
        /// </value>
        public ICommand ToggleCollapsedCommand
        {
            get { return (ICommand)GetValue(ToggleCollapsedCommandProperty); }
            set { SetValue(ToggleCollapsedCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is collapsible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is collapsible; otherwise, <c>false</c>.
        /// </value>
        public bool IsCollapsible
        {
            get { return (bool)GetValue(IsCollapsibleProperty); }
            set { SetValue(IsCollapsibleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title font stretch.
        /// </summary>
        /// <value>
        /// The title font stretch.
        /// </value>
        public FontStretch TitleFontStretch
        {
            get { return (FontStretch)GetValue(TitleFontStretchProperty); }
            set { SetValue(TitleFontStretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title font family.
        /// </summary>
        /// <value>
        /// The title font family.
        /// </value>
        public FontFamily TitleFontFamily
        {
            get { return (FontFamily)GetValue(TitleFontFamilyProperty); }
            set { SetValue(TitleFontFamilyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title font style.
        /// </summary>
        /// <value>
        /// The title font style.
        /// </value>
        public FontStyle TitleFontStyle
        {
            get { return (FontStyle)GetValue(TitleFontStyleProperty); }
            set { SetValue(TitleFontStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title font weight.
        /// </summary>
        /// <value>
        /// The title font weight.
        /// </value>
        public FontWeight TitleFontWeight
        {
            get { return (FontWeight)GetValue(TitleFontWeightProperty); }
            set { SetValue(TitleFontWeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the size of the title font.
        /// </summary>
        /// <value>
        /// The size of the title font.
        /// </value>
        public double TitleFontSize
        {
            get { return (double)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// The border size property
        /// </summary>
        public static readonly DependencyProperty BorderSizeProperty =
            DependencyProperty.Register("BorderSize", typeof(GridLength), typeof(BorderlessWindow), new PropertyMetadata(default(GridLength)));

        /// <summary>
        /// Gets or sets the size of the border.
        /// </summary>
        /// <value>
        /// The size of the border.
        /// </value>
        public GridLength BorderSize
        {
            get { return (GridLength)GetValue(BorderSizeProperty); }
            set { SetValue(BorderSizeProperty, value); }
        }

        /// <summary>
        /// The can maximize property
        /// </summary>
        public static readonly DependencyProperty CanMaximizeProperty =
            DependencyProperty.Register("CanMaximize", typeof(bool), typeof(BorderlessWindow), new PropertyMetadata(default(bool)));

        /// <summary>
        /// The close command property
        /// </summary>
        public static readonly DependencyPropertyKey CloseCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("CloseCommand", typeof(ICommand), typeof(BorderlessWindow), new PropertyMetadata(new DelegateCommand<BorderlessWindow>(OnCloseCommand)));

        /// <summary>
        /// The close command property
        /// </summary>
        public static readonly DependencyProperty CloseCommandProperty = CloseCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// The minimize command property key
        /// </summary>
        public static readonly DependencyPropertyKey MinimizeCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("MinimizeCommand", typeof(ICommand), typeof(BorderlessWindow), new PropertyMetadata(new DelegateCommand<BorderlessWindow>(OnMinimizeCommand)));

        /// <summary>
        /// The minimize command property
        /// </summary>
        public static readonly DependencyProperty MinimizeCommandProperty =
            MinimizeCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// The toggle maximized command property key
        /// </summary>
        public static readonly DependencyPropertyKey ToggleMaximizedCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("ToggleMaximizedCommand", typeof(ICommand), typeof(BorderlessWindow), new PropertyMetadata(new DelegateCommand<BorderlessWindow>(OnToggleMaximized)));

        /// <summary>
        /// The toggle maximized command property
        /// </summary>
        public static readonly DependencyProperty ToggleMaximizedCommandProperty =
            ToggleMaximizedCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// The header border thickness property
        /// </summary>
        public static readonly DependencyProperty HeaderBorderThicknessProperty =
            DependencyProperty.Register("HeaderBorderThickness", typeof(Thickness), typeof(BorderlessWindow), new PropertyMetadata(default(Thickness)));

        /// <summary>
        /// Gets or sets the header border thickness.
        /// </summary>
        /// <value>
        /// The header border thickness.
        /// </value>
        public Thickness HeaderBorderThickness
        {
            get { return (Thickness)GetValue(HeaderBorderThicknessProperty); }
            set { SetValue(HeaderBorderThicknessProperty, value); }
        }

        private static readonly Dictionary<ResizeDirection, Cursor> ResizeCursors =
            new Dictionary<ResizeDirection, Cursor>
            {
                { ResizeDirection.Top, Cursors.SizeNS},
                { ResizeDirection.Bottom, Cursors.SizeNS},
                { ResizeDirection.Left, Cursors.SizeWE},
                { ResizeDirection.Right, Cursors.SizeWE},
                { ResizeDirection.TopLeft, Cursors.SizeNWSE},
                { ResizeDirection.TopRight, Cursors.SizeNESW},
                { ResizeDirection.BottomLeft, Cursors.SizeNESW},
                { ResizeDirection.BottomRight, Cursors.SizeNWSE},
            };

        private Button maximizeButton;
        private Rectangle headerRect;
        private Border mainBorder;
        private ResizeDirection resizeDirection;
        private Rect startBounds;
        private Point startPoint;
        private bool isMoving;
        private DateTime lastDown = DateTime.MinValue;
        private bool isClicking;

        /// <summary>
        /// Gets the header rectangle.
        /// </summary>
        /// <value>
        /// The header rectangle.
        /// </value>
        protected Rectangle HeaderRectangle { get { return headerRect; } }

        /// <summary>
        /// Called when [toggle maximized].
        /// </summary>
        /// <param name="borderlessWindow">The borderless window.</param>
        private static void OnToggleMaximized(BorderlessWindow borderlessWindow)
        {
            borderlessWindow.WindowState = WindowState.Maximized == borderlessWindow.WindowState ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Called when [minimize command].
        /// </summary>
        /// <param name="borderlessWindow">The borderless window.</param>
        private static void OnMinimizeCommand(BorderlessWindow borderlessWindow)
        {
            borderlessWindow.WindowState = WindowState.Minimized;
        }

        //private FrameworkElement contentHost;

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            maximizeButton = Template.FindName("maximizeButton", this) as Button;
            if (null != maximizeButton)
                SetMaximizeContent();

            mainBorder = Template.FindName("PART_Main", this) as Border;
            if (null != mainBorder)
            {
                mainBorder.PreviewMouseMove += MainBorderOnPreviewMouseMove;
                mainBorder.MouseDown += MainBorderOnMouseDown;
                mainBorder.MouseUp += MainBorderOnMouseUp;
            }

            headerRect = Template.FindName("DragGrip", this) as Rectangle;
            if (null == headerRect) return;
            headerRect.PreviewMouseDown += HeaderRectOnPreviewMouseDown;
            headerRect.PreviewMouseMove += HeaderRectOnPreviewMouseMove;
        }

        private void SetMaximizeContent()
        {
            maximizeButton.Content = WindowState == WindowState.Maximized ? "2" : "1";
        }

        private void HeaderRectOnPreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton != MouseButtonState.Pressed) return;
            if (WindowState.Maximized == WindowState)
            {
                var relPos = mouseEventArgs.GetPosition(this);
                var relFac = relPos.X/ActualWidth;
                var workingArea = Screen.FromHandle(windowInteropHelper.EnsureHandle()).WorkingArea;
                var prevX = workingArea.Left;
                WindowState = WindowState.Normal;
                Left = prevX + relPos.X - (relFac * ActualWidth);
                Top = workingArea.Top;
            }
            DragMove();
        }

        private void HeaderRectOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            HandleHeaderMaximizeClick();
        }

        private void HandleHeaderMaximizeClick()
        {
            if (IsCollapsed) return;
            if (DateTime.MinValue == lastDown)
                lastDown = DateTime.Now;

            var deltaClick = DateTime.Now - lastDown;
            if (deltaClick.TotalMilliseconds <= GetDoubleClickTime() && isClicking)
            {
                isClicking = false;
                OnToggleMaximized(this);
                lastDown = DateTime.MinValue;
                return;
            }
            lastDown = DateTime.Now;
            isClicking = true;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (IsCollapsed)
            {
                WindowState = WindowState.Normal;
                return;
            }
            SetMaximizeContent();
            base.OnStateChanged(e);
        }

        private ResizeDirection GetDirection(Point p)
        {
            var borderThickness = mainBorder.BorderThickness;
            var result = ResizeDirection.None;
            if (p.X >= 0 && p.X <= borderThickness.Left)
            {
                if (p.Y >= 0 && p.Y <= borderThickness.Top)
                    result = ResizeDirection.TopLeft;
                else if (p.Y >= borderThickness.Top && p.Y <= mainBorder.ActualHeight - borderThickness.Bottom)
                    result = ResizeDirection.Left;
                else
                    result = ResizeDirection.BottomLeft;
            }
            else if (p.X >= borderThickness.Left && p.X <= mainBorder.ActualWidth - borderThickness.Right)
            {
                if (p.Y >= 0 && p.Y <= borderThickness.Top)
                    result = ResizeDirection.Top;
                else if (p.Y >= mainBorder.ActualHeight - borderThickness.Bottom && p.Y <= mainBorder.ActualHeight)
                    result = ResizeDirection.Bottom;
            }
            else if (p.X >= mainBorder.ActualWidth - borderThickness.Right && p.X <= mainBorder.ActualWidth)
            {
                if (p.Y >= 0 && p.Y <= borderThickness.Top)
                    result = ResizeDirection.TopRight;
                else if (p.Y >= borderThickness.Top && p.Y <= mainBorder.ActualHeight - borderThickness.Bottom)
                    result = ResizeDirection.Right;
                else
                    result = ResizeDirection.BottomRight;
            }
            return result;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var hwnd = windowInteropHelper.EnsureHandle();
            if (IntPtr.Zero == hwnd)
            {
                this.LogWarning("Unable to get HWND for this window");
                return;
            }
            var source = HwndSource.FromHwnd(hwnd);
            if (null == source)
            {
                this.LogWarning("Unable to get HWND Source for this window");
                return;
            }
            source.AddHook(WindowProc);
        }

        private IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            switch (msg)
            {
                case 0x0024: /* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        // TODO: Move this, and associated features to a separate class so it can be used from other windows as well.
        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            const int monitorDefaulttonearest = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, monitorDefaulttonearest);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;
                var source = PresentationSource.FromVisual(this);
                if (null == source)
                {
                    this.LogWarning("Unable to get PresentationSource for this window");
                    return;
                }
                var compositionTarget = source.CompositionTarget;
                if (null == compositionTarget)
                {
                    this.LogWarning("Unable to get CompositionTarget for this window");
                    return;
                }
                var dpiX = compositionTarget.TransformToDevice.M11;
                var dpiY = compositionTarget.TransformToDevice.M22;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = (int)Math.Abs(rcWorkArea.right - rcWorkArea.left * dpiX);
                mmi.ptMaxSize.y = (int)Math.Abs(rcWorkArea.bottom - rcWorkArea.top * dpiY);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        private ResizeDirection previousDirection;

        private void SetCursor(ResizeDirection direction)
        {
            if (ResizeDirection.None == direction)
            {
                Mouse.OverrideCursor = previousOverrideCursor;
                return;
            }
            Cursor cursor;
            if (!ResizeCursors.TryGetValue(direction, out cursor)) return;
            if (cursor == Mouse.OverrideCursor) return;
            previousOverrideCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = cursor;
        }

        private void MainBorderOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var oldDirection = resizeDirection;
            resizeDirection = ResizeDirection.None;
            SetCursor(resizeDirection);
            mainBorder.ReleaseMouseCapture();
            if (ResizeDirection.None == oldDirection) return;
            OnResizeEnd();
        }

        private void MainBorderOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (isMoving || IsCollapsed)
            {
                isMoving = false;
                return;
            }
            var point = mouseButtonEventArgs.GetPosition(mainBorder);
            var direction = GetDirection(point);
            if (direction == ResizeDirection.None) return;
            OnResizeBegin();
            mainBorder.CaptureMouse();
            SetStartBounds();
            resizeDirection = direction;
            startPoint = point;
            SetCursor(resizeDirection);
        }

        private void SetStartBounds()
        {
            startBounds = new Rect(Left, Top, ActualWidth, ActualHeight);
        }

        private void MainBorderOnPreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (IsCollapsed) return;
            if (ResizeDirection.None == resizeDirection)
            {
                if (previousDirection != ResizeDirection.None)
                    SetCursor(ResizeDirection.None);
                var direction = GetDirection(mouseEventArgs.GetPosition(mainBorder));
                previousDirection = direction;
                SetCursor(direction);
            }
            var point = mouseEventArgs.GetPosition(mainBorder);
            DoResize(point);
            startPoint = mouseEventArgs.GetPosition(mainBorder);
            SetStartBounds();
        }

        private void DoResize(Point mouse)
        {
            var delta = new Point(startPoint.X - mouse.X, startPoint.Y - mouse.Y);
            if (ResizeDirection.Top == (resizeDirection & ResizeDirection.Top) && Height - mouse.Y > MinHeight)
            {
                Height -= mouse.Y;
                Top += mouse.Y;
            }
            if (ResizeDirection.Left == (resizeDirection & ResizeDirection.Left) && Width - mouse.X > MinWidth)
            {
                Width -= mouse.X;
                Left += mouse.X;
            }
            if (ResizeDirection.Bottom == (resizeDirection & ResizeDirection.Bottom) && Height - delta.Y > MinHeight)
                Height = startBounds.Height - delta.Y;
            if (ResizeDirection.Right == (resizeDirection & ResizeDirection.Right) && Width - delta.X > MinWidth)
                Width = startBounds.Width - delta.X;
        }

        private Cursor previousOverrideCursor;

        /// <summary>
        /// Called when [close command].
        /// </summary>
        /// <param name="borderlessWindow">The borderless window.</param>
        private static void OnCloseCommand(BorderlessWindow borderlessWindow)
        {
            if (null == borderlessWindow) return;
            borderlessWindow.Close();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can maximize.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can maximize; otherwise, <c>false</c>.
        /// </value>
        public bool CanMaximize
        {
            get { return (bool)GetValue(CanMaximizeProperty); }
            set { SetValue(CanMaximizeProperty, value); }
        }

        /// <summary>
        /// Initializes the <see cref="BorderlessWindow" /> class.
        /// </summary>
        static BorderlessWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderlessWindow), new FrameworkPropertyMetadata(typeof(BorderlessWindow)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderlessWindow" /> class.
        /// </summary>
        public BorderlessWindow()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Top = 0;
            Left = 0;
            windowInteropHelper = new WindowInteropHelper(this);
        }

        /// <summary>
        /// Gets the double click time.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        /// <summary>
        /// ResizeDirection
        /// </summary>
        [Flags]
        public enum ResizeDirection
        {
            None = 0,
            Bottom = 1,
            Left = 2,
            Right = 4,
            Top = 8,
            TopLeft = Top | Left,
            TopRight = Top | Right,
            BottomLeft = Bottom | Left,
            BottomRight = Bottom | Right,
        }
    }
}
