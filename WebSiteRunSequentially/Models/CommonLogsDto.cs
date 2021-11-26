using Test.EfCore;

namespace WebSiteRunSequentially.Models
{
    public class CommonLogsDto
    {
        public CommonNameDateTime Common { get; }
        public List<NameDateTime> Logs { get; }

        public CommonLogsDto(CommonNameDateTime common, List<NameDateTime> logs)
        {
            Common = common;
            Logs = logs;
        }
    }
}
