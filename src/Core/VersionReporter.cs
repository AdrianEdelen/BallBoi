using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BallBoi;
public class VersionReporter
{
    public Core.Version CurrentVersion => GetCurrentVersion();

    public Core.Version NextVersion => GetLatestVersionNumber().Result;

    public string? NextVersionDescription { get; set; }
    public DateTime? NextVersionDate { get; set; }
    public string? NextVersionType { get; set; }


    public Core.Version GetCurrentVersion()
    {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        Core.Version version = new(fvi.FileVersion ?? "NULL");
        return version;
    }

    public async Task<Core.Version> GetLatestVersionNumber()
    {
        try
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://api.github.com/repos/adrianedelen/ballboi/releases/latest");

            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#ballboi");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            Core.Version version;
            HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/adrianedelen/ballboi/releases/latest").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic releaseInfo = JsonConvert.DeserializeObject<dynamic>(result);


                NextVersionDescription = releaseInfo.body.ToString();
                NextVersionDate = DateTime.Parse(releaseInfo.created_at.ToString());
                NextVersionType = bool.Parse(releaseInfo.prerelease.ToString()) ? "Optional" : "Recommended";

                string releaseName = releaseInfo.name.ToString();
                version = new Core.Version(releaseName);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                throw new NotImplementedException();
                version = new Core.Version();
            }

            return version;
            client.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Invalid Version Number");
            return new Core.Version();
        }
        
    }

    public string GetReleaseStream()
    {
        throw new NotImplementedException();
    }

    /// <returns>True if the next version is higher than the current version</returns>
    public bool IsNewerVersionAvailable()
    {
        var cur = CurrentVersion;
        var nex = NextVersion;

        if (cur.MajorVersionNumber > nex.MajorVersionNumber) return false;
        if (cur.MinorVersionNumber > nex.MinorVersionNumber) return false;
        if (cur.MajorRevisionNumber > nex.MajorVersionNumber) return false;
        if (cur.MinorRevisionNumber > nex.MinorRevisionNumber) return false;
        if (cur.FullVersionNumberString == nex.FullVersionNumberString) return false;
        return true;
    }
}

