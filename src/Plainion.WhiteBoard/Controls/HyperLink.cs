using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plainion.WhiteBoard.Controls
{
    public class HyperLink : TextBlock
    {
        protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            Uri url;

            if( Uri.TryCreate( this.Text, UriKind.Absolute, out url ) )
            {
                e.Handled = true;
                Process.Start( url.ToString() );
            }
        }
    }
}
