using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Monetary.Api.Controllers;

public static class MensagensErro
{
    public static string UnknownError = "Try again later";
    public static string NotFoundMessage = "Not found";
    public static string DescriptionRequired = "Description is required";
    public static string DescriptionExists = "Já existe uma previsão com essa descrição.";
}

[ApiController]
[Route("[controller]")]
public class WeathersController : ControllerBase
{
    private readonly ILogger<WeathersController> _logger;
    private readonly IWeatherApplication _application;

    public WeathersController(ILogger<WeathersController> logger, IWeatherApplication application)
    {
        _logger = logger;
        _application = application;
    }

    [HttpGet]
    [ProducesResponseType<WeatherQueryResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var inteiro = await _application.GetAllWeather();
            if (Equals(inteiro, Array.Empty<WeatherQueryResponse>())) return NotFound(MensagensErro.NotFoundMessage);
            return Ok(inteiro);
        }
        catch (Exception)
        {
            return Problem(MensagensErro.UnknownError);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType<WeatherQueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var inteiro = await _application.GetWeather(id);
            if (inteiro is null) return NotFound(MensagensErro.NotFoundMessage);
            return Ok(inteiro);
        }
        catch (Exception)
        {
            return Problem(MensagensErro.UnknownError);
        }
    }

    [HttpPost]
    [ProducesResponseType<WeatherCommandResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post([FromBody] WeatherCommand command)
    {
        try
        {
            if (string.IsNullOrEmpty(command.Description))
                return BadRequest(MensagensErro.DescriptionRequired);

            var addWeather = await _application.AddWeather(command);
            if (addWeather.IsFailed)
                return BadRequest(addWeather.Errors);

            return Created("[controller]", addWeather);
        }
        catch (Exception)
        {
            return Problem(MensagensErro.UnknownError);
        }
    }
}

public record WeatherCommand(string Description);
public record WeatherCommandResponse(Guid Id, string Description);
public record WeatherQueryResponse(Guid Id, string Description);

public interface IWeatherApplication
{
    Task<WeatherQueryResponse[]> GetAllWeather();

    Task<WeatherQueryResponse?> GetWeather(Guid Id);

    Task<Result<WeatherCommandResponse>> AddWeather(WeatherCommand entity);
}

public static class WeatherApplicationResult
{
    public static Result<WeatherCommandResponse> DescriptionExistente
    {
        get
        {
            return Result.Fail<WeatherCommandResponse>(MensagensErro.DescriptionExists);
        }
    }
}

public class WeatherApplication : IWeatherApplication
{
    private readonly IWeatherRepository _weatherRepository;

    public WeatherApplication(IWeatherRepository weatherRepository)
    {
        _weatherRepository = weatherRepository;
    }

    public async Task<Result<WeatherCommandResponse>> AddWeather(WeatherCommand entity)
    {
        try
        {
            if (await _weatherRepository.ExistsAsync(entity.Description) != null)
                return WeatherApplicationResult.DescriptionExistente;

            var weather = Weather.Create(entity.Description);
            var weatherSalvo = await _weatherRepository.AddAsync(weather);
            //await _unitOfWork.SaveChangeAsync();

            var response = new WeatherCommandResponse(weatherSalvo.Id, weatherSalvo.Description!);
            return Result.Ok(response);
        }
        catch (Exception)
        {
            throw;
            //return Result.Fail<WeatherCommandResponse>(MensagensErro.DescriptionExists);
        }
    }

    public Task<WeatherQueryResponse[]> GetAllWeather()
    {
        throw new NotImplementedException();
    }

    public async Task<WeatherQueryResponse?> GetWeather(Guid Id)
    {
        var response = new WeatherQueryResponse(Id, "Description");
        return await Task.FromResult(response);
    }
}

public interface IWeatherRepository
{
    Task<Weather?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Weather?> ExistsAsync(string description, CancellationToken cancellationToken = default);

    Task<Weather> AddAsync(Weather weather, CancellationToken cancellationToken = default);
}

public class WeatherRepository : IWeatherRepository
{
    public Task<Weather> AddAsync(Weather weather, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Weather?> ExistsAsync(string description, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Weather?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

//public Weather GetAllWeather()
//{
//    return new Weather();
//}

//public Weather GetWeather(Guid Id)
//{
//    return new Weather();
//}

//public void AddWeather(string description)
//{
//    var entity = WeatherBuilder.Create(description);
//}

//public void UpdateWeather()
//{
//    var entity = WeatherBuilder.Create("Description");
//    entity.Update("New description");
//}

public class Weather
{
    public Guid Id { get; set; } //= Guid.NewGuid();

    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } //= DateTime.Now;

    public DateTime ChangedAt { get; set; } //= DateTime.Now;

    public static Weather Create(string description)
    {
        return new Weather { Description = description };
    }

    //public void Create(string description)
    //{
    //    SetDescription(description);
    //}
    //
    //public void Update(string description)
    //{
    //    SetDescription(description);
    //    ChangedAt = DateTime.Now;
    //}
    //
    //private void SetDescription(string description)
    //{
    //    if (string.IsNullOrEmpty(description)) { throw new Exception(); }
    //    Description = description;
    //}
}

//
//public static class WeatherBuilder
//{
//    public static Weather Create(string description)
//    {
//        Weather weather = new Weather();
//        weather.Create(description);
//        return weather;
//    }
//}