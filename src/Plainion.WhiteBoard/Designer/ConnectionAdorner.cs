using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Designer
{
    /// <summary>
    /// Applied when the connection is selected. Showing the thumbs etc.
    /// Handles move/drag of connections.
    /// </summary>
    /// <remarks>
    /// This Adorner has to forward events to the adorned element manually in order to allow the application to handle those
    /// (e.g. del key). Reason: this adorner is in different layer but creates a canvas over the entire DesignerCanvas.
    /// </remarks>
    public class ConnectionAdorner : Adorner
    {
        private DesignerCanvas myDesignerCanvas;
        private Canvas myAdornerCanvas;
        private Connection myConnection;
        private PathGeometry myPathGeometry;
        private Connector myFixConnector;
        private Connector myDragConnector;
        private Thumb mySourceDragThumb;
        private Thumb mySinkDragThumb;
        private Pen myDrawingPen;
        private DesignerItem myHitDesignerItem;
        private Connector myHitConnector;
        private VisualCollection myVisualChildren;
        private bool myWasDotted;

        public ConnectionAdorner( DesignerCanvas designer, Connection connection )
            : base( designer )
        {
            myDesignerCanvas = designer;

            myAdornerCanvas = new Canvas();
            myAdornerCanvas.Focusable = true;
            myAdornerCanvas.IsEnabled = true;

            myVisualChildren = new VisualCollection( this );
            myVisualChildren.Add( myAdornerCanvas );

            myConnection = connection;
            myConnection.PropertyChanged += AnchorPositionChanged;

            InitializeDragThumbs();

            myDrawingPen = new Pen( Brushes.LightSlateGray, 1 );
            myDrawingPen.LineJoin = PenLineJoin.Round;

            Unloaded += OnUnload;
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            AdornedElement.RaiseEvent( e );
        }

        protected override void OnKeyUp( KeyEventArgs e )
        {
            AdornedElement.RaiseEvent( e );
        }

        private DesignerItem HitDesignerItem
        {
            get
            {
                return myHitDesignerItem;
            }
            set
            {
                if ( myHitDesignerItem == value )
                {
                    return;
                }

                if ( myHitDesignerItem != null )
                {
                    myHitDesignerItem.IsDragConnectionOver = false;
                }

                myHitDesignerItem = value;

                if ( myHitDesignerItem != null )
                {
                    myHitDesignerItem.IsDragConnectionOver = true;
                }
            }
        }

        private Connector HitConnector
        {
            get
            {
                return myHitConnector;
            }
            set
            {
                if ( myHitConnector == value )
                {
                    return;
                }

                myHitConnector = value;
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return this.myVisualChildren.Count;
            }
        }

        protected override Visual GetVisualChild( int index )
        {
            return myVisualChildren[ index ];
        }

        private void AnchorPositionChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName.Equals( "AnchorPositionSource" ) )
            {
                Canvas.SetLeft( mySourceDragThumb, myConnection.AnchorPositionSource.X );
                Canvas.SetTop( mySourceDragThumb, myConnection.AnchorPositionSource.Y );
            }

            if ( e.PropertyName.Equals( "AnchorPositionSink" ) )
            {
                Canvas.SetLeft( mySinkDragThumb, myConnection.AnchorPositionSink.X );
                Canvas.SetTop( mySinkDragThumb, myConnection.AnchorPositionSink.Y );
            }
        }

        private void OnDragCompleted( object sender, DragCompletedEventArgs e )
        {
            if ( HitConnector != null )
            {
                if ( myConnection != null )
                {
                    if ( myConnection.Source == myFixConnector )
                    {
                        myConnection.Sink = HitConnector;
                    }
                    else
                    {
                        myConnection.Source = HitConnector;
                    }
                }
            }

            HitDesignerItem = null;
            HitConnector = null;
            myPathGeometry = null;

            myConnection.IsDotted = myWasDotted;

            InvalidateVisual();
        }

        private void OnDragStarted( object sender, DragStartedEventArgs e )
        {
            HitDesignerItem = null;
            HitConnector = null;
            myPathGeometry = null;
            Cursor = Cursors.Cross;
            myWasDotted = myConnection.IsDotted;
            myConnection.StrokeDashArray = new DoubleCollection( new double[] { 1, 2 } );

            if ( sender == mySourceDragThumb )
            {
                myFixConnector = myConnection.Sink;
                myDragConnector = myConnection.Source;
            }
            else if ( sender == mySinkDragThumb )
            {
                myDragConnector = myConnection.Sink;
                myFixConnector = myConnection.Source;
            }
        }

        private void OnDragDelta( object sender, DragDeltaEventArgs e )
        {
            Point currentPosition = Mouse.GetPosition( this );

            HitTesting( currentPosition );

            myPathGeometry = UpdatePathGeometry( currentPosition );

            InvalidateVisual();
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            dc.DrawGeometry( null, myDrawingPen, myPathGeometry );
        }

        protected override Size ArrangeOverride( Size finalSize )
        {
            myAdornerCanvas.Arrange( new Rect( 0, 0, myDesignerCanvas.ActualWidth, myDesignerCanvas.ActualHeight ) );

            myAdornerCanvas.Focus();

            //Debug.WriteLine( "IsFocused: " + myAdornerCanvas.IsFocused );
            //Debug.WriteLine( "IsKeyboardFocused: " + myAdornerCanvas.IsKeyboardFocused );
            //Debug.WriteLine( "IsKeyboardFocusWithin: " + myAdornerCanvas.IsKeyboardFocusWithin );

            return finalSize;
        }

        private void OnUnload( object sender, RoutedEventArgs e )
        {
            mySourceDragThumb.DragDelta -= OnDragDelta;
            mySourceDragThumb.DragStarted -= OnDragStarted;
            mySourceDragThumb.DragCompleted -= OnDragCompleted;

            mySinkDragThumb.DragDelta -= OnDragDelta;
            mySinkDragThumb.DragStarted -= OnDragStarted;
            mySinkDragThumb.DragCompleted -= OnDragCompleted;

            Unloaded -= OnUnload;
        }

        private void InitializeDragThumbs()
        {
            Style dragThumbStyle = myConnection.FindResource( "ConnectionAdornerThumbStyle" ) as Style;

            //source drag thumb
            mySourceDragThumb = new Thumb();
            Canvas.SetLeft( mySourceDragThumb, myConnection.AnchorPositionSource.X );
            Canvas.SetTop( mySourceDragThumb, myConnection.AnchorPositionSource.Y );

            myAdornerCanvas.Children.Add( mySourceDragThumb );
            if ( dragThumbStyle != null )
            {
                mySourceDragThumb.Style = dragThumbStyle;
            }

            mySourceDragThumb.DragDelta += OnDragDelta;
            mySourceDragThumb.DragStarted += OnDragStarted;
            mySourceDragThumb.DragCompleted += OnDragCompleted;

            // sink drag thumb
            mySinkDragThumb = new Thumb();
            Canvas.SetLeft( mySinkDragThumb, myConnection.AnchorPositionSink.X );
            Canvas.SetTop( mySinkDragThumb, myConnection.AnchorPositionSink.Y );

            myAdornerCanvas.Children.Add( mySinkDragThumb );
            if ( dragThumbStyle != null )
            {
                mySinkDragThumb.Style = dragThumbStyle;
            }

            mySinkDragThumb.DragDelta += OnDragDelta;
            mySinkDragThumb.DragStarted += OnDragStarted;
            mySinkDragThumb.DragCompleted += OnDragCompleted;
        }

        private PathGeometry UpdatePathGeometry( Point position )
        {
            ConnectorOrientation targetOrientation;
            if ( HitConnector != null )
            {
                targetOrientation = HitConnector.Orientation;
            }
            else
            {
                targetOrientation = myDragConnector.Orientation;
            }

            var geometry = new PathGeometry();

            var linePoints = PathFinder.GetConnectionLine( myFixConnector.GetInfo(), position, targetOrientation );

            if ( linePoints.Count > 0 )
            {
                var figure = new PathFigure();
                figure.StartPoint = linePoints[ 0 ];
                linePoints.Remove( linePoints[ 0 ] );
                figure.Segments.Add( new PolyLineSegment( linePoints, true ) );

                geometry.Figures.Add( figure );
            }

            return geometry;
        }

        private void HitTesting( Point hitPoint )
        {
            bool hitConnectorFlag = false;

            var hitObject = myDesignerCanvas.InputHitTest( hitPoint ) as DependencyObject;

            while ( hitObject != null &&
                   hitObject != myFixConnector.ParentDesignerItem &&
                   hitObject.GetType() != typeof( DesignerCanvas ) )
            {
                if ( hitObject is Connector )
                {
                    HitConnector = hitObject as Connector;
                    hitConnectorFlag = true;
                }

                if ( hitObject is DesignerItem )
                {
                    HitDesignerItem = hitObject as DesignerItem;
                    if ( !hitConnectorFlag )
                    {
                        HitConnector = null;
                    }

                    return;
                }
                hitObject = VisualTreeHelper.GetParent( hitObject );
            }

            HitConnector = null;
            HitDesignerItem = null;
        }
    }
}
