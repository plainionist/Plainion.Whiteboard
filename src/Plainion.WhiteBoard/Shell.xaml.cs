using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;

namespace Plainion.WhiteBoard
{
    [Export]
    public partial class Shell : Window
    {
        public Shell()
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
