namespace StarWarsApi.Models;


public class User 
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FavoriteCharacter> FavoriteCharacters { get; set; } = [];
    public ICollection<FavoritePlanet> FavoritePlanets { get; set; } = [];

}