using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private Polyline _currentPolyline;
        private Rectangle _marker;
        private Point _currentPoint;
        private int? _pointIndex;
        private List<int> _addedMarkerIndexes = new List<int>();

        private DrawViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new DrawViewModel();
            this.DataContext = _viewModel;
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DrawMode = DrawMode.AddNewFigure;
            ClearMarkers();
            AddNewLine();
        }

        private void AddNewLine()
        {
            _currentPolyline = new Polyline
            {
                Stroke = Brushes.Red,
                StrokeThickness = (double)_viewModel.CurrentThikness
            };
            _currentPolyline.MouseDown += Polyline_MouseDown;
            _pointIndex = null;
            drawTable.Children.Add(_currentPolyline);
        }

        private void drawTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_viewModel.DrawMode == DrawMode.AddNewFigure)
                {
                    _currentPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                    _currentPolyline.Points.Add(_currentPoint);
                    if (_pointIndex == null) { _pointIndex = 0; }
                    else { _pointIndex++; }
                    if (_currentPolyline.Points.Count == 1)
                    {
                        _currentPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                        _currentPolyline.Points.Add(_currentPoint);
                        _pointIndex++;
                    }
                }
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                AddNewLine();
            }
        }

        private void drawTable_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.AddNewFigure && _currentPoint != null && _currentPolyline != null && _currentPolyline.Points.Count > 1)
            {
                _currentPolyline.Points.RemoveAt(_pointIndex.Value);
                _currentPoint.X = e.GetPosition(drawTable).X;
                _currentPoint.Y = e.GetPosition(drawTable).Y;
                _currentPolyline.Points.Insert(_pointIndex.Value, _currentPoint);
                drawTable.InvalidateVisual();
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DrawMode = DrawMode.EditFigure;
        }

        private void Polyline_MouseDown(object sender, MouseEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.AddNewFigure)
            {
                return;
            }

            if (_currentPolyline != (Polyline)sender)
            {
                ClearMarkers();
            }
            _currentPolyline = (Polyline)sender;
            foreach(Point pnt in _currentPolyline.Points)
            {
                _marker = new Rectangle();
                _marker.Stroke = Brushes.Green;
                _marker.Fill = Brushes.Transparent;
                _marker.Width = 10;
                _marker.Height = 10;
                Canvas.SetLeft(_marker, pnt.X - 5);
                Canvas.SetTop(_marker, pnt.Y - 5);
                _addedMarkerIndexes.Add(drawTable.Children.Add(_marker));
            }
        }

        private void ClearMarkers()
        {
            if(_addedMarkerIndexes.Count != 0)
            {
                _addedMarkerIndexes.Reverse();
                foreach (int idx in _addedMarkerIndexes)
                {
                    drawTable.Children.RemoveAt(idx);
                }
                _addedMarkerIndexes.Clear();
            }
        }
    }
}
