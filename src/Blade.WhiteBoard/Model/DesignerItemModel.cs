using System.Windows;

namespace Plainion.WhiteBoard.Model
{
    class DesignerItemModel
    {
        public double Left
        {
            get;
            set;
        }

        public double Top
        {
            get;
            set;
        }

        public Size Size
        {
            get;
            set;
        }

        public Rect GetBoundingRectangle()
        {
            return new Rect( Left, Top, Size.Width, Size.Height );
        }
    }
}
