namespace ProgressOnderwijsUtils.Tests;

public sealed class DateTagTest
{
    [Fact(Skip = "Extremely slow")]
    public void NoCollisionsSoon()
    {
        var start = DateTime.Now; //slightly evil in unit test;
        var seconds = (int)TimeSpan.FromDays(162).TotalSeconds;
        var dates = Enumerable.Range(0, seconds).Select(s => start + TimeSpan.FromSeconds(s));
        PAssert.That(() => dates.Count() > 10000000);
        var dateGroupsWithCollisions = dates.GroupBy(DateTimeShortAgeTag.ToAgeTag).Where(g => g.Count() > 1);
        PAssert.That(() => dateGroupsWithCollisions.None());
    }
}
