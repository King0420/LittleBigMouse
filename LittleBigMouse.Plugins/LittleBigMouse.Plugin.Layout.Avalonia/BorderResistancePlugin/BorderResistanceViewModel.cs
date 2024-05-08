﻿/*
  LittleBigMouse.Plugin.Location
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of LittleBigMouse.Plugin.Location.

    LittleBigMouse.Plugin.Location is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LittleBigMouse.Plugin.Location is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using Avalonia;
using HLab.Mvvm.ReactiveUI;
using LittleBigMouse.DisplayLayout.Monitors;
using LittleBigMouse.Plugin.Layout.Avalonia.SizePlugin;
using ReactiveUI;

namespace LittleBigMouse.Plugin.Layout.Avalonia.BorderResistancePlugin;


internal class BorderResistanceViewModel : ViewModel<PhysicalMonitor>
{
    public BorderResistanceViewModel()
    {


        _height = this.WhenAnyValue(e => e.Model.PhysicalRotated.Height)
            .ToProperty(this, e => e.Height);

        _width = this.WhenAnyValue(e => e.Model.PhysicalRotated.Width)
            .ToProperty(this, e => e.Width);

        _topBorder = this.WhenAnyValue(e => e.Model.PhysicalRotated.TopBorder)
            .ToProperty(this, e => e.TopBorder);

        _rightBorder = this.WhenAnyValue(e => e.Model.PhysicalRotated.RightBorder)
            .ToProperty(this, e => e.RightBorder);

        _bottomBorder = this.WhenAnyValue(e => e.Model.PhysicalRotated.BottomBorder)
            .ToProperty(this, e => e.BottomBorder);

        _leftBorder = this.WhenAnyValue(e => e.Model.PhysicalRotated.LeftBorder)
            .ToProperty(this, e => e.LeftBorder);

        _outsideHeight = this.WhenAnyValue(e => e.Model.PhysicalRotated.OutsideHeight)
            .ToProperty(this, e => e.OutsideHeight);

        _outsideWidth = this.WhenAnyValue(e => e.Model.PhysicalRotated.OutsideWidth)
            .ToProperty(this, e => e.OutsideWidth);
    }


    //Inside Vertical
    public Arrow InsideVerticalArrow
    {
        get => _insideVerticalArrow;
        set => this.RaiseAndSetIfChanged(ref _insideVerticalArrow, value);
    }
    Arrow _insideVerticalArrow;

    //Inside Horizontal
    public Arrow InsideHorizontalArrow
    {
        get => _insideHorizontalArrow;
        set => this.RaiseAndSetIfChanged(ref _insideHorizontalArrow, value);
    }
    Arrow _insideHorizontalArrow;

    //Outside Vertical
    public Arrow OutsideVerticalArrow
    {
        get => _outsideVerticalArrow;
        set => this.RaiseAndSetIfChanged(ref _outsideVerticalArrow, value);
    }
    Arrow _outsideVerticalArrow;

    //Outside Horizontal
    public Arrow OutsideHorizontalArrow
    {
        get => _outsideHorizontalArrow;
        set => this.RaiseAndSetIfChanged(ref _outsideHorizontalArrow, value);
    }
    Arrow _outsideHorizontalArrow;

    //Left Border
    public Arrow LeftBorderArrow
    {
        get => _leftBorderArrow;
        set => this.RaiseAndSetIfChanged(ref _leftBorderArrow, value);
    }
    Arrow _leftBorderArrow;

    //Top Border
    public Arrow TopBorderArrow
    {
        get => _topBorderArrow;
        set => this.RaiseAndSetIfChanged(ref _topBorderArrow, value);
    }
    Arrow _topBorderArrow;

    //Right Border
    public Arrow RightBorderArrow
    {
        get => _rightBorderArrow;
        set => this.RaiseAndSetIfChanged(ref _rightBorderArrow, value);
    }
    Arrow _rightBorderArrow;

    //Bottom Border
    public Arrow BottomBorderArrow
    {
        get => _bottomBorderArrow;
        set => this.RaiseAndSetIfChanged(ref _bottomBorderArrow, value);
    }
    Arrow _bottomBorderArrow;



    public double Height
    {
        get => _height.Value;
        set
        {
            Model.PhysicalRotated.Height = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _height;


    public double Width
    {
        get => _width.Value;
        set
        {
            Model.PhysicalRotated.Width = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _width;


    public double TopBorder
    {
        get => _topBorder.Value;
        set
        {
            Model.PhysicalRotated.TopBorder = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _topBorder;


    public double RightBorder
    {
        get => _rightBorder.Value;
        set
        {
            Model.PhysicalRotated.RightBorder = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _rightBorder;


    public double BottomBorder
    {
        get => _bottomBorder.Value;
        set
        {
            Model.PhysicalRotated.BottomBorder = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _bottomBorder;


    public double LeftBorder
    {
        get => _leftBorder.Value;
        set
        {
            Model.PhysicalRotated.LeftBorder = value;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _leftBorder;


    public double OutsideHeight
    {
        get => _outsideHeight.Value;
        set
        {
            var offset = value - OutsideHeight;
            Model.PhysicalRotated.BottomBorder += offset;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _outsideHeight;


    public double OutsideWidth
    {
        get => _outsideWidth.Value;
        set
        {
            var offset = (value - OutsideWidth) / 2;
            Model.PhysicalRotated.LeftBorder += offset;
            Model.PhysicalRotated.RightBorder += offset;
            Model.Layout.Compact();
        }
    }
    readonly ObservableAsPropertyHelper<double> _outsideWidth;

}