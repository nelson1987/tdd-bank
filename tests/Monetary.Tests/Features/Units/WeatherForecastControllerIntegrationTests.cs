namespace Monetary.Tests.Features.Units;

public class WeatherForecastControllerIntegrationTests
{
    [Fact]
    public async Task Given_GetAll_When_User_Unathorized_Then_Returns_Unauthorized()
    {
        // Act
        // var response = _sut.GetAll();
        // Assert
        //var result = response as ObjectResult;
        //result.Should().NotBeNull();
        //result!.StatusCode.Should().Be((int)HttpStatusCode.Created);

        //_logger // .Verify(LogMessage(LogLevel.Information, "Started"), // Times.Once);

        //_fixture.Freeze<Mock<IAccountRepository>>() // .Verify(x => x.Insert(It.IsAny<Account>())
        // , Times.Once);

        //_logger // .Verify(LogMessage(LogLevel.Information, "Ended"), // Times.Once);
        Assert.Fail("Criar em Integration Tests");
    }

    [Fact]
    public async Task Given_Get_When_User_Unathorized_Then_Returns_Unauthorized()
    {
        Assert.Fail("Criar em Integration Tests");
    }

    [Fact]
    public async Task Given_Post_When_User_Unathorized_Then_Returns_Unauthorized()
    {
        Assert.Fail("Criar em Integration Tests");
    }
}
