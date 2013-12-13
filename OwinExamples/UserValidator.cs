using System.Threading.Tasks;
using Environment = System.Collections.Generic.IDictionary<string, object>;

namespace OwinExamples
{
    public static class RouteUserValidator
    {
        public static Task<bool> Validator(Environment environment)
        {
            return Task.FromResult(environment["owin.RequestPath"].ToString() == "/auth");
        }
    }
}