using DocumentQnA.Application.Common.Interfaces;
using DocumentQnA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentQnA.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DocumentRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Documents
            .Include(d => d.Chunks)
            .Include(d => d.QnAHistories)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Document>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Documents.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Documents.Update(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var document = await context.Documents.FindAsync([id], cancellationToken);
        if (document is not null)
        {
            context.Documents.Remove(document);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}