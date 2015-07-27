using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Plainion.WhiteBoard.Designer
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += OnDragDelta;
        }

        void OnDragDelta( object sender, DragDeltaEventArgs e )
        {
            var designerItem = this.DataContext as DesignerItem;
            var designer = VisualTreeHelper.GetParent( designerItem ) as DesignerCanvas;

            if ( designerItem == null || designer == null || !designerItem.IsSelected )
            {
                return;
            }

            double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
            double dragDeltaVertical, dragDeltaHorizontal, scale;

            var selectedDesignerItems = designer.SelectionService.CurrentSelection
                .OfType<DesignerItem>();

            CalculateDragLimits( selectedDesignerItems, out minLeft, out minTop, out minDeltaHorizontal, out minDeltaVertical );

            foreach ( var item in selectedDesignerItems )
            {
                if ( item != null && item.ParentID == Guid.Empty )
                {
                    switch ( base.VerticalAlignment )
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min( -e.VerticalChange, minDeltaVertical );
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragBottom( scale, item, designer);
                            break;
                        case VerticalAlignment.Top:
                            double top = Canvas.GetTop( item );
                            dragDeltaVertical = Math.Min( Math.Max( -minTop, e.VerticalChange ), minDeltaVertical );
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragTop( scale, item, designer );
                            break;
                        default:
                            break;
                    }

                    switch ( base.HorizontalAlignment )
                    {
                        case HorizontalAlignment.Left:
                            double left = Canvas.GetLeft( item );
                            dragDeltaHorizontal = Math.Min( Math.Max( -minLeft, e.HorizontalChange ), minDeltaHorizontal );
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragLeft( scale, item, designer );
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min( -e.HorizontalChange, minDeltaHorizontal );
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragRight( scale, item, designer );
                            break;
                        default:
                            break;
                    }
                }
            }
            e.Handled = true;
        }

        private void DragLeft( double scale, DesignerItem item, DesignerCanvas canvas )
        {
            var groupItems = canvas.SelectionService.GetGroupMembers( canvas, item ).Cast<DesignerItem>();

            double groupLeft = Canvas.GetLeft( item ) + item.Width;
            foreach ( var groupItem in groupItems )
            {
                double groupItemLeft = Canvas.GetLeft( groupItem );
                double delta = (groupLeft - groupItemLeft) * (scale - 1);
                Canvas.SetLeft( groupItem, groupItemLeft - delta );
                groupItem.Width = groupItem.ActualWidth * scale;
            }
        }

        private void DragTop( double scale, DesignerItem item, DesignerCanvas canvas )
        {
            var groupItems = canvas.SelectionService.GetGroupMembers( canvas, item ).Cast<DesignerItem>();

            double groupBottom = Canvas.GetTop( item ) + item.Height;
            foreach ( var groupItem in groupItems )
            {
                double groupItemTop = Canvas.GetTop( groupItem );
                double delta = (groupBottom - groupItemTop) * (scale - 1);
                Canvas.SetTop( groupItem, groupItemTop - delta );
                groupItem.Height = groupItem.ActualHeight * scale;
            }
        }

        private void DragRight( double scale, DesignerItem item, DesignerCanvas canvas )
        {
            var groupItems = canvas.SelectionService.GetGroupMembers( canvas, item ).Cast<DesignerItem>();

            double groupLeft = Canvas.GetLeft( item );
            foreach ( var groupItem in groupItems )
            {
                double groupItemLeft = Canvas.GetLeft( groupItem );
                double delta = (groupItemLeft - groupLeft) * (scale - 1);

                Canvas.SetLeft( groupItem, groupItemLeft + delta );
                groupItem.Width = groupItem.ActualWidth * scale;
            }
        }

        private void DragBottom( double scale, DesignerItem item, DesignerCanvas canvas )
        {
            IEnumerable<DesignerItem> groupItems = canvas.SelectionService.GetGroupMembers( canvas, item ).Cast<DesignerItem>();
            double groupTop = Canvas.GetTop( item );
            foreach ( DesignerItem groupItem in groupItems )
            {
                double groupItemTop = Canvas.GetTop( groupItem );
                double delta = (groupItemTop - groupTop) * (scale - 1);

                Canvas.SetTop( groupItem, groupItemTop + delta );
                groupItem.Height = groupItem.ActualHeight * scale;
            }
        }

        private void CalculateDragLimits( IEnumerable<DesignerItem> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical )
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach ( var item in selectedItems )
            {
                double left = Canvas.GetLeft( item );
                double top = Canvas.GetTop( item );

                minLeft = double.IsNaN( left ) ? 0 : Math.Min( left, minLeft );
                minTop = double.IsNaN( top ) ? 0 : Math.Min( top, minTop );

                minDeltaVertical = Math.Min( minDeltaVertical, item.ActualHeight - item.MinHeight );
                minDeltaHorizontal = Math.Min( minDeltaHorizontal, item.ActualWidth - item.MinWidth );
            }
        }
    }
}
