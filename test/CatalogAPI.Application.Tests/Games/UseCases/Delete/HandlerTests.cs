using CatalogAPI.Application.Contexts.Games.UseCases.Delete;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Shared;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Games.UseCases.Delete;

public class HandlerTests
{
    private readonly IDelete _repository;
    private readonly Handler _handler;

    public HandlerTests()
    {
        _repository = Substitute.For<IDelete>();
        _handler = new Handler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();

        _repository.DeleteAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var response = await _handler.Handle(new Request(id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(204, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowGameNotFound()
    {
        _repository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // GlobalExceptionHandler mapeia GameNotFoundException -> 404.
        await Assert.ThrowsAsync<GameNotFoundException>(
            () => _handler.Handle(new Request(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnBadRequestWithoutCallingRepository()
    {
        var response = await _handler.Handle(new Request(Guid.Empty), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsDomainException_ShouldPropagate()
    {
        _repository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new DomainException("Domain rule violated")));

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(new Request(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        _repository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new Request(Guid.NewGuid()), CancellationToken.None));
    }
}