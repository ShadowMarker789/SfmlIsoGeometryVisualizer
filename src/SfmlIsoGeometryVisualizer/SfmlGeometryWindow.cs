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

namespace SfmlIsoGeometryVisualizer
{
    

    public class SfmlGeometryWindow
    {
        private RenderWindow? _renderWindow;
        private Queue<Action> _actionQueue;

        private RenderWindow Window { get => _renderWindow ?? throw new InvalidOperationException("We are not initialized!"); }

        private bool _shouldShow = false;

        public bool ShouldShow { get => _shouldShow; set => _shouldShow = value; }

        // private List<>;

        private VertexArray? _pointVertexArray;
        public VertexArray PointVertexArray { get => _pointVertexArray ?? throw new InvalidOperationException("We are not initialized!"); }

        private VertexArray? _lineArray;
        public VertexArray LineArray { get => _lineArray ?? throw new InvalidOperationException("We are not initialized!"); }

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

        private void HandleGeometryChange(NetTopologySuite.Geometries.Geometry geometry)
        {

            switch(geometry.OgcGeometryType)
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
            _renderWindow.Resized += (a, b) => { _shouldShow = true; };
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

            View mainView = new View();

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

                    // TODO: Draw the geometry here 

                    Vector2f centerPoint = new Vector2f(0f, 0f);

                    mainView.Center = centerPoint;
                    mainView.Size = new Vector2f(_renderWindow.Size.X, _renderWindow.Size.Y);

                    _renderWindow.SetView(mainView);

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
