﻿using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.System
{
    public interface IDataSeedFacade
    {
        Task SeedDataProfile(string profileName);
    }
}