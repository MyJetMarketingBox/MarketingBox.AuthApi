using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace MarketingBox.AuthApi.Settings
{
    public class SettingsModel
    {
        [YamlProperty("MarketingBoxAuthApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("MarketingBoxAuthApi.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("MarketingBoxAuthApi.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("MarketingBoxAuthApi.AuthServiceUrl")]
        public string AuthServiceUrl { get; set; }

        [YamlProperty("MarketingBoxAuthApi.JwtAudience")]
        public string JwtAudience { get; set; }

        [YamlProperty("MarketingBoxAuthApi.JwtSecret")]
        public string JwtSecret { get; set; }

        [YamlProperty("MarketingBoxAuthApi.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("MarketingBoxAuthApi.Tenants")]
        public TenantSettings Tenants { get; set; }

    }
}
