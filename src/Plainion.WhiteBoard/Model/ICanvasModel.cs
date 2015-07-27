using System.Collections.Generic;

namespace Plainion.WhiteBoard.Model
{
    public interface ICanvasModel
    {
        IEnumerable<T> GetItems<T>();
    }
}
