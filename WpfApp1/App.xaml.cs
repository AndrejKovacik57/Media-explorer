﻿using System.Configuration;
using System.Data;
using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow = new MainWindow()
        {
            DataContext = new MainWindowViewModel()
        };
        MainWindow.Show();
        base.OnStartup(e);

    }
}