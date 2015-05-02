using System.Collections.Generic;

namespace Plainion.WhiteBoard.Designer
{
    public class ItemPropertyCollection : List<ItemProperty>
    {
        public ItemPropertyCollection()
        {
        }

        public ItemPropertyCollection( IEnumerable<ItemProperty> collection )
        {
            AddRange( collection );
        }
    }
}
