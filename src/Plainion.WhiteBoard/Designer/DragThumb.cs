using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Plainion.WhiteBoard.Designer
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            DragDelta += OnDragDelta;
        }

        void OnDragDelta( object sender, DragDeltaEventArgs e )
        {
            var designerItem = DataContext as DesignerItem;
            var designer = VisualTreeHelper.GetParent( designerItem ) as DesignerCanvas;

            if ( designerItem == null || designer == null || !designerItem.IsSelected )
            {
                return;
            }

            double minLeft = double.MaxValue;
            double minTop = double.MaxValue;

            // we only move DesignerItems
            var designerItems = designer.SelectionService.CurrentSelection
                .OfType<DesignerItem>();

            foreach ( var item in designerItems )
            {
                double left = Canvas.GetLeft( item );
                double top = Canvas.GetTop( item );

                minLeft = double.IsNaN( left ) ? 0 : Math.Min( left, minLeft );
                minTop = double.IsNaN( top ) ? 0 : Math.Min( top, minTop );
            }

            double deltaHorizontal = Math.Max( -minLeft, e.HorizontalChange );
            double deltaVertical = Math.Max( -minTop, e.VerticalChange );

            foreach ( var item in designerItems )
            {
                double left = Canvas.GetLeft( item );
                double top = Canvas.GetTop( item );

                if ( double.IsNaN( left ) )
                {
                    left = 0;
                }

                if ( double.IsNaN( top ) )
                {
                    top = 0;
                }

                Canvas.SetLeft( item, left + deltaHorizontal );
                Canvas.SetTop( item, top + deltaVertical );
            }

            designer.InvalidateMeasure();

            e.Handled = true;
        }
    }
}
