using System;
using System.Collections.Generic;

namespace MarketingBox.AuthApi.Domain.Tokens
{
    public class TenantLocator
    {
        private readonly Dictionary<string, string> _hostTenantIdDict;

        public TenantLocator(Dictionary<string, string> hostTenantIdDict)
        {
            _hostTenantIdDict = hostTenantIdDict;
        }

        public string GetTenantIdByHost(string host)
        {
            string tenantId = null;

            if (_hostTenantIdDict.TryGetValue(host, out tenantId))
            {
                return tenantId;
            }

            if (host == "localhost")
            {
                return "default-tenant-id";
            }

            throw new InvalidOperationException($"There is no registered tenant for {host}");

        }
    }
}