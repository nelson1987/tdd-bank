using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Linq.Expressions;

namespace Monetary.Tests.Features;

public class Game
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public string? UserEmail { get; set; }
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? Changes { get; set; }
}

[Trait("Category", "UnitTests")]
public class GetGameHandlerUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly GameApplication _sut;
    private readonly Game _game;
    private readonly Mock<IGameService> _repositorio;
    private readonly CancellationToken _token = CancellationToken.None;

    private readonly Guid _contractNumber = Guid.NewGuid();

    public GetGameHandlerUnitTests()
    {
        _repositorio = _fixture.Freeze<Mock<IGameService>>();

        _game = _fixture.Build<Game>()
                .With(x => x.Id, _contractNumber)
                .Create();

        _fixture.Freeze<Mock<IGameService>>()
                .Setup(x => x.GetById(_contractNumber, _token))
                .ReturnsAsync(_game);

        _sut = _fixture.Create<GameApplication>();
    }

    [Fact]
    public async Task ValidarId()
    {
        // Act
        var handler = await _sut.Handle(_contractNumber, _token);
        // Assert
        handler.IsSuccess.Should().BeTrue();
        handler.Value.Id.Should().Be(_contractNumber);
        handler.Value.Description.Should().Be(_game.Description);
    }

    [Fact]
    public async Task TesteStringVazio()
    {
        // Arrange
        _fixture.Freeze<Mock<IGameService>>()
        .Setup(x => x.GetById(_contractNumber, _token))
                .ReturnsAsync(Result.Fail("Game não encontrado."));
        // Act
        var repository = await _sut.Handle(_contractNumber, _token);
        // Assert
        repository.IsFailed.Should().BeTrue();
    }
}

public interface IGameApplication
{
    Task<Result<Game>> Handle(Guid id, CancellationToken cancellation);
}

public class GameApplication : IGameApplication
{
    private readonly IGameService _gameService;

    public GameApplication(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task<Result<Game>> Handle(Guid id, CancellationToken cancellation)
    {
        return await _gameService.GetById(id, cancellation);
    }
}

[Trait("Category", "UnitTests")]
public class GameServiceUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly GameService _sut;
    private readonly Game _game;
    private readonly Mock<IGameRepository> _repositorio;
    private readonly Guid _contractNumber = Guid.NewGuid();
    private readonly CancellationToken _token = CancellationToken.None;

    public GameServiceUnitTests()
    {
        _repositorio = _fixture.Freeze<Mock<IGameRepository>>();

        _game = _fixture.Build<Game>()
                .With(x => x.Id, _contractNumber)
                .Create();

        _fixture.Freeze<Mock<IGameRepository>>()
                .Setup(x => x.GetById(_contractNumber, _token))
                .ReturnsAsync(_game);

        _sut = _fixture.Create<GameService>();
    }

    [Fact]
    public async Task ValidarId()
    {
        // Act
        var repository = await _sut.GetById(_contractNumber, _token);
        // Assert
        repository.IsSuccess.Should().BeTrue();
        repository.Value.Id.Should().Be(_contractNumber);
        repository.Value.Description.Should().Be(_game.Description);
    }

    [Fact]
    public async Task TesteStringVazio()
    {
        // Arrange
        _fixture.Freeze<Mock<IGameRepository>>()
                .Setup(x => x.GetById(_contractNumber, _token))
                .ReturnsAsync((Game?)null);
        // Act
        var repository = await _sut.GetById(_contractNumber, _token);
        // Assert
        repository.IsFailed.Should().BeTrue();
    }
}

public interface IGameService
{
    Task<Result<Game>> GetById(Guid id, CancellationToken cancellation);
}

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Result<Game>> GetById(Guid id, CancellationToken cancellation)
    {
        var game = await _gameRepository.GetById(id, cancellation);
        if (game is null)
            return Result.Fail<Game>("Game não encontrado.");
        return Result.Ok(game);
    }
}

[Trait("Category", "UnitTests")]
public class GameRepositoryUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private readonly GameRepository _sut;
    private readonly CancellationToken _token = CancellationToken.None;

    public GameRepositoryUnitTests()
    {
        _sut = _fixture.Create<GameRepository>();
    }

    [Fact]
    public async Task TesteStringVazio()
    {
        // Arrange
        var contractNumber = Guid.NewGuid();
        // Act
        var repository = await _sut.GetById(contractNumber, _token);
        // Assert
        repository!.Id.Should().Be(contractNumber);
    }
}

public class GameContext : DbContext
{
    public GameContext(DbContextOptions<GameContext> options)
      : base(options)
    { }

    public DbSet<Game> Games { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var modifiedEntries = ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added
            or EntityState.Modified
            or EntityState.Deleted)
            .ToList();
        foreach (var modifiedEntry in modifiedEntries)
        {
            var auditLog = new AuditLog
            {
                EntityName = modifiedEntry.Entity.GetType().Name,
                UserEmail = "robo",
                Action = modifiedEntry.State.ToString(),
                TimeStamp = DateTime.UtcNow,
                Changes = "GetChanges(modifiedEntry)",
            };
            AuditLogs.Add(auditLog);
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

public class ApiFixture : WebApplicationFactory<Program>
{
    static ApiFixture()
        => Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Testing")
                  .ConfigureTestServices(services =>
                  {
                      //DataContext
                      services.AddDbContext<GameContext>(x =>
                      {
                          x.UseInMemoryDatabase(databaseName: "InMemoryDatabase");
                      });
                  });
}

public class IntegrationTests : IAsyncLifetime
{
    public readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    public readonly CancellationToken _token = CancellationToken.None;
    public readonly HttpClient _client;
    public readonly ApiFixture _server;
    public readonly IGameRepository _gameRepository;

    public IntegrationTests()
    {
        _server = new ApiFixture();
        _client = _server.CreateClient();
        _gameRepository = _server.Services.GetRequiredService<IGameRepository>();
    }

    public async Task InitializeAsync()
    {
        //await todoRepository.DeleteAll(token);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        //await todoRepository.DeleteAll(token);
        await Task.CompletedTask;
    }
}

public class GameRepositoryIntegrationTests //: IntegrationTests
{
    public readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Game _entity;
    private readonly CancellationToken _token = CancellationToken.None;

    public GameRepositoryIntegrationTests() : base()
    {
        _entity = _fixture.Build<Game>()
            .Without(x => x.Id)
            .Create();
    }

    [Fact]
    public async Task Given_Account_Correto_Retorna_Ok_Context()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GameContext>(x =>
        {
            x.UseInMemoryDatabase(databaseName: "InMemoryDatabase");
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<GameContext>();
        // Arrange Act
        await dbContext!.Games.AddAsync(_entity, _token);
        await dbContext!.SaveChangesAsync();
        // Assert
        var auditLog = await dbContext!.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityName.Should().Be(typeof(Game).Name);
        auditLog!.UserEmail.Should().Be("robo");
        auditLog!.Action.Should().Be(nameof(EntityState.Added));
        auditLog!.TimeStamp.ToString("yyyy-MM-dd").Should().Be(DateTime.UtcNow.ToString("yyyy-MM-dd"));
        auditLog!.Changes.Should().Be("GetChanges(modifiedEntry)");
    }

    [Fact]
    public async Task Given_Account_Correto_Retorna_Ok_()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GameContext>(x =>
        {
            x.UseInMemoryDatabase(databaseName: "InMemoryDatabase");
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<GameContext>();
        // Arrange
        await dbContext.Games.AddAsync(_entity, _token);
        await dbContext.SaveChangesAsync();
        GameRepository gameRepository = new GameRepository(dbContext!);
        // Act
        var gameInserido = await gameRepository.GetById(_entity.Id, _token);
        // Assert
        gameInserido.Should().NotBeNull();
        gameInserido!.Id.Should().Be(_entity.Id);
        gameInserido!.Description.Should().Be(_entity.Description);
        var auditLog = await dbContext!.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityName.Should().Be(typeof(Game).Name);
        auditLog!.UserEmail.Should().Be("robo");
        auditLog!.Action.Should().Be(nameof(EntityState.Added));
        auditLog!.TimeStamp.ToString("yyyy-MM-dd").Should().Be(DateTime.UtcNow.ToString("yyyy-MM-dd"));
        auditLog!.Changes.Should().Be("GetChanges(modifiedEntry)");
    }

    [Fact]
    public async Task Given_Account_Correto_Retorna_Ok()
    {
        var services = new ServiceCollection();
        services.AddDbContext<GameContext>(x =>
        {
            x.UseInMemoryDatabase(databaseName: "InMemoryDatabase");
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<GameContext>();
        // Arrange
        GameRepository gameRepository = new GameRepository(dbContext!);
        // Act
        var gameInserido = await gameRepository.GetById(Guid.NewGuid(), _token);
        // Assert
        gameInserido.Should().NotBeNull();
        gameInserido!.Id.Should().Be(_entity.Id);
        gameInserido!.Description.Should().Be(_entity.Description);
        var auditLog = await dbContext!.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityName.Should().Be(typeof(Game).Name);
        auditLog!.UserEmail.Should().Be("robo");
        auditLog!.Action.Should().Be(nameof(EntityState.Added));
        auditLog!.TimeStamp.ToString("yyyy-MM-dd").Should().Be(DateTime.UtcNow.ToString("yyyy-MM-dd"));
        auditLog!.Changes.Should().Be("GetChanges(modifiedEntry)");
    }
}

public interface IGameRepository
{
    Task<Game?> GetById(Guid id, CancellationToken cancellation);
}

public class GameRepository : IGameRepository
{
    private readonly GameContext _gameContext;

    public GameRepository(GameContext gameContext)
    {
        _gameContext = gameContext;
    }

    public async Task<Game?> GetById(Guid id, CancellationToken cancellation)
    {
        return await _gameContext.Games.FirstOrDefaultAsync(x => x.Id == id);
        //return await Task.FromResult(new Game { Id = id, Description = "Teste" });
    }
}

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Followers { get; set; }
}

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    public DbSet<Developer> Developers { get; set; }
    public DbSet<Project> Projects { get; set; }
}

public interface IGenericRepository<T> where T : class
{
    T GetById(int id);

    IEnumerable<T> GetAll();

    IEnumerable<T> Find(Expression<Func<T, bool>> expression);

    void Add(T entity);

    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);
}

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationContext _context;

    public GenericRepository(ApplicationContext context)
    {
        _context = context;
    }

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _context.Set<T>().AddRange(entities);
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression);
    }

    public IEnumerable<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    public T GetById(int id)
    {
        return _context.Set<T>().Find(id);
    }

    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }
}

public interface IDeveloperRepository : IGenericRepository<Developer>
{
    IEnumerable<Developer> GetPopularDevelopers(int count);
}

public class DeveloperRepository : GenericRepository<Developer>, IDeveloperRepository
{
    public DeveloperRepository(ApplicationContext context) : base(context)
    {
    }

    public IEnumerable<Developer> GetPopularDevelopers(int count)
    {
        return _context.Developers.OrderByDescending(d => d.Followers).Take(count).ToList();
    }
}

public interface IProjectRepository : IGenericRepository<Project>
{
}

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationContext context) : base(context)
    {
    }
}

public interface IUnitOfWork : IDisposable
{
    IDeveloperRepository Developers { get; }
    IProjectRepository Projects { get; }

    int Complete();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationContext _context;

    public UnitOfWork(ApplicationContext context)
    {
        _context = context;
        Developers = new DeveloperRepository(_context);
        Projects = new ProjectRepository(_context);
    }

    public IDeveloperRepository Developers { get; private set; }
    public IProjectRepository Projects { get; private set; }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public class DeveloperHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public DeveloperHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Developer> GetPopularDevelopers([FromQuery] int count)
    {
        var popularDevelopers = _unitOfWork.Developers.GetPopularDevelopers(count);
        return popularDevelopers;
    }

    public void AddDeveloperAndProject()
    {
        var developer = new Developer
        {
            Followers = 35,
            Name = "Mukesh Murugan"
        };
        var project = new Project
        {
            Name = "codewithmukesh"
        };
        _unitOfWork.Developers.Add(developer);
        _unitOfWork.Projects.Add(project);
        _unitOfWork.Complete();
    }
}