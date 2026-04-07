using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Infrastructure.AI;
using DocumentQnA.Infrastructure.Parsing;
using DocumentQnA.Infrastructure.Persistence;
using DocumentQnA.Infrastructure.Persistence.Repositories;
using DocumentQnA.Infrastructure.Processing;
using DocumentQnA.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentQnA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // File Storage
        var storagePath = configuration["Storage:LocalPath"] ?? "uploads";
        services.AddSingleton<IFileStorageService>(_ => new LocalFileStorageService(storagePath));

        // Parsing
        services.AddSingleton<TextChunkingService>();
        services.AddSingleton<PdfTextExtractor>();
        services.AddSingleton<DocxTextExtractor>();
        services.AddSingleton<TxtTextExtractor>();

        // Ollama - Embedding
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
        });

        // Ollama - LLM
        services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
        });

        // Qdrant
        services.AddHttpClient<IVectorStoreService, QdrantVectorStoreService>(client =>
        {
            client.BaseAddress = new Uri(configuration["Qdrant:BaseUrl"] ?? "http://localhost:6333");
        });

        // Processing
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

        return services;
    }
}