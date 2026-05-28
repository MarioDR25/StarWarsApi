using System.ComponentModel.DataAnnotations;

namespace StarWarsApi;

public record CreateCharacterDto(
    [property: Required, StringLength(150)] string Name,
    [property: StringLength(100)] string Species,
    [property: StringLength(50)] string Gender
);

public record CharacterResponseDto(
    int Id,
    string Name,
    string Species,
    string Gender
);

public record CreatePlanetDto(
    [property: Required, StringLength(150)] string Name,
    [property: StringLength(100)] string Climate,
    [property: StringLength(100)] string Terrain
);

public record PlanetResponseDto(
    int Id,
    string Name,
    string Climate,
    string Terrain
);

public record CreateUserDto(
    [property: Required, StringLength(200)] string Username,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: Required, StringLength(200, MinimumLength = 6)] string Password
);

public record UserResponseDto(
    int Id,
    string Username,
    string Email,
    DateTime CreatedAt
);
