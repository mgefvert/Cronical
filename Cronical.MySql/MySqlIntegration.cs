using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cronical.Configuration;
using Cronical.Integrations;
using Cronical.Jobs;
using DotNetCommons.Logging;
using DotNetCommons.Security;

namespace Cronical.MySql
{
    public class MySqlIntegration : IIntegration
    {
        private string _connectionString;
        private LogChannel _logger;
        private DateTime _lastCheck;
        private ulong _ownerId;

        public bool Initialize(GlobalSettings settings, LogChannel logger)
        {
            _logger = logger;
            _ownerId = (Crc32.ComputeChecksum(Encoding.Default.GetBytes(Environment.MachineName)) << 32) | (uint)Process.GetCurrentProcess().Id;

            try
            {
                var connection = ConfigurationManager.AppSettings["Cronical.MySql.Connection"];
                if (string.IsNullOrEmpty(connection))
                    throw new Exception("No connection specified in AppSettings 'Cronical.MySql.Connection'");

                _connectionString = ConfigurationManager.ConnectionStrings[connection]?.ConnectionString;
                if (string.IsNullOrEmpty(_connectionString))
                    throw new Exception($"No ConnectionString defined for connection '{connection}'");

                _logger.Log($"Cronical.MySql: Using connection {connection} with ownerId={_ownerId:X8}");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"Cronical.MySql: Error while initializing integration: {e.Message}; disabling MySQL integration");
                return false;
            }
        }

        public List<SingleJob> FetchJobs(JobSettings defaultSettings)
        {
            try
            {
                if ((DateTime.Now - _lastCheck).TotalSeconds < 60)
                    return null;

                _lastCheck = DateTime.Now;

                using (var db = Helper.GetConnection("local"))
                {
                    var rows = db.Query($@"
                        update jobs set owner={_ownerId} where owner is null and (time is null or time <= now()) limit 10;
                        select * from jobs where owner={_ownerId};
                        delete from jobs where owner={_ownerId};
                    ");

                    return rows
                        .Select(row => new SingleJob 
                        {
                            Command = (string) row.command, 
                            Tag = (string) row.tag
                        })
                        .ToList();
                }
            }
            catch (Exception e)
            {
                _logger.Error("Cronical.MySql: Error while loading jobs: " + e.Message);
                return null;
            }
        }

        public void Completed(SingleJob job)
        {
        }

        public void Shutdown()
        {
            _logger.Log("Cronical.MySql: Shutting down");
        }
    }
}
