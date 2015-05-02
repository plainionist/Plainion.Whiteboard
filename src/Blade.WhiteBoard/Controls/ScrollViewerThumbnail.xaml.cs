using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Plainion.WhiteBoard.Designer;

namespace Plainion.WhiteBoard.Controls
{
    public partial class ScrollViewerThumbnail : UserControl
    {
        private DispatcherTimer myTimer;
        public ScrollViewerThumbnail()
        {
            InitializeComponent();

            myHighlight.DragDelta += OnDragDelta;

            Loaded += OnLoaded;
        }

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            // Binding does not work :(
            myVisualBrush.Visual = ( Visual )ScrollViewer.Content;

            // Binding does not work :(
            myHighlight.Width = ScrollViewer.ViewportWidth;
            myHighlight.Height = ScrollViewer.ViewportHeight;

            ScrollViewer.ScrollChanged += OnScrollChanged;

            myTimer = new DispatcherTimer();
            myTimer.Tick += OnElapsed;
            myTimer.Interval = TimeSpan.FromMilliseconds( 500 );
            myTimer.Start();
        }
        
        private void OnScrollChanged( object sender, ScrollChangedEventArgs e )
        {
            // Binding does not work :(
            Transform.X = ScrollViewer.HorizontalOffset;
            Transform.Y = ScrollViewer.VerticalOffset;
        }

        private void OnElapsed( object sender, System.EventArgs e )
        {
            var canvas = ( ( FrameworkElement )ScrollViewer.Content );

            // actualwidth and height are no dependency properties and we have no other update trigger yet
            myRect.Width = canvas.ActualWidth;
            myRect.Height = canvas.ActualHeight;
        }

        private void OnDragDelta( object sender, DragDeltaEventArgs e )
        {
            ScrollViewer.ScrollToVerticalOffset( ScrollViewer.VerticalOffset + e.VerticalChange );
            ScrollViewer.ScrollToHorizontalOffset( ScrollViewer.HorizontalOffset + e.HorizontalChange );
        }

        public ScrollViewer ScrollViewer
        {
            get { return ( ScrollViewer )GetValue( ScrollViewerProperty ); }
            set { SetValue( ScrollViewerProperty, value ); }
        }

        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register( "ScrollViewer",
            typeof( ScrollViewer ), typeof( ScrollViewerThumbnail ), new UIPropertyMetadata( null ) );

        public Brush HighlightFill
        {
            get { return ( Brush )GetValue( HighlightFillProperty ); }
            set { SetValue( HighlightFillProperty, value ); }
        }

        public static readonly DependencyProperty HighlightFillProperty =
            DependencyProperty.Register( "HighlightFill",
                typeof( Brush ),
                typeof( ScrollViewerThumbnail ),
                new UIPropertyMetadata( new SolidColorBrush( Color.FromArgb( 128, 255, 255, 0 ) ) ) );

    }
}
