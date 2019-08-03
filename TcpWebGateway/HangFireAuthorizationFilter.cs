using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace TcpWebGateway
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            //can add some more logic here...
            return true;
        }

    }
}
