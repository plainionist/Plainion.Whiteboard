using System;

namespace Plainion.WhiteBoard.Serialization
{
    public interface IIdTransformation
    {
        Guid GetId( Guid sourceId );
    }
}
