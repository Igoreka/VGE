using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using VectorGraphicEditor.Model;


namespace VectorGraphicEditor.ViewModel
{
    public class DrawViewModel : INotifyPropertyChanged
    {
        private Thikness _currentThikness;
        private SolidColorBrush _currentColor;
        private BrokenLine _currentBrokenLine;
        private Vertex _currentVertex;
        private DrawMode _drawMode;

        public Dictionary<Thikness, string> LineThiknees { get; set; }

        public void InitCollections()
        {
            LineThiknees = new Dictionary<Thikness, string>();
            foreach (Thikness val in Enum.GetValues(typeof(Thikness)))
            {
                LineThiknees.Add(val,EnumConverter.GetDescription(val));
            }
        }

        public DrawViewModel()
        {
            InitCollections();
            _drawMode = DrawMode.None;
            _currentThikness = Thikness.Thin;
        }

        public Vertex CurrentVertex
        {
            get { return _currentVertex; }
            set { _currentVertex = value; NotifyPropertyChanged(nameof(CurrentVertex)); }
        }

        public DrawMode DrawMode
        {
            get { return _drawMode; }
            set { _drawMode = value; NotifyPropertyChanged(nameof(DrawMode)); }
        }

        public Thikness CurrentThikness
        {
            get
            {
                return _currentThikness;
            }
            set
            {
                _currentThikness = value;
                NotifyPropertyChanged(nameof(CurrentThikness));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                PropertyChanged(this, new PropertyChangedEventArgs("HasError"));
            }
        }

       
    }
}
