using MongoDB.Driver;

namespace ERDMCore.Infrastructure.MongoDB.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "credit_management";
        public string CollectionPrefix { get; set; } = string.Empty;

        // Connection Pool Settings
        public int MinPoolSize { get; set; } = 10;
        public int MaxPoolSize { get; set; } = 100;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int SocketTimeoutSeconds { get; set; } = 30;

        // Write Concern
        public string WriteConcern { get; set; } = "majority";
        public bool JournalEnabled { get; set; } = true;
        public int? WriteConcernWValue { get; set; } = null;

        // Read Preference
       public string ReadPreferenceModel { get; set; } = "Primary";
        public List<TagSet> TagSets { get; set; } = new List<TagSet>();
        public TimeSpan? MaxStaleness { get; set; } = null;

        // Retry Settings
        public bool RetryWrites { get; set; } = true;
        public bool RetryReads { get; set; } = true;

        // SSL Settings
        public bool UseSsl { get; set; } = false;
        public bool AllowInsecureTls { get; set; } = false;

        public string GetFullCollectionName(string collectionName)
        {
            return string.IsNullOrEmpty(CollectionPrefix)
                ? collectionName
                : $"{CollectionPrefix}_{collectionName}";
        }

        public WriteConcern GetWriteConcern()
        {
            var concern = WriteConcern?.ToLowerInvariant();

            switch (concern)
            {
                case "majority":
                    return new WriteConcern("majority", journal: JournalEnabled);

                case "acknowledged":
                    return new WriteConcern(1, journal: JournalEnabled);

                case "unacknowledged":
                    return new WriteConcern(0, journal: JournalEnabled);

                case "custom":
                    var wValue = WriteConcernWValue ?? 1;
                    return new WriteConcern(wValue, journal: JournalEnabled);

                default:
                    if (int.TryParse(WriteConcern, out int wInt))
                    {
                        return new WriteConcern(wInt, journal: JournalEnabled);
                    }
                    return new WriteConcern("majority", journal: JournalEnabled);
            }
        }

        public ReadPreference GetReadPreference()
        {
            var mode = ReadPreferenceModel?.ToLowerInvariant();

            ReadPreference readPreference;

            switch (mode)
            {
                case "primary":
                    readPreference = ReadPreference.Primary;
                    break;

                case "primarypreferred":
                    readPreference = ReadPreference.PrimaryPreferred;
                    break;

                case "secondary":
                    readPreference = ReadPreference.Secondary;
                    break;

                case "secondarypreferred":
                    readPreference = ReadPreference.SecondaryPreferred;
                    break;

                case "nearest":
                    readPreference = ReadPreference.Nearest;
                    break;

                default:
                    readPreference = ReadPreference.Primary;
                    break;
            }

            // Apply TagSets if configured and mode is not Primary
            // Use the fully qualified enum name to avoid confusion with the string property
            if (TagSets != null && TagSets.Any() &&
                readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary)
            {
                readPreference = readPreference.With(TagSets);
            }

            // Apply MaxStaleness if configured and mode is not Primary
            if (MaxStaleness.HasValue &&
                readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary)
            {
                readPreference = readPreference.With(MaxStaleness);
            }

            return readPreference;
        }

        public MongoClientSettings GetClientSettings()
        {
            var settings = MongoClientSettings.FromConnectionString(ConnectionString);

            // Connection Pool Settings
            settings.MinConnectionPoolSize = MinPoolSize;
            settings.MaxConnectionPoolSize = MaxPoolSize;
            settings.ConnectTimeout = TimeSpan.FromSeconds(ConnectionTimeoutSeconds);
            settings.SocketTimeout = TimeSpan.FromSeconds(SocketTimeoutSeconds);

            // Retry Settings
            settings.RetryWrites = RetryWrites;
            settings.RetryReads = RetryReads;

            // Write Concern & Read Preference
            settings.WriteConcern = GetWriteConcern();
            settings.ReadPreference = GetReadPreference();

            // SSL Settings
            if (UseSsl)
            {
                settings.UseSsl = true;
                settings.AllowInsecureTls = AllowInsecureTls;
            }

            // Server Selection Timeout
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);

            // Heartbeat Interval
            settings.HeartbeatInterval = TimeSpan.FromSeconds(10);

            // Max Connection Idle Time
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);

            // Max Connection Life Time
            settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(30);

            return settings;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ConnectionString) &&
                   !string.IsNullOrWhiteSpace(DatabaseName);
        }

        public string GetMaskedConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return "Not configured";

            // Mask password in connection string if present
            var masked = System.Text.RegularExpressions.Regex.Replace(
                ConnectionString,
                @"password=([^;]+)",
                "password=***");

            return $"{masked}, Database={DatabaseName}";
        }
    }
}