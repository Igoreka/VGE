using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            ResetAllSelected();
            AddNewLine();
        }

        private void ResetAllSelected()
        {
            ClearMarkers();
            _currentPolyline = null;
            _currentPoint = default(Point);
            _pointIndex = null;
            _viewModel.DrawMode = DrawMode.None;
            _marker = null;
            _viewModel.AddNewLineIsChecked = false;
            _viewModel.EditLineIsChecked = false;
            drawTable.InvalidateVisual();
        }

        private void AddNewLine()
        {
            _currentPolyline = new Polyline
            {
                Stroke = _viewModel.CurrentColor,
                StrokeThickness = (double)_viewModel.CurrentThikness
            };
            _currentPolyline.MouseDown += Polyline_MouseDown;
            _currentPolyline.MouseLeftButtonDown += Polyline_MouseLeftButtonDown;
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
            if (_viewModel.DrawMode == DrawMode.AddNewFigure || _viewModel.DrawMode == DrawMode.None)
            {
                return;
            }
            ClearMarkers();
            _currentPolyline = (Polyline)sender;
            DrawMarkres(_currentPolyline);
        }

        private void Polyline_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.DrawMode == DrawMode.AddNewFigure)
            {
                return;
            }
            if (e.ClickCount != 2)
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

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            ResetAllSelected();
            FileStream fs = File.Open("d:\\temp\\xamlFileName.xaml", FileMode.Create);
            XamlWriter.Save(drawTable, fs);
            fs.Close();
            fs.Dispose();
        }

        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs = File.Open("d:\\temp\\xamlFileName.xaml", FileMode.Open, FileAccess.Read);
            Canvas FromFile = XamlReader.Load(fs) as Canvas;
            foreach (Polyline pl in FromFile.Children)
            {
                _viewModel.CurrentPickColor = ((SolidColorBrush)(pl.Stroke)).Color;
                if (0 <= pl.StrokeThickness && pl.StrokeThickness <= 1)
                {
                    _viewModel.CurrentThikness = Thikness.Thin;
                }
                else if (1 < pl.StrokeThickness && pl.StrokeThickness <= 2)
                {
                    _viewModel.CurrentThikness = Thikness.Double;
                }
                else if (2 < pl.StrokeThickness && pl.StrokeThickness <= 3)
                {
                    _viewModel.CurrentThikness = Thikness.Triple;
                }
                else
                {
                    _viewModel.CurrentThikness = Thikness.Fourth;
                }
                AddNewLine();
                foreach (Point pt in pl.Points)
                {
                    _currentPolyline.Points.Add(pt);
                }
            }
            drawTable.InvalidateVisual();
            FromFile = null;
            fs.Dispose();
            ResetAllSelected();
        }
    }
}
