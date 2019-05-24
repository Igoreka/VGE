using System.Collections.Generic;
using System.Windows.Media;
using VectorGraphicEditor.Model;

namespace VectorGraphicEditor.ViewModel
{
    public class BrokenLine
    {
        private Thikness _thikness;
        private SolidColorBrush _color;

        public List<Vertex> Vertex { get; set; }

        public Thikness Thikness
        {
            get { return _thikness; }
            set { _thikness = value;}
        }

        public SolidColorBrush Color
        {
            get { return _color; }
            set { _color = value;}
        }
    }
}
