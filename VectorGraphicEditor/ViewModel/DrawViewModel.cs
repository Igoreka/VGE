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
        private DrawMode _drawMode;
        private bool _addNewLineIsChecked;
        private bool _editLineIsChecked;
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
                NotifyPropertyChanged(nameof(CurrentPickColor));
                NotifyPropertyChanged(nameof(CurrentColor));
            }
        }

        public SolidColorBrush CurrentColor
        {
            get { return new SolidColorBrush(_currentPickColor); }
        }
        #endregion

        #region Толщина линии
        public Dictionary<Thikness, string> LineThiknees { get; set; }

        public void InitCollections()
        {
            LineThiknees = new Dictionary<Thikness, string>();
            foreach (Thikness val in Enum.GetValues(typeof(Thikness)))
            {
                LineThiknees.Add(val, EnumConverter.GetDescription(val));
            }
        }
        #endregion

        public string CurrentFileName { get; set; }

        public SolidColorBrush ButtonColorAdd { get; private set; } = Brushes.DarkGray;

        public SolidColorBrush ButtonColorEdit { get; private set; } = Brushes.DarkGray;

        public SolidColorBrush ButtonColorMove { get; private set; } = Brushes.DarkGray;

        /// <summary>
        /// Признак того что режим добавления линии с раскраской кнопок
        /// </summary>
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
                    ButtonColorAdd = Brushes.Green;
                    NotifyPropertyChanged(nameof(ButtonColorAdd));
                }
                else
                {
                    _drawMode = DrawMode.None;
                    ButtonColorAdd = Brushes.DarkGray;
                    NotifyPropertyChanged(nameof(ButtonColorAdd));
                }
            }
        }

        /// <summary>
        /// Признак того что режим редактирования линии с раскраской кнопок
        /// </summary>
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
                    ButtonColorEdit = Brushes.Green;
                    NotifyPropertyChanged(nameof(ButtonColorEdit));
                }
                else
                {
                    _drawMode = DrawMode.None;
                    ButtonColorEdit = Brushes.DarkGray;
                    NotifyPropertyChanged(nameof(ButtonColorEdit));
                }
            }
        }

        /// <summary>
        /// Конструктор с установкой значений по-умолчанию
        /// </summary>
        public DrawViewModel()
        {
            InitCollections();
            _addNewLineIsChecked = false;
            _editLineIsChecked = false;
            _drawMode = DrawMode.None;
            _currentThikness = Thikness.Thin;
            _currentPickColor = (Color)ColorConverter.ConvertFromString("Blue");
        }

        /// <summary>
        /// Режим работы (Добавление линии, редактирование линии, удаление линии ...)
        /// </summary>
        public DrawMode DrawMode
        {
            get { return _drawMode; }
            set
            {
                _drawMode = value;
                NotifyPropertyChanged(nameof(DrawMode));
                if (_drawMode == DrawMode.MoveFigure)
                {
                    ButtonColorMove = Brushes.Green;
                }
                else
                {
                    ButtonColorMove = Brushes.DarkGray;
                }
                NotifyPropertyChanged(nameof(ButtonColorMove));
            }
        }

        /// <summary>
        /// Толщина линии
        /// </summary>
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
                NotifyPropertyChanged(nameof(CurrentThiknessDouble));
            }
        }

        public double CurrentThiknessDouble => (double)_currentThikness;

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
