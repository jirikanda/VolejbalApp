using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Entity
{
    // inspirace: https://mderriey.com/2020/07/17/connect-to-azure-sql-with-aad-and-managed-identities/

    /// <summary>
    /// Podpora Managed Identity pro EF Core.
    /// </summary>
    public class AadAuthenticationDbConnectionInterceptor : DbConnectionInterceptor
    {
        private bool? useAadAuthentication = null;

        public override async Task<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken)
        {
            if (connection is SqlConnection sqlConnection)
            {
                if (useAadAuthentication == null)
                {
                    lock (typeof(AadAuthenticationDbConnectionInterceptor))
                    {
                        if (useAadAuthentication == null)
                        {
                            var connectionStringBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
                            useAadAuthentication = connectionStringBuilder.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(connectionStringBuilder.UserID);
                        }
                    }
                }

                if (useAadAuthentication.Value)
                {
                    sqlConnection.AccessToken = await GetAzureSqlAccessToken(cancellationToken);
                }
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }

        private static async Task<string> GetAzureSqlAccessToken(CancellationToken cancellationToken)
        {
            // See https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities#azure-sql
            var tokenRequestContext = new TokenRequestContext(new[] { "https://database.windows.net//.default" });
            var tokenRequestResult = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext, cancellationToken);

            return tokenRequestResult.Token;
        }
    }
}
