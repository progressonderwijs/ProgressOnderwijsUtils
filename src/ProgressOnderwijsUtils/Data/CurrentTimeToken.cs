namespace ProgressOnderwijsUtils;

public sealed class CurrentTimeToken
{
    CurrentTimeToken() { }
    public static readonly CurrentTimeToken Instance = new();

    public static CurrentTimeToken Parse(string s)
    {
        if (s != "") {
            throw new ArgumentException("Can only parse empty string as current time token!");
        }
        return Instance;
    }
}
