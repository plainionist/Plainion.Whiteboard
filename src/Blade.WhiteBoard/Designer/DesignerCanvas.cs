using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using Plainion.WhiteBoard.Model;
using Plainion.WhiteBoard.Services;

namespace Plainion.WhiteBoard.Designer
{
    public partial class DesignerCanvas : Canvas, ICanvasModel
    {
        internal const int Margin_Width= 500;
        internal const int Margin_Height = 500;

        private Point? myRubberbandSelectionStartPoint;

        [Import( typeof( SelectionService ) )]
        internal SelectionService SelectionService
        {
            get;
            private set;
        }

        [Import( typeof( PersistenceService ) )]
        internal PersistenceService PersistenceService
        {
            get;
            private set;
        }

        protected override void OnMouseDown( MouseButtonEventArgs eventArgs )
        {
            base.OnMouseDown( eventArgs );

            if( eventArgs.Source != this )
            {
                return;
            }

            if( Keyboard.Modifiers != ModifierKeys.Control )
            {
                SelectionService.ClearSelection();
                return;
            }

            // in case that this click is the start of a 
            // drag operation we cache the start point
            myRubberbandSelectionStartPoint = new Point?( eventArgs.GetPosition( this ) );

            SelectionService.ClearSelection();

            Focus();

            eventArgs.Handled = true;
        }

        protected override void OnMouseMove( MouseEventArgs eventArgs )
        {
            base.OnMouseMove( eventArgs );

            if( Keyboard.Modifiers != ModifierKeys.Control )
            {
                return;
            }

            // if mouse button is not pressed we have no drag operation, ...
            if( eventArgs.LeftButton != MouseButtonState.Pressed )
            {
                this.myRubberbandSelectionStartPoint = null;
            }

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if( this.myRubberbandSelectionStartPoint.HasValue )
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );
                if( adornerLayer != null )
                {
                    var adorner = new RubberbandAdorner( this, myRubberbandSelectionStartPoint );
                    adornerLayer.Add( adorner );
                }
            }

            eventArgs.Handled = true;
        }

        protected override void OnDrop( DragEventArgs eventArgs )
        {
            base.OnDrop( eventArgs );

            var dragObject = eventArgs.Data.GetData( typeof( ItemContentTemplate ) ) as ItemContentTemplate;

            if( dragObject == null || string.IsNullOrEmpty( dragObject.ContentXaml ) )
            {
                return;
            }

            var content = XamlReader.Load( XmlReader.Create( new StringReader( dragObject.ContentXaml ) ) );

            var newItem = new DesignerItem();
            newItem.Content = content;

            Point position = eventArgs.GetPosition( this );

            if( dragObject.DesiredSize.HasValue )
            {
                Size desiredSize = dragObject.DesiredSize.Value;
                newItem.Width = desiredSize.Width;
                newItem.Height = desiredSize.Height;

                DesignerCanvas.SetLeft( newItem, Math.Max( 0, position.X - newItem.Width / 2 ) );
                DesignerCanvas.SetTop( newItem, Math.Max( 0, position.Y - newItem.Height / 2 ) );
            }
            else
            {
                DesignerCanvas.SetLeft( newItem, Math.Max( 0, position.X ) );
                DesignerCanvas.SetTop( newItem, Math.Max( 0, position.Y ) );
            }

            Canvas.SetZIndex( newItem, Children.Count );

            Children.Add( newItem );

            newItem.SetConnectorDecoratorTemplate();

            SelectionService.SelectItem( this, newItem );

            newItem.Focus();

            eventArgs.Handled = true;
        }

        protected override Size MeasureOverride( Size constraint )
        {
            Size size = new Size();

            foreach( UIElement element in InternalChildren )
            {
                double left = Canvas.GetLeft( element );
                double top = Canvas.GetTop( element );
                left = double.IsNaN( left ) ? 0 : left;
                top = double.IsNaN( top ) ? 0 : top;

                //measure desired size for each child
                element.Measure( constraint );

                Size desiredSize = element.DesiredSize;
                if( !double.IsNaN( desiredSize.Width ) && !double.IsNaN( desiredSize.Height ) )
                {
                    size.Width = Math.Max( size.Width, left + desiredSize.Width );
                    size.Height = Math.Max( size.Height, top + desiredSize.Height );
                }
            }

            // add margin 
            size.Width += Margin_Width;
            size.Height += Margin_Height;

            return size;
        }

        public IEnumerable<T> GetItems<T>()
        {
            return Children.OfType<T>().ToList();
        }
    }
}
