using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monetary.Api.Controllers;
using Moq;
using System.Net;

namespace Monetary.Tests.Features.Units;

public class UnitTests
{
    public readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
}

public class WeatherForecastControllerUnitTests : UnitTests
{
    private readonly WeatherCommand _command;
    private readonly Mock<ILogger<WeathersController>> _logger;
    private readonly Mock<IWeatherApplication> _applicationMock;
    private readonly WeathersController _sut;

    public WeatherForecastControllerUnitTests()
    {
        // Parameters
        _command = _fixture.Create<WeatherCommand>();
        // Dependencies
        _logger = _fixture.Freeze<Mock<ILogger<WeathersController>>>();
        _applicationMock = _fixture.Freeze<Mock<IWeatherApplication>>();
        // System Under Tests
        _sut = _fixture.Build<WeathersController>()
            .OmitAutoProperties()
            .Create();
    }

    #region GetAll_Method

    [Fact]
    public async Task Given_GetAll_When_Method_Ended_Then_Returns_Success()
    {
        // Arrange
        WeatherQueryResponse[] responseMock = _fixture.Create<WeatherQueryResponse[]>();
        _applicationMock
            .Setup(x => x.GetAllWeather())
            .ReturnsAsync(responseMock);
        // Act
        var response = await _sut.GetAll();
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.Should().Be(responseMock);
        _applicationMock
            .Verify(x => x.GetAllWeather(), Times.Once());
    }

    [Fact]
    public async Task Given_GetAll_When_Unreturned_Data_Then_Returns_NotFound()
    {
        // Arrange
        WeatherQueryResponse[] responseMock = Array.Empty<WeatherQueryResponse>();
        _applicationMock
            .Setup(x => x.GetAllWeather())
            .ReturnsAsync(responseMock);
        // Act
        var response = await _sut.GetAll();
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.Should().Be(MensagensErro.NotFoundMessage);
        _applicationMock
            .Verify(x => x.GetAllWeather(), Times.Once());
    }

    [Fact]
    public async Task Given_GetAll_When_Application_Throws_Exception_Then_Return_Internal_Server_Error()
    {
        // Arrange
        Exception responseMock = new Exception();
        _applicationMock
            .Setup(x => x.GetAllWeather())
            .ThrowsAsync(responseMock);
        // Act
        var response = await _sut.GetAll();
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.As<ProblemDetails>().Detail.Should().Be(MensagensErro.UnknownError);
        _applicationMock
            .Verify(x => x.GetAllWeather(), Times.Once());
    }

    #endregion GetAll_Method

    #region Get_Method

    [Fact]
    public async Task Given_Get_When_Method_Ended_Then_Returns_Success()
    {
        // Arrange
        WeatherQueryResponse weathers = _fixture.Create<WeatherQueryResponse>();
        _applicationMock
            .Setup(x => x.GetWeather(It.IsAny<Guid>()))
            .ReturnsAsync(weathers);
        // Act
        var response = await _sut.Get(It.IsAny<Guid>());
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.Should().Be(weathers);
        _applicationMock
            .Verify(x => x.GetWeather(It.IsAny<Guid>()), Times.Once());
    }

    [Fact]
    public async Task Given_Get_When_Unreturned_Data_Then_Returns_NotFound()
    {
        // Arrange
        WeatherQueryResponse? responseMock = null;
        _applicationMock
            .Setup(x => x.GetWeather(It.IsAny<Guid>()))
            .ReturnsAsync(responseMock);
        // Act
        var response = await _sut.Get(It.IsAny<Guid>());
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.Should().Be(MensagensErro.NotFoundMessage);
        _applicationMock
            .Verify(x => x.GetWeather(It.IsAny<Guid>()), Times.Once());
    }

    [Fact]
    public async Task Given_Get_When_Application_Throws_Exception_Then_Return_Internal_Server_Error()
    {
        // Arrange
        Exception responseMock = new Exception();
        _applicationMock
            .Setup(x => x.GetWeather(It.IsAny<Guid>()))
            .ThrowsAsync(responseMock);
        // Act
        var response = await _sut.Get(It.IsAny<Guid>());
        // Assert
        response.Should().NotBeNull();
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.As<ProblemDetails>().Detail.Should().Be(MensagensErro.UnknownError);
        _applicationMock
            .Verify(x => x.GetWeather(It.IsAny<Guid>()), Times.Once());
    }

    #endregion Get_Method

    #region Post_Method

    [Fact]
    public async Task Given_Post_When_Method_Ended_Then_Returns_Success()
    {
        WeatherCommandResponse weathers = _fixture.Build<WeatherCommandResponse>()
            .With(x => x.Description, _command.Description)
            .Create();
        _applicationMock
            .Setup(x => x.AddWeather(_command))
            .ReturnsAsync(weathers);
        // Act
        var response = await _sut.Post(_command);
        //Assert
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.As<Result<WeatherCommandResponse>>().IsSuccess.Should().BeTrue();
        response.As<ObjectResult>().Value.As<Result<WeatherCommandResponse>>().Value.Description.Should().Be(_command.Description);
        _applicationMock
            .Verify(x => x.AddWeather(It.IsAny<WeatherCommand>()), Times.Once);
    }

    [Fact]
    public async Task Given_Post_When_Request_Invalid_Then_Returns_BadRequest_With_Errors()
    {
        // Arrange
        var command = _command with { Description = string.Empty };
        // Act
        var response = await _sut.Post(command);
        //Assert
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.Should().Be(MensagensErro.DescriptionRequired);
        _applicationMock
            .Verify(x => x.AddWeather(It.IsAny<WeatherCommand>()), Times.Never);
    }

    [Fact]
    public async Task Given_Post_When_Application_Throws_Exception_Then_Return_Internal_Server_Error()
    {
        // Arrange
        _applicationMock
            .Setup(x => x.AddWeather(It.IsAny<WeatherCommand>()))
            .ThrowsAsync(new Exception());
        // Act
        var response = await _sut.Post(_command);
        //Assert
        response.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        response.As<ObjectResult>().Value.Should().NotBeNull();
        response.As<ObjectResult>().Value.As<ProblemDetails>().Detail.Should().Be(MensagensErro.UnknownError);
        _applicationMock
            .Verify(x => x.AddWeather(It.IsAny<WeatherCommand>()), Times.Once);
    }

    #endregion Post_Method
}