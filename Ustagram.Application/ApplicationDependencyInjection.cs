using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Ustagram.Application.Abstractions;
using Ustagram.Application.Services;

namespace Ustagram.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection service)
    {
        service.AddScoped<IUserService, UserService>();
        service.AddScoped<IPostService, PostService>();
        service.AddScoped<ICommentService, CommentService>();
        service.AddScoped<IFavouritesService, FavouritesService>();
        service.AddScoped<IFileService, FileService>();
        service.AddScoped<INotificationService, NotificationService>();
        service.AddSingleton<JwtService>();
        service.AddSingleton<CommentHub>();
        service.AddLogging();

        return service;
    }
}