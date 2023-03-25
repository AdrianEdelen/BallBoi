namespace BallBoi_Test;
using BallBoi;

public class VersionReporter_Test
{

    




    [Fact]
    public void Version_Test()
    {
        //we have code that can result in the return value being the string "Null" = valid result
        //we also have code that can return a string value with a correct version number = valid result.


        //version example "[0.0.0.1]";

        var nullCase = new VersionReporter("null");
        var successCase = new VersionReporter("[0.0.0.1]");


        var ver = VersionReporter.Version;


        //assert null if version number is null;
        Assert.Equal(nullCase, "null");

        Assert.True(Version == "Null" || ver.regex)


        //assert 0.0.0.1 if version number is 0.0.0.1
        Assert.Equal(successCase, "[0.0.0.1]");



    }

}