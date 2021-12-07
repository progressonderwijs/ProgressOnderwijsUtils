using System.Drawing;
using System.Runtime.InteropServices;

namespace ProgressOnderwijsUtils;

public sealed class StringMeasurer
{
    readonly Func<string, double> measure;
    readonly ConcurrentDictionary<string, double> cache;
    const float ArbitraryFontSizeInPoints = 12.0f;
    const double ArbitraryFontSizeInPixels = 16.0; //default web font size, maar groter maken leek 0 impact te hebben.
    const string ellipsis = "…";
    readonly double ellipsis_ems;
    public static readonly StringMeasurer Instance = new();

    public StringMeasurer()
    {
        var graphics = Graphics.FromImage(new Bitmap(1, 1));
        var font = new Font("Verdana", ArbitraryFontSizeInPoints, FontStyle.Regular);
        var format = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces, };
        var sync = new object();
        measure = str => {
            lock (sync) {
                try {
                    return graphics.MeasureString(str, font, new PointF(0, 0), format).Width;
                } catch (ExternalException exception) when (exception.Message == "A generic error occurred in GDI+.") {
                    //Soms werkt dit niet, bv met U+FE0F.
                    //Vermoedelijk mogen die scalars niet los voorkomen en is de input corrupt.
                    return 0;
                }
            }
        };
        cache = new(StringComparer.Ordinal);
        cache["\t"] = cache["\r"] = cache["\n"] = cache[" "] = (measure("a a a") - 3 * measure("a")) / 2;
        ellipsis_ems = Measure(ellipsis);
    }

    [Pure]
    public double Measure(string str)
    {
        var iter = StringInfo.GetTextElementEnumerator(str);
        double widthSum = 0;
        while (iter.MoveNext()) {
            var graphemeCluster = iter.GetTextElement();
            var graphemeWidth = cache.GetOrAdd(graphemeCluster, measure);
            widthSum += graphemeWidth;
        }
        return widthSum / ArbitraryFontSizeInPixels;
    }

    [Pure]
    string TrimToEms(string str, double ems)
    {
        var builder = new StringBuilder(str.Length);
        var iter = StringInfo.GetTextElementEnumerator(str);
        var widthSum = 0.0;
        var maxPixels = ems * ArbitraryFontSizeInPixels;

        while (iter.MoveNext()) {
            var graphemeCluster = iter.GetTextElement();
            widthSum += cache.GetOrAdd(graphemeCluster, measure);
            if (maxPixels < widthSum) {
                break;
            }
            _ = builder.Append(graphemeCluster);
        }
        return builder.ToString();
    }

    [Pure]
    public (string result, bool didShorten) ElideIfNecessary(string s, double ems)
        => Measure(s) < ems ? (s, false) : (TrimToEms(s, ems - ellipsis_ems) + ellipsis, true);
}