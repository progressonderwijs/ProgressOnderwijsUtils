namespace ProgressOnderwijsUtils.Tests;

public static class DateTimeExtensionsTest
{
    [Fact]
    public static void Overlapt_1_voor_en_in_2()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 1, 15), new(2000, 2, 15));
        AssertOverlapt(periode1, periode2, true);
    }

    [Fact]
    public static void Overlapt_2_voor_en_in_1()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 1, 15), new(2000, 2, 15));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        AssertOverlapt(periode1, periode2, true);
    }

    [Fact]
    public static void Overlapt_1_in_2()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 1, 15), new(2000, 1, 25));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        AssertOverlapt(periode1, periode2, true);
    }

    [Fact]
    public static void Overlapt_2_in_1()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 1, 15), new(2000, 1, 25));
        AssertOverlapt(periode1, periode2, true);
    }

    [Fact]
    public static void Overlapt_1_voor_2()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 2, 1), new(2000, 3, 1));
        AssertOverlapt(periode1, periode2, false);
    }

    [Fact]
    public static void Overlapt_2_voor_1()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 2, 1), new(2000, 3, 1));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 1, 1), new(2000, 2, 1));
        AssertOverlapt(periode1, periode2, false);
    }

    [Fact]
    public static void Overlapt_1_is_2()
    {
        var periode1 = new DateTimeExtensions.Periode(new(2000, 2, 1), new(2000, 3, 1));
        var periode2 = new DateTimeExtensions.Periode(new(2000, 2, 1), new(2000, 3, 1));
        AssertOverlapt(periode1, periode2, true);
    }

    static void AssertOverlapt(DateTimeExtensions.Periode periode1, DateTimeExtensions.Periode periode2, bool uitkomst)
        => PAssert.That(() => periode1.Overlapt(periode2) == uitkomst);

    [Fact]
    public static void ToDateOnly_ReturnsCorrectDateOnly()
    {
        var dateTime = new DateTime(2024, 3, 15, 10, 30, 45);
        var result = dateTime.ToDateOnly();
        PAssert.That(() => result == new DateOnly(2024, 3, 15));
    }

    [Fact]
    public static void ToDateOnly_Nullable_WithValue_ReturnsCorrectDateOnly()
    {
        DateTime? dateTime = new DateTime(2024, 3, 15, 10, 30, 45);
        var result = dateTime.ToDateOnly();
        PAssert.That(() => result == new DateOnly(2024, 3, 15));
    }

    [Fact]
    public static void ToDateOnly_Nullable_WithNull_ReturnsNull()
    {
        DateTime? dateTime = null;
        var result = dateTime.ToDateOnly();
        PAssert.That(() => result == null);
    }
}
