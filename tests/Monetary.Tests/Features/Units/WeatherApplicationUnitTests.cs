using AutoFixture;
using FluentAssertions;
using FluentResults;
using Monetary.Api.Controllers;
using Moq;

namespace Monetary.Tests.Features.Units;

public class WeatherApplicationUnitTests : UnitTests
{
    private readonly WeatherCommand _command;
    private readonly Mock<IWeatherRepository> _repositoryMock;
    private readonly WeatherApplication _sut;

    public WeatherApplicationUnitTests()
    {
        // Parameters
        _command = _fixture.Create<WeatherCommand>();
        // Dependencies
        _repositoryMock = _fixture.Freeze<Mock<IWeatherRepository>>();
        // System Under Tests
        _sut = _fixture.Build<WeatherApplication>()
            .OmitAutoProperties()
            .Create();
    }

    [Fact]
    public async Task Given_AddWeather_When_Method_Ended_Then_Returns_Success()
    {
        // Arrange
        Weather? responseGetMock = null;
        _repositoryMock
            .Setup(x => x.ExistsAsync(_command.Description, CancellationToken.None))
            .ReturnsAsync(responseGetMock);
        Weather responseAddMock = _fixture.Build<Weather>()
            .With(x => x.Description, _command.Description)
            .Create();
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Weather>(), CancellationToken.None))
            .ReturnsAsync(responseAddMock);
        // Act
        var response = await _sut.AddWeather(_command);
        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.IsFailed.Should().BeFalse();
        response.Value.Id.Should().NotBeEmpty();
        response.Value.Description.Should().Be(_command.Description);
        _repositoryMock
            .Verify(x => x.ExistsAsync(_command.Description, CancellationToken.None), Times.Once());
        _repositoryMock
            .Verify(x => x.AddAsync(It.IsAny<Weather>(), CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task Given_AddWeather_When_Entity_Exists_Then_Returns_Fail()
    {
        // Arrange
        Weather? responseMock = _fixture.Build<Weather?>()
            .With(x => x.Description, _command.Description)
            .Create();
        _repositoryMock
            .Setup(x => x.ExistsAsync(_command.Description, CancellationToken.None))
            .ReturnsAsync(responseMock);
        // Act
        var response = await _sut.AddWeather(_command);
        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.IsFailed.Should().BeTrue();
        response.Reasons.Should().Contain(x => x.Message == MensagensErro.DescriptionExists);
        _repositoryMock
            .Verify(x => x.ExistsAsync(_command.Description, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task Given_AddWeather_When_Repository_Throws_Exception_Then_Returns_Fail()
    {
        // Arrange
        Exception responseMock = new Exception("A");
        _repositoryMock
            .Setup(x => x.ExistsAsync(_command.Description, CancellationToken.None))
            .ThrowsAsync(responseMock);
        // Act
        Func<Task<Result<WeatherCommandResponse>>> response = async () => await _sut.AddWeather(_command);
        // Assert
        await response.Should().ThrowAsync<Exception>().WithMessage("A");
        _repositoryMock
            .Verify(x => x.ExistsAsync(_command.Description, CancellationToken.None), Times.Once());
    }
}