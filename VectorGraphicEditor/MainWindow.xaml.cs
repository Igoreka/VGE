using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using VectorGraphicEditor.Model;
using VectorGraphicEditor.ViewModel;

namespace VectorGraphicEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Текущий маркер вершины
        /// </summary>
        private Rectangle _marker;
        /// <summary>
        /// Текущая ломанная кривая
        /// </summary>
        private Polyline _currentPolyline;
        /// <summary>
        /// Текущая вершина ломанной кривой
        /// </summary>
        private Point _currentPoint;
        /// <summary>
        /// Точка от которой расчитывается перенос линии
        /// </summary>
        private Point _moveFromPoint;
        /// <summary>
        /// Точка до которой рассчитывается перенос ломаной линии
        /// </summary>
        private Point _moveToPoint;
        /// <summary>
        /// Индекс текущей вершины в коллекции вершин линии
        /// </summary>
        private int? _pointIndex;
        /// <summary>
        /// Коллекция маркеров, выделяющих вершины ломаной линии
        /// </summary>
        private List<int> _addedMarkerIndexes = new List<int>();
        /// <summary>
        /// Толщина линии, привязка к источнику
        /// </summary>
        private Binding _bindThikness;
        /// <summary>
        /// Цвет линии, привязка к источнику
        /// </summary>
        private Binding _bindColor;

        private DrawViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new DrawViewModel();
            this.DataContext = _viewModel;
        }

        /// <summary>
        /// Добавление новой лини, нажатие кнопки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            ResetAllSelected();
            _viewModel.AddNewLineIsChecked = true;
            _viewModel.DrawMode = DrawMode.AddNewFigure;
            AddNewLine();
        }

        private void ClearPolylineBindings(Polyline pl)
        {
            if (pl == null)
            {
                return;
            }
            //Удаляем привязки
            //И проставляем цвет текущий
            if (_bindColor != null)
            {
                BindingOperations.ClearBinding(pl, Polyline.StrokeProperty);
            }
            if (_bindThikness != null)
            {
                BindingOperations.ClearBinding(pl, Polyline.StrokeThicknessProperty);
            }
            pl.Stroke = _viewModel.CurrentColor;
            pl.StrokeThickness = _viewModel.CurrentThiknessDouble;
        }

        /// <summary>
        /// Сброс всех переменных, используемых для выделения линии
        /// </summary>
        private void ResetAllSelected()
        {
            ClearMarkers();
            ClearPolylineBindings(_currentPolyline);
            _currentPolyline = null;
            _currentPoint = default(Point);
            _pointIndex = null;
            _viewModel.DrawMode = DrawMode.None;
            _marker = null;
            _viewModel.AddNewLineIsChecked = false;
            _viewModel.EditLineIsChecked = false;
            DrawTable.InvalidateVisual();
        }

        /// <summary>
        /// Процедура создания новой ломаной кривой
        /// </summary>
        private void AddNewLine()
        {
            _currentPolyline = new Polyline
            {
                Stroke = _viewModel.CurrentColor,
                StrokeThickness = _viewModel.CurrentThiknessDouble
            };
            _currentPolyline.MouseDown += Polyline_MouseDown;
            _currentPolyline.MouseLeftButtonDown += Polyline_MouseLeftButtonDown;
            _pointIndex = null;
            DrawTable.Children.Add(_currentPolyline);
        }

        /// <summary>
        /// Отрисовка вершин
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_viewModel.DrawMode == DrawMode.AddNewFigure)
                {
                   // Добавление новой вершины в ломаной линии
                    _currentPoint = new Point(e.GetPosition(DrawTable).X, e.GetPosition(DrawTable).Y);
                    _currentPolyline.Points.Add(_currentPoint);
                    if (_pointIndex == null) { _pointIndex = 0; }
                    else { _pointIndex++; }
                    if (_currentPolyline.Points.Count == 1)
                    {
                        //Сразу добавлем вторую вершину, которую будем двигать
                        _currentPoint = new Point(e.GetPosition(DrawTable).X, e.GetPosition(DrawTable).Y);
                        _currentPolyline.Points.Add(_currentPoint);
                        _pointIndex++;
                    }
                }
                if (_viewModel.DrawMode == DrawMode.MoveFigure && _currentPolyline != null && _moveToPoint == default(Point))
                {
                    //Выставляем точку по которой считаем смещение фигуры
                    _moveToPoint = new Point(e.GetPosition(DrawTable).X, e.GetPosition(DrawTable).Y);
                    _marker = new Rectangle();
                    _marker.Stroke = Brushes.Green;
                    _marker.Fill = Brushes.Green;
                    _marker.Width = 14;
                    _marker.Height = 14;
                    Canvas.SetLeft(_marker, _moveToPoint.X - 7);
                    Canvas.SetTop(_marker, _moveToPoint.Y - 7);
                    _addedMarkerIndexes.Add(DrawTable.Children.Add(_marker));
                    _marker = null;
                    MoveFigure();
                }
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                //По клику правой кнопки мыши прекращаем действия
                if (_viewModel.DrawMode == DrawMode.AddNewFigure)
                {
                    //Снимаем выделение цветом с кнопки добавления новой линии
                    _viewModel.AddNewLineIsChecked = false;
                }
                if (_viewModel.DrawMode == DrawMode.MoveVertex)
                {
                    //Переходим из режима редактирования вершин к режиму редактиования линии
                    _viewModel.DrawMode = DrawMode.EditFigure;
                }                
            }
        }

        /// <summary>
        /// Производим смещение координат вершин линии
        /// </summary>
        private void MoveFigure()
        {
            if (_moveToPoint == default(Point) || _moveFromPoint == default(Point) || _currentPolyline == null)
            {
                return;
            }
            var deltaX = _moveToPoint.X - _moveFromPoint.X;
            var deltaY = _moveToPoint.Y - _moveFromPoint.Y;
            List<Point> tmpListPoint = new List<Point>();
            foreach (Point pt in _currentPolyline.Points)
            {
                tmpListPoint.Add(new Point(pt.X + deltaX, pt.Y + deltaY));
            }
            _currentPolyline.Points.Clear();
            foreach (Point pt in tmpListPoint)
            {
                _currentPolyline.Points.Add(new Point(pt.X, pt.Y));
            }
            tmpListPoint = null;
            _moveToPoint = default(Point);
            _moveFromPoint = default(Point);
            _currentPolyline = null;
            ClearMarkers();
            _viewModel.DrawMode = DrawMode.EditFigure;
            DrawTable.InvalidateVisual();
        }


        /// <summary>
        /// Обработчик кнопки удаления вершины ломаной линии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelVertex_Click(object sender, RoutedEventArgs e)
        {
            //Удаление вершин ломаной линии
            if (_viewModel.DrawMode != DrawMode.EditFigure)
            {
                return;
            }
            if (_pointIndex == null || _marker == null || _currentPolyline == null)
            {
                return;
            }
            _currentPolyline.Points.RemoveAt(_pointIndex.Value);
            _marker.Height = 0;
            _marker.Width = 0;
            _pointIndex = null;
            _marker = null;
            DrawTable.InvalidateVisual();
        }

        /// <summary>
        /// Обработчик нажатия кнопки перемещения линии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.EditFigure && _currentPolyline != null)
            {
                _viewModel.DrawMode = DrawMode.MoveFigure;
            }
        }

        /// <summary>
        /// Удаление ломаной линии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelLine_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DrawMode != DrawMode.EditFigure)
            {
                return;
            }
            if (_currentPolyline == null)
            {
                return;
            }
            ClearMarkers();
            DrawTable.Children.Remove(_currentPolyline);
            _currentPolyline = null;
            _pointIndex = null;
            _marker = null;
            DrawTable.InvalidateVisual();
        }

        private void DrawTable_MouseMove(object sender, MouseEventArgs e)
        {
            //Отрисовка перемещения вершины ломаной линии при добавлении и при редактировании вершин
            if ((_viewModel.DrawMode == DrawMode.AddNewFigure || _viewModel.DrawMode == DrawMode.MoveVertex)
                && _currentPoint != null
                && _currentPolyline != null
                && _currentPolyline.Points.Count > 1)
            {
                _currentPolyline.Points.RemoveAt(_pointIndex.Value);
                _currentPoint.X = e.GetPosition(DrawTable).X;
                _currentPoint.Y = e.GetPosition(DrawTable).Y;
                _currentPolyline.Points.Insert(_pointIndex.Value, _currentPoint);
                if (_marker != null)
                {
                    Canvas.SetLeft(_marker, _currentPoint.X - 5);
                    Canvas.SetTop(_marker, _currentPoint.Y - 5);
                }
                DrawTable.InvalidateVisual();
            }
        }

        /// <summary>
        /// Обаработчик нажатия кнопки выбора линии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EditLineIsChecked = true;
            _marker = null;
            _currentPolyline = null;
        }

        /// <summary>
        /// Выделение ломаной линии маркерами вершин
        /// и маркером центра масс ломаной линии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polyline_MouseDown(object sender, MouseEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.AddNewFigure || _viewModel.DrawMode == DrawMode.None)
            {
                return;
            }
            ClearMarkers();
            ClearPolylineBindings(_currentPolyline);
            _currentPolyline = (Polyline)sender;
            DrawMarkres(_currentPolyline);
            //Делаем привязку свойст толщины и цвета к источникам изменения
            _bindThikness = new Binding();
            _bindColor = new Binding();
            _bindThikness.Path = new PropertyPath("CurrentThiknessDouble");
            _bindColor.Path = new PropertyPath("CurrentColor");
            _bindThikness.Mode = BindingMode.OneWay;
            _bindColor.Mode = BindingMode.OneWay;
            _viewModel.CurrentPickColor = ((SolidColorBrush)(_currentPolyline.Stroke)).Color;
            _viewModel.CurrentThikness =  DoubleToThikness(_currentPolyline.StrokeThickness);
            _currentPolyline.SetBinding(Polyline.StrokeThicknessProperty, _bindThikness);
            _currentPolyline.SetBinding(Polyline.StrokeProperty, _bindColor);
        }

        /// <summary>
        /// Добавление новой вершины по двойному клику
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polyline_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.AddNewFigure
                || e.ClickCount != 2
                || _currentPolyline == null
                )
            {
                return;
            }

            ClearMarkers();
            Point PointToInsert = e.GetPosition((Polyline)sender);
            Point startp;
            Point stopp;
            int indexofpoint = 0;
            for (int i = 1; i < _currentPolyline.Points.Count; i++)
            {
                startp = _currentPolyline.Points[i - 1];
                stopp = _currentPolyline.Points[i];
                double l1 = Math.Sqrt((PointToInsert.X - startp.X) * (PointToInsert.X - startp.X) + (PointToInsert.Y - startp.Y) * (PointToInsert.Y - startp.Y));
                double l2 = Math.Sqrt((stopp.X - PointToInsert.X) * (stopp.X - PointToInsert.X) + (stopp.Y - PointToInsert.Y) * (stopp.Y - PointToInsert.Y));
                double l3 = Math.Sqrt((stopp.X - startp.X) * (stopp.X - startp.X) + (stopp.Y - startp.Y) * (stopp.Y - startp.Y));

                if (Math.Round(l1 + l2) == Math.Round(l3))
                {
                    indexofpoint = i;
                }
            }
            _currentPolyline.Points.Insert(indexofpoint, PointToInsert);
            DrawMarkres(_currentPolyline);
        }

        /// <summary>
        /// Отрисовка маркеров вершин линии
        /// </summary>
        /// <param name="polyline"></param>
        private void DrawMarkres(Polyline polyline)
        {
            Point ptLeftTop = new Point();
            Point ptRightDown = new Point();
            Point ptCenter = new Point();
            int i = 0;
            foreach (Point pnt in polyline.Points)
            {
                //Ищем координаты для маркера центра ломаной линии
                if (i == 0)
                {
                    ptLeftTop.X = pnt.X;
                    ptLeftTop.Y = pnt.Y;
                    ptRightDown.X = pnt.X;
                    ptRightDown.Y = pnt.Y;
                    i = 1;
                }
                if (pnt.X < ptLeftTop.X) { ptLeftTop.X = pnt.X; }
                if (pnt.Y < ptLeftTop.Y) { ptLeftTop.Y = pnt.Y; }
                if (ptRightDown.X < pnt.X) { ptRightDown.X = pnt.X; }
                if (ptRightDown.Y < pnt.Y) { ptRightDown.Y = pnt.Y; }
                //Ищем координаты для маркера центра ломаной линии
                _marker = new Rectangle();
                _marker.Stroke = Brushes.Green;
                _marker.Fill = Brushes.Transparent;
                _marker.Width = 10;
                _marker.Height = 10;
                _marker.MouseDown += Marker_MouseDown;
                Canvas.SetLeft(_marker, pnt.X - 5);
                Canvas.SetTop(_marker, pnt.Y - 5);
                _addedMarkerIndexes.Add(DrawTable.Children.Add(_marker));
            }
            //Вычисляем точку центра ломаной линии
            ptCenter.X = (ptLeftTop.X + ptRightDown.X) /2;
            ptCenter.Y = (ptLeftTop.Y + ptRightDown.Y) / 2;
            //Устанаваливаем маркер, от которого будем пермещать ломаную линию
            _moveFromPoint = new Point(ptCenter.X, ptCenter.Y);
            _marker = new Rectangle();
            _marker.Stroke = Brushes.Blue;
            _marker.Fill = Brushes.Blue;
            _marker.Width = 12;
            _marker.Height = 12;
            Canvas.SetLeft(_marker, _moveFromPoint.X - 6);
            Canvas.SetTop(_marker, _moveFromPoint.Y - 6);
            _addedMarkerIndexes.Add(DrawTable.Children.Add(_marker));
            _marker = null;
        }

        /// <summary>
        /// Обработка нажатия кнопки на вершине
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Marker_MouseDown(object sender, MouseEventArgs e)
        {
            if (_viewModel.DrawMode != DrawMode.MoveVertex)
            {
                _viewModel.DrawMode = DrawMode.MoveVertex;
            }

            if (_marker != null)
            {
                _marker.Fill = Brushes.Transparent;
            }
            _marker = (Rectangle)sender;

            _marker.Fill = Brushes.Green;
            FindVertexIndex();
        }

        /// <summary>
        /// Поиск вершины, которая соответствует выбранному маркеру
        /// </summary>
        private void FindVertexIndex()
        {
            _pointIndex = 0;
            _currentPoint.X = Canvas.GetLeft(_marker) + 5;
            _currentPoint.Y = Canvas.GetTop(_marker) + 5;
            foreach (Point pt in _currentPolyline.Points)
            {
                if (pt.X == _currentPoint.X && pt.Y == _currentPoint.Y)
                {
                    return;
                }
                _pointIndex++;
            }
        }

        /// <summary>
        /// Очистка всех маркеров с канвы
        /// </summary>
        private void ClearMarkers()
        {
            if (_addedMarkerIndexes.Count != 0)
            {
                _addedMarkerIndexes.Reverse();
                foreach (int idx in _addedMarkerIndexes)
                {
                    DrawTable.Children.RemoveAt(idx);
                }
                _addedMarkerIndexes.Clear();
            }
        }

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog myDialog = new SaveFileDialog();
            myDialog.Filter = "Векторные рисунки(*.XAML)|*.XAML" + "|Все файлы (*.*)|*.* ";
            myDialog.AddExtension = true;
            myDialog.OverwritePrompt = true;
            if (myDialog.ShowDialog() == true)
            {
                _viewModel.CurrentFileName = myDialog.FileName;
                ResetAllSelected();
                FileStream fs = File.Open(_viewModel.CurrentFileName, FileMode.Create);
                XamlWriter.Save(DrawTable, fs);
                fs.Close();
                fs.Dispose();
            }
        }

        /// <summary>
        /// Преобразуем толщину линии к enum
        /// </summary>
        /// <param name="lineThik"></param>
        /// <returns></returns>
        private Thikness DoubleToThikness(double lineThik)
        {
            if (0 <= lineThik && lineThik <= 1)
            {
                return Thikness.Thin;
            }
            else if (1 < lineThik && lineThik <= 2)
            {
                return Thikness.Double;
            }
            else if (2 < lineThik && lineThik <= 3)
            {
                return Thikness.Triple;
            }
            else
            {
                return Thikness.Fourth;
            }
        }

        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Векторные рисунки(*.XAML)|*.XAML" + "|Все файлы (*.*)|*.* ";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = true;
            if (myDialog.ShowDialog() == true)
            {
                DrawTable.Children.Clear();
                _viewModel.CurrentFileName = myDialog.FileName;

                FileStream fs = File.Open(_viewModel.CurrentFileName, FileMode.Open, FileAccess.Read);
                Canvas FromFile = XamlReader.Load(fs) as Canvas;
                foreach (Polyline pl in FromFile.Children)
                {
                    _viewModel.CurrentPickColor = ((SolidColorBrush)(pl.Stroke)).Color;
                    _viewModel.CurrentThikness =  DoubleToThikness(pl.StrokeThickness);
                    
                    AddNewLine();
                    foreach (Point pt in pl.Points)
                    {
                        _currentPolyline.Points.Add(pt);
                    }
                }
                DrawTable.InvalidateVisual();
                FromFile = null;
                fs.Dispose();
                ResetAllSelected();
            }
        }
    }
}
