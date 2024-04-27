﻿using System.Collections.Generic;
using HLab.Mvvm.Annotations;
using LittleBigMouse.Ui.Avalonia.Options;

namespace LittleBigMouse.Ui.Avalonia.Controls;

public class LocationControlViewModelDesign : IDesignViewModel
{
    public List<ListItem> AlgorithmList { get; } = new()
    {
        new ("strait","Strait","Simple and highly CPU-efficient transition."),
        new ("cross","Corner crossing","In direction-friendly manner, allows traversal through corners."),

    };

    public object SelectedAlgorithm { get; set; } = "Strait";
}