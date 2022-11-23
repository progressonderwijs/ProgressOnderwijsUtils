using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace ProgressOnderwijsUtils.SeleniumWebDriverPool;

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

    [Obsolete("Will be removed")]
    public static WebDriverPool ChromePool(bool runHeadless, bool supressContentPopups, string? downloadDirectory)
    {
        var preferences = new List<(string Name, object Value)>();
        var arguments = new List<string>();
        if (!supressContentPopups) {
            preferences.Add(("profile.default_content_settings.popups", 0));
        }
        if (!string.IsNullOrEmpty(downloadDirectory)) {
            preferences.Add(("download.default_directory", downloadDirectory));
        }

        if (!supressContentPopups || !string.IsNullOrEmpty(downloadDirectory)) {
            preferences.Add(("download.directory_upgrade", 1));
        }

        if (runHeadless) {
            arguments.Add("headless");
        }
        arguments.Add("window-size=1920,1080");
        return ChromePool(preferences.ToArray(), arguments.ToArray());
    }

    public static WebDriverPool ChromePool((string Name, object Value)[] profilePreferences, string[] arguments)
    {
        var driverService = ChromeDriverService.CreateDefaultService();
        driverService.Start();
        var driverOptions = new ChromeOptions { AcceptInsecureCertificates = true, Proxy = null, };
        driverOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
        driverOptions.SetLoggingPreference(LogType.Driver, LogLevel.Warning);
        foreach (var argument in arguments) {
            driverOptions.AddArgument(argument);
        }
        foreach (var profilePreference in profilePreferences) {
            driverOptions.AddUserProfilePreference(profilePreference.Name, profilePreference.Value);
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
