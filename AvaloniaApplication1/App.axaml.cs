using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1;

public partial class App : Application
{
    public static IContainer? ServiceProvider { get; private set; }

    /// <summary>
    /// Setup IoC.
    /// <para>
    /// We don't need to do it in the <see cref="OnFrameworkInitializationCompleted"/> as in the <a href="https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-implement-dependency-injection#step-3-modify-appaxamlcs">Avalonia's doc</a>
    /// because we don't actually create anything related to Avalonia here..
    /// </para>
    /// </summary>
    static App()
    {
        //Microsoft-things must to be registered first (in the DI Graph)
        ServiceCollection services = new();
        services.AddLoggingService();

        //Everything-else is added (registered) on top of the Microsoft-things (to the DI Graph)
        ServiceProvider = new DryIocServiceProviderFactory().CreateBuilder(services).Container;
        ServiceProvider.RegisterApplicationServices();

        //Make application crashed on startup when DryIoc detected something wrong with your IoC setup
        //(For eg: when you forgot to register something to the DI Graph)
        ServiceProvider.ValidateAndThrow();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
