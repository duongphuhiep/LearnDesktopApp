using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AvaloniaApplication1Tests;

public class DryIocIntegrationTests(ITestOutputHelper _outputHelper)
{
    [Fact]
    public void MsDryIoc_CombinedRegistrationsTest()
    {
        //Microsoft-things must to be registered first
        ServiceCollection services = new();
        services.AddLogging(builder
            => builder.AddConsole()
                .AddXUnit(_outputHelper)
                .SetMinimumLevel(LogLevel.Debug));

        //Everything-else registration is added on top of the Microsoft-things
        var container = new DryIocServiceProviderFactory().CreateBuilder(services).Container;
        container.Register<IDep, X>();
        container.GetRequiredService<IDep>().ShouldBeOfType<X>();

        //Replace the X implementation with a different one (a Mock in the test for eg)
        container.Register<IDep, Y>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        container.GetRequiredService<IDep>().ShouldBeOfType<Y>();

        //Resolve Microsoft-things works too
        var logger = container.GetRequiredService<ILogger<IDep>>();
        logger.LogInformation("alright");
    }


    [Fact]
    public void CloneContainerTest()
    {
        //Given container1 with X implementation, and Counter singleton
        var container1 = new Container();
        container1.Register<IDep, X>();
        container1.Register<Counter>(Reuse.Singleton);

        container1.Resolve<Counter>().Increment();
        container1.Resolve<Counter>().Count.ShouldBe(1);
        container1.Resolve<Counter>().Increment();
        container1.Resolve<Counter>().Count.ShouldBe(2);
        container1.Resolve<IDep>().ShouldBeOfType<X>();

        //When: container2 is a clone of container1
        var container2 = container1.With(container1.Rules, null, RegistrySharing.CloneAndDropCache, Container.NewSingletonScope());

        //Then: container2 has the same implementation as container1
        container2.Resolve<Counter>().Increment();
        container2.Resolve<Counter>().Count.ShouldBe(1);
        container2.Resolve<Counter>().Increment();
        container2.Resolve<Counter>().Count.ShouldBe(2);
        container2.Resolve<IDep>().ShouldBeOfType<X>();

        //When: Replace the X implementation with Y in the container2
        container2.Register<IDep, Y>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        container2.GetRequiredService<IDep>().ShouldBeOfType<Y>();

        //Then: container1 is not affected by the container2
        container1.GetRequiredService<IDep>().ShouldBeOfType<X>();

        //Then: container1 singleton is not affected by the container2 singleton
        container1.Resolve<Counter>().Increment();
        container1.Resolve<Counter>().Count.ShouldBe(3);

        container2.Resolve<Counter>().Count.ShouldBe(2);
    }


    interface IDep;
    class X : IDep;
    class Y : IDep;
    class Counter
    {
        public int Count { get; private set; }
        public void Increment()
        {
            Count++;
        }
    }
}
