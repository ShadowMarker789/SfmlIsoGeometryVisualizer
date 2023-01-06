using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Window;
using SFML.Graphics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NetTopologySuite.Geometries;

namespace SfmlIsoGeometryVisualizer
{
    

    public class SfmlGeometryWindow
    {
        private RenderWindow? _renderWindow;
        private Queue<Action> _actionQueue;

        private RenderWindow Window { get => _renderWindow ?? throw new InvalidOperationException("We are not initialized!"); }

        private bool _shouldShow = false;

        public bool ShouldShow { get => _shouldShow; set => _shouldShow = value; }

        private bool _sizeHasChanged = true;

        // private List<>;

        private VertexArray? _pointVertexArray;
        public VertexArray PointVertexArray { get => _pointVertexArray ?? throw new InvalidOperationException("We are not initialized!"); }

        private VertexArray? _lineArray;
        public VertexArray LineArray { get => _lineArray ?? throw new InvalidOperationException("We are not initialized!"); }

        private View? _mainView;
        private View MainView { get => _mainView ?? throw new InvalidOperationException("We are not initialized!"); }

        private Geometry? _displayingGeometry;

        public SfmlGeometryWindow()
        {

            _actionQueue = new Queue<Action>();

            Thread t = new Thread(SfmlWindowEntryPoint);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                t.SetApartmentState(ApartmentState.STA);
            }

            t.Name = nameof(SfmlWindowEntryPoint);
            t.IsBackground = false;
            t.Start();
        }

        public void SetSfmlWindowRect(Avalonia.PixelPoint position, Avalonia.Size size)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    Window.Position = new Vector2i(position.X, position.Y);
                    Window.Size = new Vector2u((uint)size.Width, (uint)size.Height);
                    _shouldShow = true;
                });
            }
        }

        public void SetGeometry(NetTopologySuite.Geometries.Geometry geometry)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    HandleGeometryChange(geometry);
                });
            }
        }

        private void HandleSizeChanged()
        {
            if (_displayingGeometry is null) return;

            var env = _displayingGeometry.EnvelopeInternal;

            var envCenter = env.Centre;

            var geoExpandedSize = new Vector2f((float)(env.Width * 1.2), (float)(env.Height * 1.2));

            var geoRatio = geoExpandedSize.X / geoExpandedSize.Y;

            var windowRatio = ((float)Window.Size.X) / ((float)Window.Size.Y);

            if (windowRatio > geoRatio)
            {
                // window is wider than geometry
                // so use the heights, and keep the aspect ratio 

                MainView.Size = new Vector2f(geoExpandedSize.Y * windowRatio, geoExpandedSize.Y);
            }
            else
            {
                // geometry is wider than window 
                // so use the width, and keep the aspect ratio for height 
                MainView.Size = new Vector2f(geoExpandedSize.X, geoExpandedSize.X / windowRatio);
            }
            MainView.Center = new Vector2f((float)envCenter.X, -(float)envCenter.Y);
        }

        private void HandleGeometryChange(NetTopologySuite.Geometries.Geometry geometry)
        {
            _displayingGeometry = geometry;

            HandleSizeChanged();


            switch (geometry.OgcGeometryType)
            {
                case NetTopologySuite.Geometries.OgcGeometryType.Polygon:
                    {
                        NetTopologySuite.Geometries.Polygon p = (NetTopologySuite.Geometries.Polygon)geometry;
                        PointVertexArray.PrimitiveType = PrimitiveType.Points;
                        PointVertexArray.Clear();
                        foreach (var pt in p.ExteriorRing.Coordinates)
                        {
                            PointVertexArray.Append(new Vertex(new Vector2f((float)pt.X, (float)-pt.Y), Color.Yellow));
                        }

                        {
                            LineArray.Clear();
                            double previousX = p.ExteriorRing.CoordinateSequence.GetX(0), previousY = -p.ExteriorRing.CoordinateSequence.GetY(0);
                            for (int i = 1; i < p.ExteriorRing.CoordinateSequence.Count; i++)
                            {
                                LineArray.Append(new Vertex(new Vector2f(
                                    (float)previousX, (float)previousY
                                    ), Color.Blue));

                                previousX = p.ExteriorRing.CoordinateSequence.GetX(i);
                                previousY = -p.ExteriorRing.CoordinateSequence.GetY(i);

                                LineArray.Append(new Vertex(new Vector2f(
                                    (float)previousX, (float)previousY
                                    ), Color.Blue));

                            }
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        private void SfmlWindowEntryPoint()
        {
            _renderWindow = new RenderWindow(new VideoMode(640, 360), "Sfml Geometry Visualizer", Styles.Resize);
            _renderWindow.SetVerticalSyncEnabled(true);

            _renderWindow.Closed += (a, b) => { ((RenderWindow?)a)?.Close(); Environment.Exit(0); };
            _renderWindow.Resized += (a, b) => { _shouldShow = true; _sizeHasChanged = true; };
            // TODO: Handle other events here 

            //CircleShape cs = new CircleShape();

            //cs.OutlineThickness = 2;
            //cs.Radius = 20;
            //cs.OutlineColor = Color.White;

            RectangleShape rs = new RectangleShape()
            {
                OutlineThickness = 2f,
                OutlineColor = Color.Red,
                FillColor = Color.Transparent,
            };

            _pointVertexArray = new VertexArray(PrimitiveType.Points);

            _lineArray = new VertexArray(PrimitiveType.Lines);

            _mainView = new View();

            Stopwatch renderTimer = new Stopwatch();
            renderTimer.Start();

            while (_renderWindow.IsOpen)
            {
                _renderWindow.DispatchEvents();

                // TODO: Handle delegate dispatch(es) here 
                lock(_actionQueue)
                {
                    while (_actionQueue.TryDequeue(out var action))
                    {
                        action.Invoke();
                    }
                }

                if (renderTimer.Elapsed > TimeSpan.FromMilliseconds(50.0d))
                {
                    _shouldShow = true;
                    renderTimer.Restart();
                }

                if (_shouldShow)
                {

                    _renderWindow.Clear();

                    if (_sizeHasChanged)
                    {
                        _sizeHasChanged = false;
                        HandleSizeChanged();
                    }


                    // TODO: Draw the geometry here 

                    Vector2f centerPoint = new Vector2f(0f, 0f);

                    _renderWindow.SetView(_mainView);

                    rs.Position = new Vector2f(2 - _renderWindow.Size.X / 2f, 2 - _renderWindow.Size.Y / 2f);
                    rs.Size = new Vector2f(_renderWindow.Size.X - 4f, _renderWindow.Size.Y - 4f);

                    _renderWindow.Draw(rs);

                    _renderWindow.Draw(LineArray);

                    _renderWindow.Draw(PointVertexArray);

                    _renderWindow.Display();

                    _shouldShow = false;
                }
                else
                {
                    Thread.Sleep(2);
                }
            }
        }


        
    }
}
