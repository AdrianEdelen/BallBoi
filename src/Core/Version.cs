using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core;
public class Version
{
    private string _versionString;

    public int MajorVersionNumber { get; set; }
    public int MinorVersionNumber { get; set; }
    public int MajorRevisionNumber { get; set; }
    public int MinorRevisionNumber { get; set; }
    public string FullVersionNumberString => $"{MajorVersionNumber}.{MinorVersionNumber}.{MajorRevisionNumber}.{MinorRevisionNumber}";
    public Version()
    {
        _versionString = "0.0.0.0";
        BuildVersions();
    }
    public Version(string versionString)
    {
        _versionString = versionString;
        BuildVersions();
    }

    private string MatchVersionString()
    {
        Regex r = new Regex("\\d.\\d.\\d.\\d");
        Match match = r.Match(_versionString);

        return match.Value;
    }


    private void BuildVersions()
    {
        var versionString = MatchVersionString();
        var cleanVersion = versionString.Replace(".", string.Empty);
        var versionStringArray = cleanVersion.ToCharArray();

        try
        {
            MajorVersionNumber = int.Parse(cleanVersion[0].ToString());
            MinorVersionNumber = int.Parse(cleanVersion[1].ToString());
            MajorRevisionNumber = int.Parse(cleanVersion[2].ToString());
            MinorRevisionNumber = int.Parse(cleanVersion[3].ToString());
        }
        catch (FormatException ex)
        {
            //TODO: Error Handling
            throw new NotImplementedException();
        }
    }

}
