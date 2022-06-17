using System.Configuration;
using System.Diagnostics;
using System.Text;
using Cronical.Integrations;
using DotNetCommons.Security;
using Serilog;

namespace Cronical.MySql;

/// <summary>
/// Integration for loading jobs from a MySQL database. Depends on a database schema
/// in the accompanying file.
/// </summary>
public class MySqlIntegration : IIntegration
{
    private string _connectionString;
    private DateTime _lastCheck;
    private ulong _ownerId;

    /// <summary>
    /// Initialize the integration.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool Initialize(GlobalSettings settings)
    {
        _ownerId = ((ulong)Crc32.ComputeChecksum(Encoding.Default.GetBytes(Environment.MachineName)) << 32) | (uint)Process.GetCurrentProcess().Id;

        try
        {
            var connection = ConfigurationManager.AppSettings["Cronical.MySql.Connection"];
            if (string.IsNullOrEmpty(connection))
                throw new Exception("No connection specified in AppSettings 'Cronical.MySql.Connection'");

            _connectionString = ConfigurationManager.ConnectionStrings[connection]?.ConnectionString;
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception($"No ConnectionString defined for connection '{connection}'");

            Log.Information($"Cronical.MySql: Using connection {connection} with ownerId={_ownerId:X8}");
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Cronical.MySql: Error while initializing integration: {e.Message}; disabling MySQL integration");
            return false;
        }
    }

    /// <summary>
    /// Reload the job list. Only checks the tables every 60 seconds; the rest of the time it returns
    /// NoChange. Uses a single job queue defined in the "jobs" table - can only handle SingleJob objects
    /// for now, no provision exists for actual cron jobs run regularly or service jobs.
    /// </summary>
    /// <param name="defaultSettings"></param>
    /// <returns></returns>
    public (JobLoadResult, List<ICronicalJob>) FetchJobs(JobSettings defaultSettings)
    {
        try
        {
            if ((DateTime.Now - _lastCheck).TotalSeconds < 60)
                return (JobLoadResult.NoChange, null);

            _lastCheck = DateTime.Now;

            using (var db = Helper.GetConnection(_connectionString))
            {
                var rows = db.Query($@"
                        update jobs set owner={_ownerId} where owner is null and (time is null or time <= now()) limit 10;
                        select * from jobs where owner={_ownerId};
                        delete from jobs where owner={_ownerId};
                    ")
                    .Select(row => (Job)new SingleJob
                    {
                        Command = (string)row.command,
                        Tag = (string)row.tag
                    })
                    .ToList();

                return rows.Any() ? (JobLoadResult.AddJobs, rows) : (JobLoadResult.NoChange, null);
            }
        }
        catch (Exception e)
        {
            Log.Error("Cronical.MySql: Error while loading jobs: " + e.Message);
            return (JobLoadResult.NoChange, null);
        }
    }

    public void Completed(Job job)
    {
        // TODO
    }

    public void Shutdown()
    {
        Log.Information("Cronical.MySql: Shutting down");
    }
}