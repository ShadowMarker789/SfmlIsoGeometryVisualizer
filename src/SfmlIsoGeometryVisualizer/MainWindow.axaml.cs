using Avalonia;
using Avalonia.Controls;
using NetTopologySuite.Geometries;
using System;

namespace SfmlIsoGeometryVisualizer
{
    public partial class MainWindow : Window
    {
        public static StyledProperty<string> GeometryWellKnownTextProperty =
            AvaloniaProperty.Register<MainWindow, string>(nameof(GeometryWellKnownText), string.Empty, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public string GeometryWellKnownText { get => GetValue(GeometryWellKnownTextProperty); set => SetValue(GeometryWellKnownTextProperty, value); }

        public static StyledProperty<string> OldGeometryWellKnownTextProperty =
            AvaloniaProperty.Register<MainWindow, string>(nameof(OldGeometryWellKnownText), string.Empty, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public string OldGeometryWellKnownText { get => GetValue(OldGeometryWellKnownTextProperty); set => SetValue(OldGeometryWellKnownTextProperty, value); }

        public static StyledProperty<Geometry> GeometryProperty =
            AvaloniaProperty.Register<MainWindow, Geometry>(nameof(GeometryWellKnownText), Polygon.Empty, defaultBindingMode: Avalonia.Data.BindingMode.OneWay);
        public Geometry Geometry{ get => GetValue(GeometryProperty); set => SetValue(GeometryProperty, value); }

        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += MainWindow_SizeChanged;
            this.PositionChanged += MainWindow_PositionChanged;
            this.Closed += MainWindow_Closed;
            this.Loaded += MainWindow_Loaded;

            this.DisplayButton.Click += DisplayButton_Click;
        }

        private void DisplayButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var text = GeometryWellKnownText;
                var geometry = Program.WKTReader.Read(text);

                Program.SetSfmlGeometry(geometry);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                return;
            }
            Program.StartSfmlWindow();
            Program.ShowSfmlWindow();
            PushRectToSfml();
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            //PushRectToSfml();
        }

        private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            //PushRectToSfml();
        }

        private void PushRectToSfml()
        {
            if (FrameSize is null)
                return;
            Size size = FrameSize.Value;

            Program.SetSfmlWindowRect(new PixelPoint((int)(this.Position.X + size.Width), (int)(this.Position.Y)), this.FrameSize.GetValueOrDefault());
        }
    }
}
