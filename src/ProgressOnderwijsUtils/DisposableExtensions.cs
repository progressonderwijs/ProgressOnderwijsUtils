namespace ProgressOnderwijsUtils;

public static class DisposableExtensions
{
    public static T Using<TDisposable, T>(this TDisposable disposable, Func<TDisposable, T> func)
        where TDisposable : IDisposable
    {
        using (disposable) {
            return func(disposable);
        }
    }
}