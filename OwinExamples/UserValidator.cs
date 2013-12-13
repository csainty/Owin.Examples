using System.Threading.Tasks;
using Environment = System.Collections.Generic.IDictionary<string, object>;

namespace OwinExamples
{
    public interface IUserValidator
    {
        Task<bool> IsLoggedIn(Environment environment);
    }

    public class RouteUserValidator : IUserValidator
    {
        public Task<bool> IsLoggedIn(Environment environment)
        {
            return Task.FromResult(environment["owin.RequestPath"].ToString() == "/auth");
        }
    }
}