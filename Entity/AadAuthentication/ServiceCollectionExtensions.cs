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

namespace KandaEu.Volejbal.Entity.AadAuthentication
{
    public static class ServiceCollectionExtensions
	{
        public static DbContextOptionsBuilder UseSqlServerAadAuthentication(this DbContextOptionsBuilder optionsBuilder)
		{
            return optionsBuilder.AddInterceptors(new AadAuthenticationDbConnectionInterceptor());
		}
	}
}
