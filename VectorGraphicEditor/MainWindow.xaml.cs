using System.Windows;
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
        private Point _currentPoint;
        private bool _isAddNewObject;
        public MainWindow()
        {
            InitializeComponent();
            _isAddNewObject = false;            
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            _isAddNewObject = true;
            _currentPolyline = new Polyline
            {
                Stroke = Brushes.Red
            };
            drawTable.Children.Add(_currentPolyline);
        }

        private void drawTable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _isAddNewObject = false;
            }
        }

        private void drawTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isAddNewObject)
                {
                    _currentPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                    _currentPolyline.Points.Add(_currentPoint);
                    if (_currentPolyline.Points.Count == 1)
                    {
                        _currentPoint = new Point(e.GetPosition(drawTable).X, e.GetPosition(drawTable).Y);
                        _currentPolyline.Points.Add(_currentPoint);
                    }
                }
            }
        }

        private void drawTable_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isAddNewObject && _currentPoint != null && _currentPolyline != null && _currentPolyline.Points.Count > 1)
            {
                _currentPolyline.Points.Remove(_currentPoint);
                _currentPoint.X = e.GetPosition(drawTable).X;
                _currentPoint.Y = e.GetPosition(drawTable).Y;
                _currentPolyline.Points.Add(_currentPoint);
                txtMessage.Text = "("+_currentPoint.X.ToString() + ";" + _currentPoint.Y.ToString() + ")";
                drawTable.InvalidateVisual();
            }
        }
    }
}
