using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// Systémové akce.
/// </summary>
public class SystemFunctions(IDataSeedApi _dataSeedFacade)
{
	/// <summary>
	/// Provede seedování dat zadaného profilu.
	/// Název profilu nemá obsahovat Profile, vyhledává se dle názvu typu bez ohledu na velikost písmen.
	/// </summary>
	[Function(nameof(SeedDataAsync))]
	public async Task<IActionResult> SeedDataAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.SystemSeed)] HttpRequest request,
		string profileName,
		CancellationToken cancellationToken)
	{
		await _dataSeedFacade.SeedDataProfileAsync(profileName, cancellationToken);
		return new NoContentResult();
	}
}
