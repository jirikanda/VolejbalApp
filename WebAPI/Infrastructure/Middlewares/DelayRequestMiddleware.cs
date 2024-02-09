namespace KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;

public class DelayRequestMiddleware
{
	private readonly RequestDelegate _next;

	public DelayRequestMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		await Task.Delay(500);
		await _next(context);
	}

}
