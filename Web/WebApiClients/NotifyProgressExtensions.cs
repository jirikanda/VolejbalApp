using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.WebApiClients;
/*
public static class NotifyProgressExtensions
{
    public static async Task Execute(this INotifyProgress notifyProgress, Func<Task> action)
    {
        await Execute<object>(notifyProgress, async() =>
        {
            await action();
            return null;
        });
    }

    public static async Task<TResult> Execute<TResult>(this INotifyProgress notifyProgress, Func<Task<TResult>> action)
    {
        notifyProgress.SetProgress();
        try
        {
            return await action();
        }
        finally
        {
            notifyProgress.ClearProgress();
        }
    }
}
*/
