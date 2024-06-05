﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.CustomComponent;
using SystemOfThermometry3.WinUIWorker;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        Windows.UI.ViewManagement.ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
        appView.SetPreferredMinSize(new Size(500, 300));
        //WinUIWorker.WinUIWorker mainForm = new WinUIWorker.WinUIWorker();
        //mainForm.loadProgram();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Frame rootFrame = Window.Current.Content as Frame;

        if (rootFrame == null)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;
        }
        if (rootFrame.Content == null)
        {
            // Переход во фрейме к странице MainPage
            rootFrame.Navigate(typeof(MainWindow), args.Arguments);
        }
        Window.Current.Activate();

        
        //WinUIWorker.WinUIWorker worker  = new WinUIWorker.WinUIWorker();
        //worker.loadProgram();

        //m_window = new MainWindow();

    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private Window m_window;
}
