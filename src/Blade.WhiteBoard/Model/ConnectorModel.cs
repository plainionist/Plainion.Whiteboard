using System.Windows;
using Plainion;

namespace Plainion.WhiteBoard.Model
{
    internal class ConnectorModel
    {
        public ConnectorModel( DesignerItemModel ownerItem )
        {
            Contract.RequiresNotNull( ownerItem, "ownerItem" );

            OwnerItem = ownerItem;
        }

        public DesignerItemModel OwnerItem
        {
            get;
            private set;
        }

        public Point Position
        {
            get;
            set;
        }

        public ConnectorOrientation Orientation
        {
            get;
            set;
        }
    }
}
