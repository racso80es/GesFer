using FluentAssertions;
using GesFer.Application.Commands.PurchaseDeliveryNote;
using GesFer.Application.Handlers.PurchaseDeliveryNote;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Alias to resolve ambiguity
using PurchaseDeliveryNoteEntity = GesFer.Product.Back.Domain.Entities.PurchaseDeliveryNote;

namespace GesFer.Product.UnitTests.Handlers.PurchaseDeliveryNote;

public class ConfirmPurchaseDeliveryNoteCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly ConfirmPurchaseDeliveryNoteCommandHandler _handler;

    public ConfirmPurchaseDeliveryNoteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new ConfirmPurchaseDeliveryNoteCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ConfirmsDeliveryNote()
    {
        // Arrange
        var deliveryNoteId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        // Seed DeliveryNote
        _context.PurchaseDeliveryNotes.Add(new PurchaseDeliveryNoteEntity
        {
            Id = deliveryNoteId,
            CompanyId = companyId,
            Date = DateTime.UtcNow,
            Reference = "REF-001",
            BillingStatus = BillingStatus.Pending,
            DeletedAt = null
        });
        await _context.SaveChangesAsync();

        var command = new ConfirmPurchaseDeliveryNoteCommand { DeliveryNoteId = deliveryNoteId };

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deliveryNote = await _context.PurchaseDeliveryNotes.FindAsync(deliveryNoteId);
        deliveryNote.Should().NotBeNull();
        deliveryNote!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var command = new ConfirmPurchaseDeliveryNoteCommand { DeliveryNoteId = Guid.NewGuid() };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El albarán con ID * no existe*");
    }
}
