using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NetTopologySuite.Geometries;
using System;

namespace SfmlIsoGeometryVisualizer
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.

        private static SfmlGeometryWindow? _sfmlGeometryWindow;

        public static NetTopologySuite.IO.WKTReader WKTReader { get; private set; } = new NetTopologySuite.IO.WKTReader();

        [STAThread]
        public static void Main(string[] args)
        {
            BuildAndRunAvaloniaApp();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();


        public static void BuildAndRunAvaloniaApp()
        {
            string[]? args;
            try
            {
                args = Environment.GetCommandLineArgs();
            }
            catch // uhmm, theoretically this could fail, but I do not know what would cause that 
            // you know, aside from out of memory issues, but how can we be out of memory at startup?
            {
                args = Array.Empty<string>();
            }
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        public static void ShowSfmlWindow()
        {
            if (_sfmlGeometryWindow is null) return;

            _sfmlGeometryWindow.ShouldShow = true;
        }

        public static void SetSfmlWindowRect(Avalonia.PixelPoint position, Avalonia.Size size)
        {
            if (_sfmlGeometryWindow is null) return;

            _sfmlGeometryWindow.SetSfmlWindowRect(position, size);
        }

        public static void StartSfmlWindow()
        {
            if (_sfmlGeometryWindow is not null) return;

            _sfmlGeometryWindow = new SfmlGeometryWindow();
        }

        public static void SetSfmlGeometry(Geometry geometry) 
        {
            if (_sfmlGeometryWindow is null) return;

            _sfmlGeometryWindow.SetGeometry(geometry);
        }
    }
}
