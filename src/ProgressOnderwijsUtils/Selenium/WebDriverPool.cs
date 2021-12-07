using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace ProgressOnderwijsUtils.Selenium;

/// <summary>
/// In theory, ChromeDriverServer (etc.) should use some form of pooling internally, so there's theoretically little need to here.
/// However... The first created driver has a ~18s overhead, and the next ones ~8s, whereas this pooling is much less than 1s.  So... no idea what DriverService is doing, but nothing very effective.
/// </summary>
public sealed class WebDriverPool : IDisposable
{
    bool disposed;
    public readonly ConcurrentBag<IWebDriver> cachedIdleDrivers; //TODO: in .net core 3, consider DisposableObjectPool
    readonly ICommandServer server;
    readonly Func<IWebDriver> driverFactory;

    public static WebDriverPool ChromePool(bool runHeadless)
    {
        var driverService = ChromeDriverService.CreateDefaultService();
        driverService.Start();
        var driverOptions = new ChromeOptions { AcceptInsecureCertificates = true, Proxy = null, };
        driverOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
        driverOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
        driverOptions.AddArgument("window-size=1920,1080");
        if (runHeadless) {
            driverOptions.AddArgument("headless");
        }

        return new(driverService, () => new ChromeDriver(driverService, driverOptions));
    }

    public WebDriverPool(ICommandServer server, Func<IWebDriver> driverFactory)
    {
        this.server = server;
        this.driverFactory = driverFactory;
        cachedIdleDrivers = new();
        AppDomain.CurrentDomain.DomainUnload += Cleanup;
        AppDomain.CurrentDomain.ProcessExit += Cleanup;
    }

    void Cleanup(object? sender, EventArgs e)
        => Dispose();

    public Rental RentDriver()
        => new(this, cachedIdleDrivers.TryTake(out var cached) ? cached : driverFactory());

    public readonly struct Rental : IDisposable
    {
        public readonly IWebDriver? WebDriver;
        readonly WebDriverPool? webDriverPool;

        public Rental(WebDriverPool webDriverPool, IWebDriver webDriver)
        {
            this.webDriverPool = webDriverPool;
            WebDriver = webDriver;
        }

        public void Dispose()
            => webDriverPool?.Return(WebDriver);
    }

    void Return(IWebDriver? webDriver)
    {
        if (webDriver != null) {
            webDriver.Manage().Cookies.DeleteAllCookies();
            cachedIdleDrivers.Add(webDriver);
        }
    }

    public void Dispose()
    {
        if (disposed) {
            return;
        }
        disposed = true;
        GC.SuppressFinalize(this);
        AppDomain.CurrentDomain.DomainUnload -= Cleanup;
        AppDomain.CurrentDomain.ProcessExit -= Cleanup;

        while (cachedIdleDrivers.TryTake(out var driver)) {
            driver.Dispose();
        }

        server.Dispose();
    }

    ~WebDriverPool()
        => Dispose();
}
