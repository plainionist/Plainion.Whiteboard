using System.Windows;
using System.Windows.Controls;

namespace Plainion.WhiteBoard.Toolbox
{
    /// <summary>
    /// Implements ItemsControl for ToolboxItems 
    /// </summary>
    public class Toolbox : ItemsControl
    {
        public Toolbox()
        {
            ItemSize = new Size( 50, 50 );
        }

        /// <summary>
        /// Defines the ItemHeight and ItemWidth properties of
        /// the WrapPanel used for this Toolbox
        /// </summary>
        public Size ItemSize
        {
            get;
            set;
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.      
        /// </summary>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolboxItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.       
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride( object item )
        {
            return (item is ToolboxItem);
        }
    }
}
