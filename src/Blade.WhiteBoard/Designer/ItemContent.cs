using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace Plainion.WhiteBoard.Designer
{
    public class ItemContent : ContentControl
    {
        public static readonly DependencyProperty PropertiesProperty;

        static ItemContent()
        {
            PropertiesProperty = DependencyProperty.Register( "Properties", typeof( ItemPropertyCollection ), typeof( ItemContent ) );
        }

        // seems that we null the properties here somehow :(
        //public ItemContent()
        //{
        //    Properties = new ItemPropertyCollection();
        //}

        public ItemPropertyCollection Properties
        {
            get
            {
                return (ItemPropertyCollection)GetValue( PropertiesProperty );
            }
            set
            {
                if ( value == null )
                {
                    SetValue( PropertiesProperty, new ItemPropertyCollection() );
                }
                else
                {
                    SetValue( PropertiesProperty, value );
                }
            }
        }

        internal string Save()
        {
            var container = new ItemContent();
            var innerContent = Content;

            try
            {
                Content = null;

                container.Content = innerContent;
                container.Properties = Properties;

                return XamlWriter.Save( container );
            }
            finally
            {
                container.Content = null;
                container.Properties = null;

                Content = innerContent;
            }
        }
    }
}
