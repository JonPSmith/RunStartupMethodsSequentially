using Benchmark;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;
using Test.ServicesToCall;
using TestSupport.EfHelpers;

public class Program
{
    static void Main(string[] args) => BenchmarkRunner.Run<Program>();

    private TestDbContext _context;
    private IGetLockAndThenRunServices _lockAndRun;

    [GlobalSetup]
    public void Setup()
    {

        var options = this.CreateUniqueClassOptions<TestDbContext>();
        _context = new TestDbContext(options);
        _context.Database.EnsureClean();
    }

    [IterationSetup]
    public void SetupServices()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(dbOptions =>
            dbOptions.UseSqlServer(_context.Database.GetConnectionString()));
        var options = services.RegisterRunMethodsSequentially(options =>
        {
            options.RegisterAsHostedService = false;
            options.AddSqlServerLockAndRunMethods(_context.Database.GetConnectionString());
            //options.AddRunMethodsWithoutLock();
            //options.RegisterServiceToRunInJob<DoNothingStartupService>();
            options.RegisterServiceToRunInJob<UpdateDatabase1>();
            options.RegisterServiceToRunInJob<UpdateDatabase2>();
            //options.RegisterServiceToRunInJob<UpdateDatabaseUseScoped1>();
            //options.RegisterServiceToRunInJob<UpdateDatabaseUseScoped2>();
        });

        for (int i = 0; i < 200; i++)
        {
            services.AddTransient<DummyService>();
        }

        var serviceProvider = services.BuildServiceProvider();
        _lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();
    }

    [Benchmark]
    public async Task RunLockAndRun()
    {
        await _lockAndRun.LockAndLoadAsync();
    }
}
