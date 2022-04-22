using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.WebApiClients;
/*
public class TerminWebApiClientProgressDecorator : ITerminWebApiClient
{
    private readonly ITerminWebApiClient terminWebApiClient;
    private readonly INotifyProgress notifyProgress;

    public TerminWebApiClientProgressDecorator(ITerminWebApiClient terminWebApiClient, INotifyProgress notifyProgress)
    {
        this.terminWebApiClient = terminWebApiClient;
        this.notifyProgress = notifyProgress;
    }

    public async Task<TerminDetailDto> GetDetailTerminuAsync(int terminId)
    {
        return await notifyProgress.Execute(async () => await terminWebApiClient.GetDetailTerminuAsync(terminId));
    }

    public Task<TerminDetailDto> GetDetailTerminuAsync(int terminId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<TerminListDto> GetTerminyAsync()
    {
        return await notifyProgress.Execute(async () => await terminWebApiClient.GetTerminyAsync());
    }

    public Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task OdhlasitAsync(int terminId, int osobaId)
    {
        await notifyProgress.Execute(async () => await terminWebApiClient.OdhlasitAsync(terminId, osobaId));
    }

    public Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task PrihlasitAsync(int terminId, int osobaId)
    {
        await notifyProgress.Execute(async () => await terminWebApiClient.PrihlasitAsync(terminId, osobaId));
    }

    public Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
*/
