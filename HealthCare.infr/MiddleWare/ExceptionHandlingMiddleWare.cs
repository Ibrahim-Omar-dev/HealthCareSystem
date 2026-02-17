using HealthCare.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HealthCare.Infreastructure.MiddleWare
{
    public class ExceptionHandlingMiddleWare
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DbUpdateException ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var logger=context.RequestServices.GetRequiredService<IAppLogger>();

                string message = "Database error occurred.";

                if (ex.InnerException is SqlException innerException)
                {
                    logger.LogError("Sql Exception ", innerException);
                    switch (innerException.Number)
                    {
                        case 2627: // Unique constraint
                            message = "This record already exists.";
                            break;

                        case 515: // Cannot insert NULL
                            message = "A required field is missing.";
                            break;

                        case 547: // Foreign key constraint
                            message = "Cannot delete or update due to related data.";
                            break;

                        default:
                            message = "Unexpected database error.";
                            break;
                    }
                }
                else
                {
                    logger.LogError("EF related Error .......");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    message = "An error occure While Saving the entity change";

                }
                var response = new
                {
                    error = message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
