using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Designer;

namespace Plainion.WhiteBoard.Toolbox
{
    /// <summary>
    /// Represents a selectable item in the Toolbox
    /// </summary>
    public class ToolboxItem : ContentControl
    {
        private Point? myDragStartPoint;

        static ToolboxItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( ToolboxItem ), new FrameworkPropertyMetadata( typeof( ToolboxItem ) ) );
        }

        protected override void OnPreviewMouseDown( MouseButtonEventArgs eventArgs )
        {
            base.OnPreviewMouseDown( eventArgs );

            myDragStartPoint = new Point?( eventArgs.GetPosition( this ) );
        }

        protected override void OnMouseMove( MouseEventArgs eventArgs )
        {
            base.OnMouseMove( eventArgs );

            if ( eventArgs.LeftButton != MouseButtonState.Pressed )
            {
                myDragStartPoint = null;
                return;
            }

            if ( !myDragStartPoint.HasValue )
            {
                return;
            }

            if ( Content == null )
            {
                return;
            }

            var dataObject = ItemContentTemplate.Create( (ItemContent)Content );
            var panel = VisualTreeHelper.GetParent( this ) as WrapPanel;
            if ( panel != null )
            {
                // desired size for DesignerCanvas is the stretched Toolbox item size
                const double scale = 1.3;
                dataObject.DesiredSize = new Size( panel.ItemWidth * scale, panel.ItemHeight * scale );
            }

            DragDrop.DoDragDrop( this, dataObject, DragDropEffects.Copy );

            eventArgs.Handled = true;
        }
    }
}
