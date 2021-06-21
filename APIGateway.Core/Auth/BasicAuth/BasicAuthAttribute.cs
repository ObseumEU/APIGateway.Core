using System;
using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Core.Auth.BasicAuth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthAttribute : TypeFilterAttribute
    {
        public BasicAuthAttribute(string scope = "default") : base(typeof(BasicAuthFilter))
        {
            Arguments = new object[] {scope};
        }
    }
}