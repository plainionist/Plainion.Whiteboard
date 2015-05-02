using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.WhiteBoard.Model;
using Plainion;

namespace Plainion.WhiteBoard.Designer
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart( Name = "PART_DragThumb", Type = typeof( DragThumb ) )]
    [TemplatePart( Name = "PART_ResizeDecorator", Type = typeof( Control ) )]
    [TemplatePart( Name = "PART_ConnectorDecorator", Type = typeof( Control ) )]
    [TemplatePart( Name = "PART_ContentPresenter", Type = typeof( ContentPresenter ) )]
    [TypeDescriptionProvider( typeof( ItemPropertiesProvider ) )]
    public partial class DesignerItem : ContentControl, ISelectable, IGroupable
    {
        public static readonly DependencyProperty ParentIDProperty = DependencyProperty.Register( "ParentID", typeof( Guid ), typeof( DesignerItem ) );

        public static readonly DependencyProperty IsGroupProperty = DependencyProperty.Register( "IsGroup", typeof( bool ), typeof( DesignerItem ) );

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register( "IsSelected", typeof( bool ), typeof( DesignerItem ), new FrameworkPropertyMetadata( false ) );

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty = DependencyProperty.RegisterAttached( "DragThumbTemplate", typeof( ControlTemplate ), typeof( DesignerItem ) );

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty = DependencyProperty.RegisterAttached( "ConnectorDecoratorTemplate", typeof( ControlTemplate ), typeof( DesignerItem ) );

        public static readonly DependencyProperty IsDragConnectionOverProperty = DependencyProperty.Register( "IsDragConnectionOver", typeof( bool ), typeof( DesignerItem ), new FrameworkPropertyMetadata( false ) );

        static DesignerItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DesignerItem ), new FrameworkPropertyMetadata( typeof( DesignerItem ) ) );
        }

        public DesignerItem( Guid id )
        {
            Contract.RequiresNotNull( id, "id" );

            ID = id;
            Loaded += DesignerItem_Loaded;
        }

        public DesignerItem()
            : this( Guid.NewGuid() )
        {
        }

        public Guid ID
        {
            get;
            private set;
        }

        public Guid ParentID
        {
            get { return ( Guid )GetValue( ParentIDProperty ); }
            set
            {
                Contract.RequiresNotNull( value, "value" );

                SetValue( ParentIDProperty, value );
            }
        }

        public bool IsGroup
        {
            get { return ( bool )GetValue( IsGroupProperty ); }
            set { SetValue( IsGroupProperty, value ); }
        }

        public bool IsSelected
        {
            get { return ( bool )GetValue( IsSelectedProperty ); }
            set { SetValue( IsSelectedProperty, value ); }
        }

        public static ControlTemplate GetDragThumbTemplate( UIElement element )
        {
            return ( ControlTemplate )element.GetValue( DragThumbTemplateProperty );
        }

        public static void SetDragThumbTemplate( UIElement element, ControlTemplate value )
        {
            element.SetValue( DragThumbTemplateProperty, value );
        }

        public static ControlTemplate GetConnectorDecoratorTemplate( UIElement element )
        {
            return ( ControlTemplate )element.GetValue( ConnectorDecoratorTemplateProperty );
        }

        public static void SetConnectorDecoratorTemplate( UIElement element, ControlTemplate value )
        {
            element.SetValue( ConnectorDecoratorTemplateProperty, value );
        }

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return ( bool )GetValue( IsDragConnectionOverProperty ); }
            set { SetValue( IsDragConnectionOverProperty, value ); }
        }

        protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            base.OnPreviewMouseLeftButtonUp( e );

            var contentUI = Content as FrameworkElement;
            if( contentUI != null )
            {
                var hitChild = contentUI.InputHitTest( e.GetPosition( this ) );
                if( hitChild != null )
                {
                    hitChild.RaiseEvent( e );
                    e.Handled = true;
                    return;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            base.OnPreviewMouseLeftButtonDown( e );

            e.Handled = false;

            var designer = VisualTreeHelper.GetParent( this ) as DesignerCanvas;
            if( designer == null )
            {
                return;
            }

            if( ( Keyboard.Modifiers & ( ModifierKeys.Shift | ModifierKeys.Control ) ) != ModifierKeys.None )
            {
                if( IsSelected )
                {
                    designer.SelectionService.RemoveFromSelection( designer, this );
                }
                else
                {
                    designer.SelectionService.AddToSelection( designer, this );
                }
            }
            else if( !IsSelected )
            {
                designer.SelectionService.SelectItem( designer, this );
            }

            Focus();
        }

        void DesignerItem_Loaded( object sender, RoutedEventArgs eventArgs )
        {
            if( base.Template == null )
            {
                return;
            }

            var contentPresenter = Template.FindName( "PART_ContentPresenter", this ) as ContentPresenter;
            if( contentPresenter == null )
            {
                return;
            }

            var contentVisual = VisualTreeHelper.GetChild( contentPresenter, 0 ) as UIElement;
            if( contentVisual == null )
            {
                return;
            }

            var thumb = Template.FindName( "PART_DragThumb", this ) as DragThumb;
            if( thumb == null )
            {
                return;
            }

            var template = DesignerItem.GetDragThumbTemplate( contentVisual ) as ControlTemplate;
            if( template == null )
            {
                return;
            }

            thumb.Template = template;
        }

        public void SetConnectorDecoratorTemplate()
        {
            if( ApplyTemplate() && Content is UIElement )
            {
                ControlTemplate template = DesignerItem.GetConnectorDecoratorTemplate( Content as UIElement );
                Control decorator = Template.FindName( "PART_ConnectorDecorator", this ) as Control;
                if( decorator != null && template != null )
                {
                    decorator.Template = template;
                }
            }

            // TODO: find a better place - we just put it here because it is called after a new instance is created
            // TODO: we need to introduce binding here - with INotifyPropertyChanged
            //var nameBinding = new Binding();
            //nameBinding.Source = TypeDescriptor.GetComponentName( this );
            //SetBinding( ContentControl.NameProperty, nameBinding );
        }
    }
}
