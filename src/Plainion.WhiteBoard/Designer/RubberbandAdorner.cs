using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;

namespace Plainion.WhiteBoard.Designer
{
    public class RubberbandAdorner : Adorner
    {
        private Point? myStartPoint;
        private Point? myEndPoint;
        private Pen myRubberbandPen;
        private DesignerCanvas myDesignerCanvas;

        public RubberbandAdorner( DesignerCanvas designerCanvas, Point? dragStartPoint )
            : base( designerCanvas )
        {
            myDesignerCanvas = designerCanvas;
            myStartPoint = dragStartPoint;

            myRubberbandPen = new Pen( Brushes.LightSlateGray, 1 );
            myRubberbandPen.DashStyle = new DashStyle( new double[] { 2 }, 1 );
        }

        protected override void OnMouseMove( MouseEventArgs eventArgs )
        {
            if ( eventArgs.LeftButton == MouseButtonState.Pressed )
            {
                if ( !this.IsMouseCaptured )
                {
                    this.CaptureMouse();
                }

                myEndPoint = eventArgs.GetPosition( this );
                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if ( this.IsMouseCaptured )
                {
                    this.ReleaseMouseCapture();
                }
            }

            eventArgs.Handled = true;
        }

        protected override void OnMouseUp( MouseButtonEventArgs eventArgs )
        {
            if ( IsMouseCaptured )
            {
                ReleaseMouseCapture();
            }

            // remove this adorner from adorner layer
            var adornerLayer = AdornerLayer.GetAdornerLayer( myDesignerCanvas );
            if ( adornerLayer != null )
            {
                adornerLayer.Remove( this );
            }

            eventArgs.Handled = true;
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle( Brushes.Transparent, null, new Rect( RenderSize ) );

            if ( myStartPoint.HasValue && myEndPoint.HasValue )
            {
                dc.DrawRectangle( Brushes.Transparent, myRubberbandPen, new Rect( myStartPoint.Value, myEndPoint.Value ) );
            }
        }

        private void UpdateSelection()
        {
            myDesignerCanvas.SelectionService.ClearSelection();

            Rect rubberBand = new Rect( myStartPoint.Value, myEndPoint.Value );
            foreach ( Control item in myDesignerCanvas.Children )
            {
                Rect itemRect = VisualTreeHelper.GetDescendantBounds( item );
                Rect itemBounds = item.TransformToAncestor( myDesignerCanvas ).TransformBounds( itemRect );

                if ( rubberBand.Contains( itemBounds ) )
                {
                    if ( item is Connection )
                    {
                        myDesignerCanvas.SelectionService.AddToSelection( myDesignerCanvas, item as ISelectable );
                    }
                    else
                    {
                        DesignerItem di = item as DesignerItem;
                        if ( di.ParentID == Guid.Empty )
                        {
                            myDesignerCanvas.SelectionService.AddToSelection( myDesignerCanvas, di );
                        }
                    }
                }
            }
        }
    }
}
