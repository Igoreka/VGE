using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private bool _isAddNewObject;
        private int? _pointIndex;
        private List<int> _addedMarkerIndexes = new List<int>();
        public MainWindow()
        {
            InitializeComponent();
            _isAddNewObject = false;
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            _isAddNewObject = true;
            addNewLin();
        }

        private void addNewLin()
        {
            _currentPolyline = new Polyline
            {
                Stroke = Brushes.Red
            };
            _currentPolyline.MouseDown += Polyline_MouseDown;
            _pointIndex = null;
            drawTable.Children.Add(_currentPolyline);
        }

        private void drawTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isAddNewObject)
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
                addNewLin();
            }
        }

        private void drawTable_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isAddNewObject && _currentPoint != null && _currentPolyline != null && _currentPolyline.Points.Count > 1)
            {
                _currentPolyline.Points.RemoveAt(_pointIndex.Value);
                _currentPoint.X = e.GetPosition(drawTable).X;
                _currentPoint.Y = e.GetPosition(drawTable).Y;
                _currentPolyline.Points.Insert(_pointIndex.Value, _currentPoint);
                txtMessage.Text = "(" + _currentPoint.X.ToString() + ";" + _currentPoint.Y.ToString() + ")";
                drawTable.InvalidateVisual();
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            _isAddNewObject = false;
        }

        private void Polyline_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isAddNewObject)
            {
                return;
            }

            if (_currentPolyline != (Polyline)sender && _addedMarkerIndexes.Count!= 0)
            {
                _addedMarkerIndexes.Reverse();
                foreach (int idx in _addedMarkerIndexes)
                {
                    drawTable.Children.RemoveAt(idx);
                }
                _addedMarkerIndexes.Clear();
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


    }
}
