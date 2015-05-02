using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Plainion.WhiteBoard.Controls
{
    class BrushPicker : ColorPicker
    {
        private bool mySilent;

        public BrushPicker()
        {
            var selectedColorDescriptor = DependencyPropertyDescriptor.FromProperty( ColorPicker.SelectedColorProperty, typeof( ColorPicker ) );
            selectedColorDescriptor.AddValueChanged( this, OnSelectedColorChanged );

            mySilent = false;
        }

        private void OnSelectedColorChanged( object sender, EventArgs e )
        {
            WithSilentGuard( () => SelectedBrush = new SolidColorBrush( SelectedColor ) );
        }

        private void WithSilentGuard( Action action )
        {
            if( mySilent )
            {
                return;
            }

            mySilent = true;

            action();

            mySilent = false;
        }

        public SolidColorBrush SelectedBrush
        {
            get { return ( SolidColorBrush )GetValue( SelectedBrushProperty ); }
            set { SetValue( SelectedBrushProperty, value ); }
        }

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register( "SelectedBrush", typeof( SolidColorBrush ),
            typeof( BrushPicker ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( OnSelectedBrushChanged ) ) );

        private static void OnSelectedBrushChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var picker = ( BrushPicker )d;
            if( picker.SelectedBrush != null )
            {
                picker.WithSilentGuard( () => picker.SelectedColor = picker.SelectedBrush.Color );
            }
        }

    }
}
