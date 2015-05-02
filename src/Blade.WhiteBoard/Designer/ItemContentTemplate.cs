using System;
using System.Windows;

namespace Plainion.WhiteBoard.Designer
{
    /// <summary>
    /// Contains the template for a canvas item in XAML format.
    /// </summary>
    public class ItemContentTemplate
    {
        private ItemContentTemplate( string xaml )
        {
            if ( string.IsNullOrEmpty( xaml ) )
            {
                throw new ArgumentNullException( "xaml" );
            }

            ContentXaml = xaml;
        }

        public string ContentXaml
        {
            get;
            private set;
        }

        public Size? DesiredSize
        {
            get;
            set;
        }

        public static ItemContentTemplate Create( ItemContent template )
        {
            var blob = template.Save();
            return new ItemContentTemplate( blob );
        }
    }
}
