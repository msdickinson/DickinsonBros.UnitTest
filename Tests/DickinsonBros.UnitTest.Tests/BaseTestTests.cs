using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace DickinsonBros.UnitTest.Tests
{
    [TestClass]
    public class BaseTestTests
    {
        [TestMethod]
        public void RunDependencyInjectedTest_UsesMockAndService_DoesNotThrows()
        {
            //Setup
            var uut = new DemoClassWithBaseTest();

            //Act
            uut.Run();

            //Assert
            //Does Not Throw
        }

        [TestMethod]
        public async Task RunDependencyInjectedTestAsync_UsesMockAndService_DoesNotThrows()
        {
            //Setup
            var uut = new DemoClassWithBaseTest();

            //Act
            await uut.RunAsync();

            //Assert
            //Does Not Throw
        }

        [TestMethod]
        public async Task BuildConfigurationRoot_Runs_ReturnsConfigurationRoot()
        {
            //Setup
            var sampleOptions = new SampleOptions
            {
                Token = "SampleToken"
            };
            
            var uut = new DemoClassWithBaseTest();

            //Act
            var configurationRoot = uut.BuildConfigurationRoot(sampleOptions);

            //Assert
            Assert.IsNotNull(configurationRoot);
            Assert.AreEqual(sampleOptions.Token, configurationRoot.GetValue<string>("SampleOptions:Token"));

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [TestMethod]
        public async Task BuildConfigurationRoot_OfT_ReturnsConfigurationRoot()
        {
            //Setup
            var sampleOptions = new SampleOptions<DemoClass>
            {
                Token = "SampleToken"
            };

            var uut = new DemoClassWithBaseTest();

            //Act
            var configurationRoot = uut.BuildConfigurationRoot(sampleOptions);

            //Assert
            Assert.IsNotNull(configurationRoot);
            Assert.AreEqual(sampleOptions.Token, configurationRoot.GetValue<string>("SampleOptions:DemoClass:Token"));

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    public class DemoClassWithBaseTest : BaseTest
    {
        public void Run()
        {
            RunDependencyInjectedTest
            (
                (serviceProvider) =>
                {
                    var demoClass = serviceProvider.GetRequiredService<IDemoClass>();
                    var demoTwoClass = serviceProvider.GetMock<IDemoTwoClass>();

                    demoTwoClass.Setup(e => e.Speak()).Returns("A");
                    demoClass.Speak();
                },
                serviceCollection =>
                {
                    serviceCollection.AddSingleton<IDemoClass, DemoClass>();
                    serviceCollection.AddSingleton(Mock.Of<IDemoTwoClass>());
                }
            );
        }

        public async Task RunAsync()
        {
            await RunDependencyInjectedTestAsync
            (
                async (serviceProvider) =>
                {
                    var demoClass = serviceProvider.GetRequiredService<IDemoClass>();
                    var demoTwoClass = serviceProvider.GetMock<IDemoTwoClass>();

                    demoTwoClass.Setup(e => e.Speak()).Returns("A");
                    demoTwoClass.Object.Speak();
                    demoClass.Speak();

                    await Task.CompletedTask;
                },
                serviceCollection =>
                {
                    serviceCollection.AddSingleton<IDemoClass, DemoClass>();
                    serviceCollection.AddSingleton(Mock.Of<IDemoTwoClass>());
                }
            );
        }


    
    }

    public class SampleOptions
    {
        public string Token { get; set; }
    }

    public class SampleOptions<T> : SampleOptions
    {
    }

    public class DemoClass : IDemoClass
    {
        public string Speak()
        {
            return "Hi";
        }
    }

    public interface IDemoClass
    {
        string Speak();
    }

    public interface IDemoTwoClass
    {
        string Speak();
    }
}