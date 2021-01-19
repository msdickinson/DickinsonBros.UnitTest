using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoreLinq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DickinsonBros.UnitTest
{
    public abstract class BaseTest
    {
        public void RunDependencyInjectedTest(Action<IServiceProvider> callback, params Action<IServiceCollection>[] serviceCollectionConfigurators)
        {
            RunDependencyInjectedTestAsync(
                async serviceProvider =>
                {
                    callback(serviceProvider);
                    await Task.CompletedTask;
                },
                serviceCollectionConfigurators
            ).GetAwaiter().GetResult();
        }
        public async Task RunDependencyInjectedTestAsync(Func<IServiceProvider, Task> callback, params Action<IServiceCollection>[] serviceCollectionConfigurators)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();

            var IConfigurationRootMock = new Mock<IConfiguration>();
            serviceCollection.AddSingleton(IConfigurationRootMock.Object);

            serviceCollectionConfigurators.ForEach(serviceCollectionConfigurator => serviceCollectionConfigurator(serviceCollection));

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            await callback(serviceProvider);
        }

        public IConfigurationRoot BuildConfigurationRoot<T>(T options)
        {
            var jsonOptionsWithRoot = (string)null;

            if(typeof(T).GetGenericArguments().Any())
            {
                var jsonOptions = System.Text.Json.JsonSerializer.Serialize(options);
                jsonOptionsWithRoot = $"{{\"{typeof(T).BaseType.Name}\": {{ \"{typeof(T).GetGenericArguments().First().Name}\": {jsonOptions} }} }}";
            }
            else
            {
                var jsonOptions = System.Text.Json.JsonSerializer.Serialize(options);
                jsonOptionsWithRoot = $"{{\"{typeof(T).Name}\": {jsonOptions}}}";
            }

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(jsonOptionsWithRoot));
            return new ConfigurationBuilder().AddJsonStream(stream).Build();
        }

    }
}
