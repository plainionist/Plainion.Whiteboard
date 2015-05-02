using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Plainion.WhiteBoard.Designer
{
    public class SavedFeedbackAdorner : Adorner
    {
        public SavedFeedbackAdorner( UIElement owner )
            : base( owner )
        {
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            var text = new FormattedText(
                "Saved",
                Thread.CurrentThread.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface( "Verdana" ),
                150,
                Brushes.Gray );
            text.TextAlignment = TextAlignment.Center;

            dc.DrawText( text, new Point( AdornedElement.RenderSize.Width / 2, AdornedElement.RenderSize.Height / 2 - text.Height / 2 ) );
        }
    }
}
