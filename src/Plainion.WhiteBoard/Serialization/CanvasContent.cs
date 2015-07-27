using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Plainion.WhiteBoard.Designer;

namespace Plainion.WhiteBoard.Serialization
{
    public class CanvasContent
    {
        private Point myOffset;

        public CanvasContent( IEnumerable<DesignerItem> items, IEnumerable<Connection> connections )
        {
            Items = items.ToList();
            Connections = connections.ToList();
        }

        public Point Offset
        {
            get
            {
                return myOffset;
            }
            set
            {
                myOffset = value;
            }
        }

        public void AddOffset( double offsetX, double offsetY )
        {
            myOffset.Offset( offsetX, offsetY );
        }

        public IEnumerable<DesignerItem> Items
        {
            get;
            private set;
        }

        public IEnumerable<Connection> Connections
        {
            get;
            private set;
        }
    }
}
