﻿using System;
using System.Linq;
using Havit.Data.Patterns.DataSeeds;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.VolejbalApp.DataLayer.Seeds.Core;
using Havit.VolejbalApp.Facades.Infrastructure.Security;
using Havit.VolejbalApp.Facades.Infrastructure.Security.Authorization;
using Havit.VolejbalApp.Services.Infrastructure;

namespace Havit.VolejbalApp.Facades.System
{
    /// <summary>
    /// Fasáda k seedování dat.
    /// </summary>
    [Service]
    public class DataSeedFacade : IDataSeedFacade
    {
        private readonly IDataSeedRunner dataSeedRunner;
        private readonly IApplicationAuthorizationService applicationAuthorizationService;

        public DataSeedFacade(IDataSeedRunner dataSeedRunner, IApplicationAuthorizationService applicationAuthorizationService)
        {
            this.dataSeedRunner = dataSeedRunner;
            this.applicationAuthorizationService = applicationAuthorizationService;
        }

        /// <summary>
        /// Provede seedování dat daného profilu.
        /// Pokud jde produkční prostředí a profil není pro produkční prostředí povolen, vrací BadRequest.
        /// </summary>        
        public void SeedDataProfile(string profileName)
        {
			applicationAuthorizationService.VerifyCurrentUserAuthorization(Operations.SystemAdministration);

			string typeName = profileName + "Profile";
			Type type = typeof(CoreProfile).Assembly.GetTypes().FirstOrDefault(item => String.Equals(item.Name, typeName, StringComparison.InvariantCultureIgnoreCase));

            if (type == null)
            {
                throw new OperationFailedException($"Profil {profileName} nebyl nalezen.");
            }

            dataSeedRunner.SeedData(type);
        }
    }
}
