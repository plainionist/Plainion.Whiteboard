using System;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Plainion.WhiteBoard.Controls
{
    [MarkupExtensionReturnType( typeof( Geometry ) )]
    public class TextGeometryExtension : MarkupExtension
    {
        public string Value
        {
            get;
            set;
        }

        public Point StartPoint
        {
            get;
            set;
        }

        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            var text = new FormattedText(
                Value,
                Thread.CurrentThread.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface( "Verdana" ),
                8,
                Brushes.Black );

            var geometry = text.BuildGeometry( StartPoint );

            return geometry;
        }
    }
}
