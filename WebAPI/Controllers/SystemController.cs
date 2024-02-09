﻿using KandaEu.Volejbal.Contracts.System;
using Microsoft.AspNetCore.Mvc;

namespace Havit.VolejbalApp.WebAPI.Controllers;

/// <summary>
/// Controller pro systémové akce.
/// </summary>
public class SystemController : ControllerBase
{
	private readonly IDataSeedFacade dataSeedFacade;

	public SystemController(IDataSeedFacade dataSeedFacade)
	{
		this.dataSeedFacade = dataSeedFacade;
	}

	/// <summary>
	/// Provede seedování dat zadaného profilu.
	/// Název profilu nemá obsahovat "Profile", vyhledává se dle názvu typu bez ohledu na velikost písmen.
	/// </summary>
	/// <param name="profile">Název profilu k seedování.</param>
	/// <param name="cancellationToken">Stopping token.</param>
	[HttpPost("api/system/seed/{profile}")]
	public async Task SeedDataAsync(string profile, CancellationToken cancellationToken)
	{
		await dataSeedFacade.SeedDataProfileAsync(profile, cancellationToken);
	}
}
