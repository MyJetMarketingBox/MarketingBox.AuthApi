using System;
using System.Collections.Generic;
using Autofac;
using MarketingBox.Auth.Service.Client;
using MarketingBox.Auth.Service.MyNoSql.Users;
using MarketingBox.AuthApi.Domain.Tokens;
using MyJetWallet.Sdk.NoSql;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;

namespace MarketingBox.AuthApi.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var dict = new Dictionary<string, string>();
            foreach (var keyValue in Program.Settings.Tenants.TenantHostMapping)
            {
                foreach (var value in keyValue.Value.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var tenantName = keyValue.Key.ToLower();
                    dict[value] = tenantName;
                    Console.WriteLine($"Tenant name: '{tenantName}' host: '{value}' ");
                }
            }

            var tenantLocator = new TenantLocator(dict);

            builder
                .RegisterInstance(tenantLocator)
                .As<TenantLocator>();

            builder.RegisterAuthServiceClient(Program.Settings.AuthServiceUrl);
            var noSqlClient = builder.CreateNoSqlClient(
                Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort).Invoke(),
                Program.LogFactory);

            var subs = new MyNoSqlReadRepository<UserNoSql>(noSqlClient, UserNoSql.TableName);
            builder.RegisterInstance(subs)
                .As<IMyNoSqlServerDataReader<UserNoSql>>();
        }
    }
}