using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Designer
{
    /// <summary>
    /// Used to create a new connection
    /// </summary>
    public class ConnectorAdorner : Adorner
    {
        private PathGeometry myPathGeometry;
        private DesignerCanvas myDesignerCanvas;
        private Connector mySourceConnector;
        private Pen myDrawingPen;
        private DesignerItem myHitDesignerItem;
        private Connector myHitConnector;

        public ConnectorAdorner( DesignerCanvas designer, Connector sourceConnector )
            : base( designer )
        {
            myDesignerCanvas = designer;
            mySourceConnector = sourceConnector;
            myDrawingPen = new Pen( Brushes.LightSlateGray, 1 );
            myDrawingPen.LineJoin = PenLineJoin.Round;
            Cursor = Cursors.Cross;
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
                if ( myHitConnector != value )
                {
                    myHitConnector = value;
                }
            }
        }


        protected override void OnMouseUp( MouseButtonEventArgs e )
        {
            if ( HitConnector != null )
            {
                var newConnection = new Connection( mySourceConnector, HitConnector );
                newConnection.SinkArrowSymbol = myDesignerCanvas.ArrowStyle;

                Canvas.SetZIndex( newConnection, myDesignerCanvas.Children.Count );
                myDesignerCanvas.Children.Add( newConnection );
            }

            if ( HitDesignerItem != null )
            {
                HitDesignerItem.IsDragConnectionOver = false;
            }

            if ( IsMouseCaptured )
            {
                ReleaseMouseCapture();
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer( myDesignerCanvas );
            if ( adornerLayer != null )
            {
                adornerLayer.Remove( this );
            }
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            if ( e.LeftButton == MouseButtonState.Pressed )
            {
                if ( !IsMouseCaptured )
                {
                    CaptureMouse();
                }

                HitTesting( e.GetPosition( this ) );

                myPathGeometry = GetPathGeometry( e.GetPosition( this ) );

                InvalidateVisual();
            }
            else
            {
                if ( this.IsMouseCaptured )
                {
                    ReleaseMouseCapture();
                }
            }
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            dc.DrawGeometry( null, myDrawingPen, this.myPathGeometry );

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle( Brushes.Transparent, null, new Rect( RenderSize ) );
        }

        private PathGeometry GetPathGeometry( Point position )
        {
            PathGeometry geometry = new PathGeometry();

            ConnectorOrientation targetOrientation;
            if ( HitConnector != null )
            {
                targetOrientation = HitConnector.Orientation;
            }
            else
            {
                targetOrientation = ConnectorOrientation.None;
            }

            var pathPoints = PathFinder.GetConnectionLine( mySourceConnector.GetInfo(), position, targetOrientation );

            if ( pathPoints.Count > 0 )
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = pathPoints[ 0 ];
                pathPoints.Remove( pathPoints[ 0 ] );
                figure.Segments.Add( new PolyLineSegment( pathPoints, true ) );
                geometry.Figures.Add( figure );
            }

            return geometry;
        }

        private void HitTesting( Point hitPoint )
        {
            bool hitConnectorFlag = false;
            
            var hitObject = myDesignerCanvas.InputHitTest( hitPoint ) as DependencyObject;

            while ( hitObject != null &&
                   hitObject != mySourceConnector.ParentDesignerItem &&
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
