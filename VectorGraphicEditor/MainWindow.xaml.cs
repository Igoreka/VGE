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
        private Rectangle _marker;
        private Polyline _currentPolyline;
        private Point _currentPoint;
        private Point _moveFromPoint;
        private Point _moveToPoint;
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
            _viewModel.AddNewLineIsChecked = true;
            ClearMarkers();
            AddNewLine();
        }

        private void AddNewLine()
        {
            _currentPolyline = new Polyline
            {
                Stroke = _viewModel.CurrentColor,
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
                if (_viewModel.DrawMode == DrawMode.MoveFigure && _currentPolyline != null && _moveFromPoint == default(Point))
                {
                    _moveFromPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                    _marker = new Rectangle();
                    _marker.Stroke = Brushes.Blue;
                    _marker.Fill = Brushes.Blue;
                    _marker.Width = 14;
                    _marker.Height = 14;
                    Canvas.SetLeft(_marker, _moveFromPoint.X - 7);
                    Canvas.SetTop(_marker, _moveFromPoint.Y - 7);
                    _addedMarkerIndexes.Add(drawTable.Children.Add(_marker));
                    _marker = null;
                }
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                if (_viewModel.DrawMode == DrawMode.AddNewFigure)
                {
                    _viewModel.AddNewLineIsChecked = false;
                }
                if (_viewModel.DrawMode == DrawMode.MoveVertex)
                {
                    _viewModel.DrawMode = DrawMode.EditFigure;
                }
                if (_viewModel.DrawMode == DrawMode.MoveFigure && _currentPolyline != null && _moveToPoint == default(Point))
                {
                    _moveToPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                    _marker = new Rectangle();
                    _marker.Stroke = Brushes.Green;
                    _marker.Fill = Brushes.Green;
                    _marker.Width = 14;
                    _marker.Height = 14;
                    Canvas.SetLeft(_marker, _moveToPoint.X - 7);
                    Canvas.SetTop(_marker, _moveToPoint.Y - 7);
                    _addedMarkerIndexes.Add(drawTable.Children.Add(_marker));
                    _marker = null;
                    MoveFigure();
                }
            }
        }

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
            drawTable.InvalidateVisual();
        }


        private void btnDelVertex_Click(object sender, RoutedEventArgs e)
        {
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
            drawTable.InvalidateVisual();
        }

        private void btnMoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.EditFigure && _currentPolyline != null)
            {
                _viewModel.DrawMode = DrawMode.MoveFigure;
            }
        }

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
            drawTable.Children.Remove(_currentPolyline);
            _currentPolyline = null;
            _pointIndex = null;
            _marker = null;
            drawTable.InvalidateVisual();
        }

        private void drawTable_MouseMove(object sender, MouseEventArgs e)
        {
            if ((_viewModel.DrawMode == DrawMode.AddNewFigure || _viewModel.DrawMode == DrawMode.MoveVertex)
                && _currentPoint != null
                && _currentPolyline != null
                && _currentPolyline.Points.Count > 1)
            {
                _currentPolyline.Points.RemoveAt(_pointIndex.Value);
                _currentPoint.X = e.GetPosition(drawTable).X;
                _currentPoint.Y = e.GetPosition(drawTable).Y;
                _currentPolyline.Points.Insert(_pointIndex.Value, _currentPoint);
                if (_marker != null)
                {
                    Canvas.SetLeft(_marker, _currentPoint.X - 5);
                    Canvas.SetTop(_marker, _currentPoint.Y - 5);
                }
                drawTable.InvalidateVisual();
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EditLineIsChecked = true;
            _marker = null;
            _currentPolyline = null;
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
            DrawMarkres(_currentPolyline);
        }

        private void DrawMarkres(Polyline polyline)
        {
            foreach (Point pnt in polyline.Points)
            {
                _marker = new Rectangle();
                _marker.Stroke = Brushes.Green;
                _marker.Fill = Brushes.Transparent;
                _marker.Width = 10;
                _marker.Height = 10;
                _marker.MouseDown += Marker_MouseDown;
                Canvas.SetLeft(_marker, pnt.X - 5);
                Canvas.SetTop(_marker, pnt.Y - 5);
                _addedMarkerIndexes.Add(drawTable.Children.Add(_marker));
            }
            _marker = null;
        }

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

        private void ClearMarkers()
        {
            if (_addedMarkerIndexes.Count != 0)
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
