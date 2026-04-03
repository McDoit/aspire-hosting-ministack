using Aspire.Hosting;

namespace McDoit.Aspire.Hosting.Ministack.TestAppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        _ = builder.Build();
    }
}
