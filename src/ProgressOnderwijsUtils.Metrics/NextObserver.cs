namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The diagnostic listeners never complete nor fail, so only their values are of interest.
/// </summary>
sealed class NextObserver<T>(Action<T> onNext) : IObserver<T>
{
    public void OnNext(T value)
        => onNext(value);

    public void OnCompleted() { }

    public void OnError(Exception error) { }
}
