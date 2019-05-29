using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using VectorGraphicEditor.Model;


namespace VectorGraphicEditor.ViewModel
{
    public class DrawViewModel : INotifyPropertyChanged
    {
        private Thikness _currentThikness;
        private SolidColorBrush _currentColor;
        private DrawMode _drawMode;
        private bool _addNewLineIsChecked;
        private bool _editLineIsChecked;
        private SolidColorBrush _buttonColorAdd = Brushes.DarkGray;
        private SolidColorBrush _buttonColorEdit = Brushes.DarkGray;
        private SolidColorBrush _buttonColorMove = Brushes.DarkGray;
        private Color _currentPickColor;
        
        #region Цвет
        public Color CurrentPickColor
        {
            get
            {
                return _currentPickColor;
            }
            set
            {
                _currentPickColor = value;
                _currentColor = new SolidColorBrush(_currentPickColor);
                NotifyPropertyChanged(nameof(CurrentPickColor));
                NotifyPropertyChanged(nameof(CurrentColor));
            }
        }

        public SolidColorBrush CurrentColor
        {
            get { return _currentColor; }
        }
        #endregion

        #region Толщина линии
        public Dictionary<Thikness, string> LineThiknees { get; set; }

        public void InitCollections()
        {
            LineThiknees = new Dictionary<Thikness, string>();
            foreach (Thikness val in Enum.GetValues(typeof(Thikness)))
            {
                LineThiknees.Add(val,EnumConverter.GetDescription(val));
            }
        }
        #endregion

        public String CurrentFileName { get; set; }

        public SolidColorBrush ButtonColorAdd => _buttonColorAdd;

        public SolidColorBrush ButtonColorEdit => _buttonColorEdit;

        public SolidColorBrush ButtonColorMove => _buttonColorMove;

        public bool AddNewLineIsChecked
        {
            get { return _addNewLineIsChecked; }
            set
            {
                _addNewLineIsChecked = value;
                NotifyPropertyChanged(nameof(AddNewLineIsChecked));
                if (_addNewLineIsChecked)
                {
                    EditLineIsChecked = !_addNewLineIsChecked;
                    _drawMode = DrawMode.AddNewFigure;
                     _buttonColorAdd = Brushes.Green;
                    NotifyPropertyChanged(nameof(ButtonColorAdd));
                }
                else
                {
                    _drawMode = DrawMode.None;
                    _buttonColorAdd = Brushes.DarkGray;
                    NotifyPropertyChanged(nameof(ButtonColorAdd));
                }
            }
        }
        public bool EditLineIsChecked
        {
            get { return _editLineIsChecked; }
            set
            {
                _editLineIsChecked = value;
                NotifyPropertyChanged(nameof(AddNewLineIsChecked));
                if (_editLineIsChecked)
                {
                    AddNewLineIsChecked = !_editLineIsChecked;
                    _drawMode = DrawMode.EditFigure;
                    _buttonColorEdit = Brushes.Green;
                    NotifyPropertyChanged(nameof(ButtonColorEdit));
                }
                else
                {
                    _drawMode = DrawMode.None;
                    _buttonColorEdit = Brushes.DarkGray;
                    NotifyPropertyChanged(nameof(ButtonColorEdit));
                }
            }
        }

        public DrawViewModel()
        {
            InitCollections();
            _addNewLineIsChecked = false;
            _editLineIsChecked = false;
            _drawMode = DrawMode.None;
            _currentThikness = Thikness.Thin;
        }

        public DrawMode DrawMode
        {
            get { return _drawMode; }
            set
            {
                _drawMode = value;
                NotifyPropertyChanged(nameof(DrawMode));
                if (_drawMode == DrawMode.MoveFigure)
                {
                    _buttonColorMove = Brushes.Green;
                }
                else
                {
                    _buttonColorMove = Brushes.DarkGray;
                }
                NotifyPropertyChanged(nameof(ButtonColorMove));
            }
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
