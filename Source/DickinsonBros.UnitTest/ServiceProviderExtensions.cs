using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DickinsonBros.UnitTest
{
    public static class ServiceProviderExtensions
    {
        public static Mock<T> GetMock<T>(this IServiceProvider serviceProvider)
            where T : class
        {
            return Moq.Mock.Get<T>(serviceProvider.GetRequiredService<T>());
        }

        public static T GetControllerInstance<T>(this IServiceProvider serviceProvider) where T : ControllerBase
        {
            return serviceProvider.GetControllerInstance<T>(new Dictionary<string, string>(), new List<Claim>());
        }

        public static T GetControllerInstance<T>(this IServiceProvider serviceProvider, IEnumerable<Claim> claims) where T : ControllerBase
        {
            return serviceProvider.GetControllerInstance<T>(new Dictionary<string, string>(), claims);
        }

        public static T GetControllerInstance<T>(this IServiceProvider serviceProvider, IDictionary<string, string> headers) where T : ControllerBase
        {
            return serviceProvider.GetControllerInstance<T>(headers, new List<Claim>());
        }

        public static T GetControllerInstance<T>(this IServiceProvider serviceProvider, IDictionary<string, string> headers, IEnumerable<Claim> claims) where T : ControllerBase
        {
            var httpContextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();

            var principal = new ClaimsPrincipal();

            var responseHeaderDictionary = new HeaderDictionary();


            httpContextMock
                .SetupGet(httpContext => httpContext.Request)
                .Returns(() => httpRequestMock.Object);

            httpContextMock
            .SetupGet(httpContext => httpContext.User.Claims)
            .Returns(() => claims);

            httpRequestMock
                .SetupGet(httpRequest => httpRequest.Headers)
                .Returns(() =>
                {
                    var headerDictionary = new HeaderDictionary();

                    foreach (var header in headers)
                    {
                        headerDictionary.Add(header.Key, header.Value);
                    }
                    return headerDictionary;
                });

            var uut = serviceProvider.GetRequiredService<T>();

            uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            return uut;
        }
    }
}
