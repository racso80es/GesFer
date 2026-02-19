using FluentAssertions;
using GesFer.Application.Commands.SalesDeliveryNote;
using GesFer.Application.Handlers.SalesDeliveryNote;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Alias to resolve ambiguity
using SalesDeliveryNoteEntity = GesFer.Product.Back.Domain.Entities.SalesDeliveryNote;

namespace GesFer.Product.UnitTests.Handlers.SalesDeliveryNote;

public class ConfirmSalesDeliveryNoteCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly ConfirmSalesDeliveryNoteCommandHandler _handler;

    public ConfirmSalesDeliveryNoteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new ConfirmSalesDeliveryNoteCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ConfirmsDeliveryNote()
    {
        // Arrange
        var deliveryNoteId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        // Seed DeliveryNote
        _context.SalesDeliveryNotes.Add(new SalesDeliveryNoteEntity
        {
            Id = deliveryNoteId,
            CompanyId = companyId,
            Date = DateTime.UtcNow,
            Reference = "REF-001",
            BillingStatus = BillingStatus.Pending,
            DeletedAt = null
        });
        await _context.SaveChangesAsync();

        var command = new ConfirmSalesDeliveryNoteCommand { DeliveryNoteId = deliveryNoteId };

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deliveryNote = await _context.SalesDeliveryNotes.FindAsync(deliveryNoteId);
        deliveryNote.Should().NotBeNull();
        deliveryNote!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var command = new ConfirmSalesDeliveryNoteCommand { DeliveryNoteId = Guid.NewGuid() };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El albarán con ID * no existe*");
    }
}
