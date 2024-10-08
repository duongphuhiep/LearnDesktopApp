﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication1.ViewModels;

public partial class MessageBoxViewModel : ObservableObject
{
    [ObservableProperty]
    private string _message;
}
public class MessageBoxViewModelForDesigner : MessageBoxViewModel
{
    public MessageBoxViewModelForDesigner()
    {
        Message = "Dialog message";
    }
}