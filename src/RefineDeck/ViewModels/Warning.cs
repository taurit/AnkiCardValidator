﻿using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class Warning
{
    public string Severity { get; set; }
}
