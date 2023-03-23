namespace App.Contracts;

public class WeatherDto
{
  public DateTime Date { get; init; }
  public string Weather { get; init; }
  public int Temperature { get; init; }
}