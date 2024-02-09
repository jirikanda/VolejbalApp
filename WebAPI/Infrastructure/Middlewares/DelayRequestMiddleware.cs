namespace KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;

public class DelayRequestMiddleware(RequestDelegate _next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		await Task.Delay(500);
		await _next(context);
	}

}
