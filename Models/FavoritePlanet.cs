namespace StarWarsApi.Models;

public class FavoritePlanet
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int PlanetId { get; set; }
    public Planet? Planet { get; set; }

}