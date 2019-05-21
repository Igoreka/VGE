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
            TestDrawPolyLine();
            _isAddNewObject = false;
            txtMessage.Text = DrawTable.Height.ToString() + ";" + DrawTable.Width.ToString();
        }

        private void TestDrawPolyLine()
        {
            PointCollection test = new PointCollection
            {
                new Point(10, 10),
                new Point(10, 40),
                new Point(20, 40),
                new Point(20, 80),
                new Point(60, 90),
                new Point(60, 150),
                new Point(100, 200)
            };
            _ = DrawTable.Children.Add(new Polyline
            {
                Stroke = Brushes.Blue,
                Points = test
            });
            test = new PointCollection
            {
                new Point(0, 0),
                new Point(0, 400),
                new Point(400, 400),
                new Point(0, 400),
                new Point(0, 0)
            };
            _ = DrawTable.Children.Add(new Polyline
            {
                Stroke = Brushes.Green,
                Points = test
            });
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            _isAddNewObject = true;
            _currentPolyline = new Polyline
            {
                Stroke = Brushes.Red
            };
            DrawTable.Children.Add(_currentPolyline);
        }

        private void DrawTable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _isAddNewObject = false;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isAddNewObject)
                {
                    _currentPoint = new Point(e.GetPosition(DrawTable).X, e.GetPosition(DrawTable).Y);
                    _currentPolyline.Points.Add(_currentPoint);
                }
            }
        }

    }
}
