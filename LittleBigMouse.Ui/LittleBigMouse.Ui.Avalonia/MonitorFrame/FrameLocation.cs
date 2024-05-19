﻿using ReactiveUI;
using LittleBigMouse.DisplayLayout.Monitors;
using LittleBigMouse.Plugins;

namespace LittleBigMouse.Ui.Avalonia.MonitorFrame;

public class FrameLocation : ReactiveObject, IFrameLocation
{
    public FrameLocation(PhysicalMonitor monitor)
    {
        Monitor = monitor;

        _width = this
            .WhenAnyValue(e => e.Monitor.PhysicalRotated.OutsideWidth)
            .ToProperty(this,e => e.Width);

        _height = this
            .WhenAnyValue(e => e.Monitor.PhysicalRotated.OutsideHeight)
            .ToProperty(this,e => e.Height);

        _x = this
            .WhenAnyValue(e => e.Monitor.DepthProjection.X)
            .ToProperty(this, e => e.X);

        _y = this
            .WhenAnyValue(e => e.Monitor.DepthProjection.Y)
            .ToProperty(this, e => e.Y);
    }

    public PhysicalMonitor Monitor { get; }

    public double Width => _width.Value;
    readonly ObservableAsPropertyHelper<double> _width;

    public double Height => _height.Value;
    readonly ObservableAsPropertyHelper<double> _height;

    public double X
    {
        get => _x.Value;
        set => Monitor.DepthProjection.X = value;
    }
    readonly ObservableAsPropertyHelper<double> _x;

    public double Y
    {
        get => _y.Value;
        set => Monitor.DepthProjection.Y = value;
    }
    readonly ObservableAsPropertyHelper<double> _y;
}