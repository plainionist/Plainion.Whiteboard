using System.Windows;
using System.Windows.Input;

namespace Plainion.WhiteBoard
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            if( Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4 )
            {
                // we allow close only via the toolbar buttons in order to have control over save or discard on close
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown( e );
            }
        }
    }
}
