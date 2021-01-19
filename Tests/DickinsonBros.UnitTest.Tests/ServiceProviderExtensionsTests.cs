using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System;

namespace DickinsonBros.UnitTest.Tests
{
    [TestClass]
    public class ServiceProviderExtensionsTests
    {
        [TestMethod]
        public void GetMock_Runs_ReturnsMock()
        {
            //Setup
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Mock.Of<IDemoTwoClass>());
            var expectedValue = "A";
            var observedValue = "";

            //Act
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {

                var demoClass = serviceProvider.GetMock<IDemoTwoClass>();
                demoClass.Setup(e => e.Speak()).Returns(expectedValue);
                observedValue = demoClass.Object.Speak();
            }

            //Assert
            Assert.AreEqual(expectedValue, observedValue);
        }

        [TestMethod]
        public void GetControllerInstance_Runs_AddContextToClass()
        {
            //Setup
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<APIDemoClass>();
            var demoAPIClass = (APIDemoClass)null;

            //Act
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                demoAPIClass = serviceProvider.GetControllerInstance<APIDemoClass>();
            }

            //Assert
            Assert.IsNotNull(demoAPIClass.HttpContext);
        }

        [TestMethod]
        public void GetControllerInstance_WithHeaders_AddsHeaderToRequest()
        {
            //Setup
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<APIDemoClass>();
            var headers = new Dictionary<string, string>
            {
                { "X-Correlation-ID", "CollerationId"}
            };
            var demoAPIClass = (APIDemoClass)null;

            //Act
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                demoAPIClass = serviceProvider.GetControllerInstance<APIDemoClass>(headers);
                demoAPIClass.Speak();
            }

            //Assert
            Assert.AreEqual(1, demoAPIClass.HttpContext.Request.Headers.Count());
            Assert.AreEqual(headers.First().Key, demoAPIClass.HttpContext.Request.Headers.First().Key);
            Assert.AreEqual(headers.First().Value, demoAPIClass.HttpContext.Request.Headers.First().Value.ToString());
        }

        [TestMethod]
        public void GetControllerInstance_WithClaims_AddsClaimsToContext()
        {
            //Setup
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<APIDemoClass>();
            var demoAPIClass = (APIDemoClass)null;

            var expectedUserId = "1";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier.ToString(), expectedUserId)
            };

            //Act
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                demoAPIClass = serviceProvider.GetControllerInstance<APIDemoClass>(claims);
                demoAPIClass.Speak();
            }

            //Assert

            Assert.AreEqual(1, demoAPIClass.User.Claims.Count());
            Assert.AreEqual(expectedUserId, demoAPIClass.User.Claims.First().Value);
            Assert.AreEqual(ClaimTypes.NameIdentifier.ToString(), demoAPIClass.User.Claims.First().Type);
        }

        [TestMethod]
        public void GetControllerInstance_WithHeadersAndClaims_AddsHeaderToRequestAndClaimsToContext()
        {
            //Setup
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<APIDemoClass>();
            var headers = new Dictionary<string, string>
            {
                { "X-Correlation-ID", "CollerationId"}
            };
            var demoAPIClass = (APIDemoClass)null;

            var expectedUserId = "1";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier.ToString(), expectedUserId)
            };

            //Act
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                demoAPIClass = serviceProvider.GetControllerInstance<APIDemoClass>(headers, claims);
                demoAPIClass.Speak();
            }

            //Assert
            Assert.AreEqual(1, demoAPIClass.HttpContext.Request.Headers.Count());
            Assert.AreEqual(headers.First().Key, demoAPIClass.HttpContext.Request.Headers.First().Key);
            Assert.AreEqual(headers.First().Value, demoAPIClass.HttpContext.Request.Headers.First().Value.ToString());

            Assert.AreEqual(1, demoAPIClass.User.Claims.Count());
            Assert.AreEqual(expectedUserId, demoAPIClass.User.Claims.First().Value);
            Assert.AreEqual(ClaimTypes.NameIdentifier.ToString(), demoAPIClass.User.Claims.First().Type);
        }
    }
    public class APIDemoClass : ControllerBase, IAPIDemoClass
    {
        public string Speak()
        {
            return "Hi";
        }
    }

    public interface IAPIDemoClass
    {
        string Speak();
    }

}
