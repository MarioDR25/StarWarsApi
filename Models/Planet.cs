namespace StarWarsApi.Models;

public class Planet 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Climate { get; set; } = string.Empty;
    public string Terrain { get; set; } = string.Empty;
}