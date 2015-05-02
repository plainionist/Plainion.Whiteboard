using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Designer
{
    [TypeDescriptionProvider( typeof( ItemPropertiesProvider ) )]
    public class Connection : ItemContent, ISelectable, INotifyPropertyChanged
    {
        private Adorner myConnectionAdorner;
        private Connector mySource;
        private Connector mySink;
        private PathGeometry myPathGeometry;
        private Point myAnchorPositionSource;
        private double myAnchorAngleSource = 0;
        private Point myAnchorPositionSink;
        private double myAnchorAngleSink = 0;
        private ArrowSymbol mySourceArrowSymbol = ArrowSymbol.None;
        private ArrowSymbol mySinkArrowSymbol = ArrowSymbol.Arrow;
        private Point myLabelPosition;
        private DoubleCollection myStrokeDashArray;
        private bool myIsSelected;

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached( "Caption", typeof( string ), typeof( Connection ) );

        public string Caption
        {
            get
            {
                return (string)GetValue( CaptionProperty );
            }
            set
            {
                SetValue( CaptionProperty, value );
            }
        }

        public Connection( Connector source, Connector sink )
        {
            if ( source == null )
            {
                throw new ArgumentNullException( "source" );
            }

            if ( sink == null )
            {
                throw new ArgumentNullException( "sink" );
            }

            ID = Guid.NewGuid();
            Source = source;
            Sink = sink;

            Unloaded += OnUnloaded;
        }

        // we could use DependencyProperties as well to inform others of property changes
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid ID
        {
            get;
            private set;
        }

        public Connector Source
        {
            get
            {
                return mySource;
            }
            set
            {
                if ( mySource == value )
                {
                    return;
                }

                if ( mySource != null )
                {
                    mySource.PropertyChanged -= OnConnectorPositionChanged;
                    mySource.Connections.Remove( this );
                }

                mySource = value;

                if ( mySource != null )
                {
                    mySource.Connections.Add( this );
                    mySource.PropertyChanged += OnConnectorPositionChanged;
                }

                UpdatePathGeometry();
            }
        }

        public Connector Sink
        {
            get
            {
                return mySink;
            }
            set
            {
                if ( mySink == value )
                {
                    return;
                }

                if ( mySink != null )
                {
                    mySink.PropertyChanged -= OnConnectorPositionChanged;
                    mySink.Connections.Remove( this );
                }

                mySink = value;

                if ( mySink != null )
                {
                    mySink.Connections.Add( this );
                    mySink.PropertyChanged += OnConnectorPositionChanged;
                }

                UpdatePathGeometry();
            }
        }

        public PathGeometry PathGeometry
        {
            get
            {
                return myPathGeometry;
            }
            set
            {
                if ( myPathGeometry == value )
                {
                    return;
                }

                myPathGeometry = value;

                UpdateAnchorPosition();

                OnPropertyChanged( "PathGeometry" );
            }
        }

        /// <summary>
        /// between source connector position and the beginning 
        /// of the path geometry we leave some space for visual reasons; 
        /// so the anchor position source really marks the beginning 
        /// of the path geometry on the source side
        /// </summary>
        public Point AnchorPositionSource
        {
            get
            {
                return myAnchorPositionSource;
            }
            set
            {
                if ( myAnchorPositionSource == value )
                {
                    return;
                }

                myAnchorPositionSource = value;

                OnPropertyChanged( "AnchorPositionSource" );
            }
        }

        /// <summary>
        /// slope of the path at the anchor position
        /// needed for the rotation angle of the arrow
        /// </summary>
        public double AnchorAngleSource
        {
            get
            {
                return myAnchorAngleSource;
            }
            set
            {
                if ( myAnchorAngleSource == value )
                {
                    return;
                }

                myAnchorAngleSource = value;

                OnPropertyChanged( "AnchorAngleSource" );
            }
        }

        /// <summary>
        /// nalogue to source side
        /// </summary>
        public Point AnchorPositionSink
        {
            get
            {
                return myAnchorPositionSink;
            }
            set
            {
                if ( myAnchorPositionSink == value )
                {
                    return;
                }

                myAnchorPositionSink = value;

                OnPropertyChanged( "AnchorPositionSink" );
            }
        }

        /// <summary>
        /// analogue to source side
        /// </summary>
        public double AnchorAngleSink
        {
            get
            {
                return myAnchorAngleSink;
            }
            set
            {
                if ( myAnchorAngleSink == value )
                {
                    return;
                }

                myAnchorAngleSink = value;

                OnPropertyChanged( "AnchorAngleSink" );
            }
        }

        public ArrowSymbol SourceArrowSymbol
        {
            get
            {
                return mySourceArrowSymbol;
            }
            set
            {
                if ( mySourceArrowSymbol == value )
                {
                    return;
                }

                mySourceArrowSymbol = value;

                OnPropertyChanged( "SourceArrowSymbol" );
            }
        }

        public ArrowSymbol SinkArrowSymbol
        {
            get
            {
                return mySinkArrowSymbol;
            }
            set
            {
                if ( mySinkArrowSymbol == value )
                {
                    return;
                }

                mySinkArrowSymbol = value;

                OnPropertyChanged( "SinkArrowSymbol" );
            }
        }

        /// <summary>
        /// specifies a point at half path length
        /// </summary>
        public Point LabelPosition
        {
            get
            {
                return myLabelPosition;
            }
            set
            {
                if ( myLabelPosition == value )
                {
                    return;
                }

                myLabelPosition = value;

                OnPropertyChanged( "LabelPosition" );
            }
        }

        /// <summary>
        /// pattern of dashes and gaps that is used to outline the connection path
        /// </summary>
        public DoubleCollection StrokeDashArray
        {
            get
            {
                return myStrokeDashArray;
            }
            set
            {
                if ( myStrokeDashArray == value )
                {
                    return;
                }

                myStrokeDashArray = value;

                OnPropertyChanged( "StrokeDashArray" );
            }
        }

        /// <summary>
        /// if connected, the ConnectionAdorner becomes visible
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return myIsSelected;
            }
            set
            {
                if ( myIsSelected == value )
                {
                    return;
                }

                myIsSelected = value;

                OnPropertyChanged( "IsSelected" );

                if ( myIsSelected )
                {
                    ShowAdorner();
                }
                else
                {
                    HideAdorner();
                }
            }
        }

        protected override void OnPreviewMouseDown( MouseButtonEventArgs eventArgs )
        {
            base.OnPreviewMouseDown( eventArgs );

            eventArgs.Handled = false;

            var designer = VisualTreeHelper.GetParent( this ) as DesignerCanvas;
            if ( designer == null )
            {
                return;
            }

            if ( (Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None )
            {
                if ( IsSelected )
                {
                    designer.SelectionService.RemoveFromSelection( designer, this );
                }
                else
                {
                    designer.SelectionService.AddToSelection( designer, this );
                }
            }
            else if ( !IsSelected )
            {
                designer.SelectionService.SelectItem( designer, this );
            }

            // dont set focus here - handled in "IsSelected" because of the adorner
            //Focus();
        }

        private void OnConnectorPositionChanged( object sender, PropertyChangedEventArgs e )
        {
            // whenever the 'Position' property of the source or sink Connector 
            // changes we must update the connection path geometry
            if ( e.PropertyName.Equals( "Position" ) )
            {
                UpdatePathGeometry();
            }
        }

        private void UpdatePathGeometry()
        {
            if ( Source == null || Sink == null )
            {
                return;
            }

            var linePoints = PathFinder.GetConnectionLine( Source.GetInfo(), Sink.GetInfo(), true );
            if ( linePoints.Count > 0 )
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = linePoints[ 0 ];
                linePoints.Remove( linePoints[ 0 ] );
                figure.Segments.Add( new PolyLineSegment( linePoints, true ) );

                var geometry = new PathGeometry();
                geometry.Figures.Add( figure );

                PathGeometry = geometry;
            }
        }

        private void UpdateAnchorPosition()
        {
            Point pathStartPoint;
            Point pathTangentAtStartPoint;
            Point pathEndPoint;
            Point pathTangentAtEndPoint;
            Point pathMidPoint;
            Point pathTangentAtMidPoint;

            // the PathGeometry.GetPointAtFractionLength method gets the point and a tangent vector 
            // on PathGeometry at the specified fraction of its length
            PathGeometry.GetPointAtFractionLength( 0, out pathStartPoint, out pathTangentAtStartPoint );
            PathGeometry.GetPointAtFractionLength( 1, out pathEndPoint, out pathTangentAtEndPoint );
            PathGeometry.GetPointAtFractionLength( 0.5, out pathMidPoint, out pathTangentAtMidPoint );

            // get angle from tangent vector
            AnchorAngleSource = Math.Atan2( -pathTangentAtStartPoint.Y, -pathTangentAtStartPoint.X ) * (180 / Math.PI);
            AnchorAngleSink = Math.Atan2( pathTangentAtEndPoint.Y, pathTangentAtEndPoint.X ) * (180 / Math.PI);

            // add some margin on source and sink side for visual reasons only
            //pathStartPoint.Offset( -pathTangentAtStartPoint.X * 5, -pathTangentAtStartPoint.Y * 5 );
            //pathEndPoint.Offset( pathTangentAtEndPoint.X * 5, pathTangentAtEndPoint.Y * 5 );

            AnchorPositionSource = pathStartPoint;
            AnchorPositionSink = pathEndPoint;
            LabelPosition = pathMidPoint;
        }

        private void ShowAdorner()
        {
            // the ConnectionAdorner is created once for each Connection
            if ( myConnectionAdorner == null )
            {
                var designer = VisualTreeHelper.GetParent( this ) as DesignerCanvas;

                var adornerLayer = AdornerLayer.GetAdornerLayer( this );
                if ( adornerLayer != null )
                {
                    myConnectionAdorner = new ConnectionAdorner( designer, this );
                    adornerLayer.Add( myConnectionAdorner );
                }
            }

            myConnectionAdorner.Visibility = Visibility.Visible;
        }

        private void HideAdorner()
        {
            if ( myConnectionAdorner != null )
            {
                myConnectionAdorner.Visibility = Visibility.Collapsed;
            }
        }

        private void OnUnloaded( object sender, RoutedEventArgs e )
        {
            // do some housekeeping when Connection is unloaded

            // remove event handler
            Source = null;
            Sink = null;

            // remove adorner
            if ( myConnectionAdorner != null )
            {
                var designer = VisualTreeHelper.GetParent( this ) as DesignerCanvas;

                var adornerLayer = AdornerLayer.GetAdornerLayer( this );
                if ( adornerLayer != null )
                {
                    adornerLayer.Remove( myConnectionAdorner );
                    myConnectionAdorner = null;
                }
            }
        }

        private void OnPropertyChanged( string name )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( name ) );
            }
        }

        /// <summary>
        /// Accessor for properties
        /// </summary>
        public bool IsDotted
        {
            get
            {
                return myStrokeDashArray != null && myStrokeDashArray.Count > 0;
            }
            set
            {
                if ( value )
                {
                    StrokeDashArray = new DoubleCollection( new double[] { 3, 5 } );
                }
                else
                {
                    StrokeDashArray = null;
                }
            }
        }
    }
}
