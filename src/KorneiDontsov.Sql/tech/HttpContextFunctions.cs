using Microsoft.AspNetCore.Http;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

static class HttpContextFunctions {
	public static Task WriteTextResponse (this HttpContext context, Int32 statusCode, String text) {
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = MediaTypeNames.Text.Plain;
		return context.Response.WriteAsync(text);
	}
}
