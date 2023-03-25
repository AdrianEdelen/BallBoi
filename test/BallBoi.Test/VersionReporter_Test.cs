namespace BallBoi_Test;
using BallBoi;
using System.Text.RegularExpressions;

public class VersionReporter_Test
{

    [Fact]
    public void Version_Test()
    {
        //version example "0.0.0.1";
        var version = VersionReporter.Version;
        Regex r = new Regex("^\\d.\\d.\\d.\\d$");
        Assert.True(version == "NULL" || r.IsMatch(version));
    }

}