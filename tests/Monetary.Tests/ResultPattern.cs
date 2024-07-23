using FluentAssertions;
using Monetary.Api.Controllers;
using Monetary.Tests.Features.Units;

namespace Monetary.Tests;

public record GamerResponse(int id);

public static class GamerResponseResult
{
    public static Db1Result<GamerResponse> DescriptionExistente
    {
        get
        {
            return Db1Results.Fail<GamerResponse>(MensagensErro.DescriptionExists);
        }
    }
}

public class GamerService
{
    public Db1Result<GamerResponse> Handle(int id)
    {
        if (id == 0)
            return GamerResponseResult.DescriptionExistente;
        else if (id == 1)
            return Db1Results.Ok(new GamerResponse(id));
        else
            return Db1Results.NotFound<GamerResponse>();
    }
}

public class Db1Result<TValue>
{
    private readonly object? _value;
    private readonly ErrorReason? _error;
    private readonly string? _message;

    public Db1Result(TValue value)
    {
        _value = value;
    }

    public Db1Result(ErrorReason error)
    {
        _error = error;
    }

    public Db1Result(string message)
    {
        _message = message;
        _error = ErrorReason.Fail;
    }

    public new TValue? Value => _value is TValue value ? value : default;
    public new ErrorReason? Error => _error is ErrorReason error ? error : default;
    public new string Message => _message is string message ? message : default;
    public bool IsSucesso => _error is null && string.IsNullOrEmpty(_message);

    public static implicit operator Db1Result<TValue>(TValue value)
    {
        return new Db1Result<TValue>(value);
    }
}

public static class Db1Results
{
    public static Db1Result<TValue> Ok<TValue>(TValue value) => new Db1Result<TValue>(value);

    public static Db1Result<TValue> NotFound<TValue>() => new Db1Result<TValue>(ErrorReason.NotFound);

    public static Db1Result<TValue> Fail<TValue>(string message) => new Db1Result<TValue>(message);
}

public enum ErrorReason
{
    NotFound,
    BadRequest,
    DuplicateId,
    Fail,
}

public class GamerServiceUnitTests : UnitTests
{
    private readonly GamerService _sut;

    public GamerServiceUnitTests()
    {
        _sut = new GamerService();
    }

    [Fact]
    public void Dado_Id_Igual_0_Then_Handle_Retorna_Falha()
    {
        // Arrange
        int id = 0;
        // Act
        var response = _sut.Handle(id);
        // Assert
        response.Should().NotBeNull();
        response.IsSucesso.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Value.Should().Be(ErrorReason.Fail);
        response.Message!.Should().Be(MensagensErro.DescriptionExists);
    }

    [Fact]
    public void Dado_Id_Igual_1_Then_Handle_Retorna_Ok()
    {
        // Arrange
        int id = 1;
        // Act
        var response = _sut.Handle(id);
        // Assert
        response.Should().NotBeNull();
        response.IsSucesso.Should().BeTrue();
        response.Value.As<GamerResponse>().Should().NotBeNull();
        response.Value.As<GamerResponse>().id.Should().Be(id);
    }

    [Fact]
    public void Dado_Id_Diferente_0_e_1_Then_Handle_Retorna_NotFound()
    {
        // Arrange
        int id = 2;
        // Act
        var response = _sut.Handle(id);
        // Assert
        response.Should().NotBeNull();
        response.IsSucesso.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Value.Should().Be(ErrorReason.NotFound);
        response.Message.Should().BeNull();
    }
}