using System;

namespace Plainion.WhiteBoard.Serialization
{
    public class IdentityIdTransformation : IIdTransformation
    {
        public Guid GetId( Guid sourceId )
        {
            return sourceId;
        }
    }
}
