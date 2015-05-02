using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Plainion.WhiteBoard.Controls
{
    public class SelectionChangedBehaviour
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached( "Command", typeof( ICommand ), typeof( SelectionChangedBehaviour ), new PropertyMetadata( PropertyChangedCallback ) );

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.RegisterAttached( "CommandTarget", typeof( IInputElement ), typeof( SelectionChangedBehaviour ) );

        public static void PropertyChangedCallback( DependencyObject depObj, DependencyPropertyChangedEventArgs args )
        {
            var selector = (Selector)depObj;
            if ( selector != null )
            {
                selector.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var selector = (Selector)sender;

            var command = selector.GetValue( CommandProperty ) as ICommand;
            if ( command == null )
            {
                throw new InvalidOperationException( "Command does not implement ICommand" );
            }

            var routedCommand = command as RoutedCommand;
            if ( routedCommand != null )
            {
                var target = (IInputElement)selector.GetValue( CommandTargetProperty );
                routedCommand.Execute( selector.SelectedItem, target );
            }
            else
            {
                command.Execute( selector.SelectedItem );
            }
        }

        public static ICommand GetCommand( UIElement element )
        {
            return (ICommand)element.GetValue( CommandProperty );
        }

        public static void SetCommand( UIElement element, ICommand command )
        {
            element.SetValue( CommandProperty, command );
        }

        public static IInputElement GetCommandTarget( UIElement element )
        {
            return (IInputElement)element.GetValue( CommandTargetProperty );
        }

        public static void SetCommandTarget( UIElement element, IInputElement target )
        {
            element.SetValue( CommandTargetProperty, target );
        }
    }
}
