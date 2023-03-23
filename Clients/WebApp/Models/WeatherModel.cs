using App.Contracts;

namespace App.Models;

#nullable disable
public class WeatherModel
{
  public IEnumerable<WeatherDto> WeatherDtos { get; set; }
}
