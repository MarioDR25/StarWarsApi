using Microsoft.EntityFrameworkCore;
using StarWarsApi.Data;
using StarWarsApi.Helpers;
using StarWarsApi.Models;

namespace StarWarsApi.Endpoints;

public static class Users
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapGet("/favorites", GetUserFavoritesAsync);

        return group;

        static async Task<IResult> GetAllAsync(StarWarsDbContext db) =>
            TypedResults.Ok(await db.Users
                .Select(u => new UserResponseDto(u.Id, u.Username, u.Email, u.CreatedAt))
                .ToListAsync());

        static async Task<IResult> GetByIdAsync(int id, StarWarsDbContext db)
        {
            var item = await db.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto(u.Id, u.Username, u.Email, u.CreatedAt))
                .FirstOrDefaultAsync();

            return item is not null ? TypedResults.Ok(item) : TypedResults.NotFound();
        }

        static async Task<IResult> CreateAsync(CreateUserDto user, StarWarsDbContext db)
        {
            User newUser = new()
            {
                Username = user.Username,
                Email = user.Email,
                Password = PasswordHasher.Hash(user.Password)
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            var response = new UserResponseDto(newUser.Id, newUser.Username, newUser.Email, newUser.CreatedAt);
            return TypedResults.Created($"/api/users/{newUser.Id}", response);
        }

        static async Task<IResult> UpdateAsync(int id, CreateUserDto updateData, StarWarsDbContext db)
        {
            var existingItem = await db.Users.FindAsync(id);
            if (existingItem is null)
                return TypedResults.NotFound(new { message = "User not found." });

            existingItem.Username = updateData.Username;
            existingItem.Email = updateData.Email;
            existingItem.Password = PasswordHasher.Hash(updateData.Password);

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> DeleteAsync(int id, StarWarsDbContext db)
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return TypedResults.NotFound();

            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> GetUserFavoritesAsync(StarWarsDbContext db, HttpContext httpContext)
        {
            if (!UserHelper.TryGetUserId(httpContext, out int userId))
                return TypedResults.BadRequest(new { message = "Missing X-User-Id header with a valid ID." });

            var userFavorites = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    UserId = u.Id,
                    u.Username,
                    FavoriteCharacters = u.FavoriteCharacters.Select(fc => new CharacterResponseDto(fc.Character!.Id, fc.Character.Name, fc.Character.Species, fc.Character.Gender)),
                    FavoritePlanets = u.FavoritePlanets.Select(fp => new PlanetResponseDto(fp.Planet!.Id, fp.Planet.Name, fp.Planet.Climate, fp.Planet.Terrain))
                })
                .FirstOrDefaultAsync();

            if (userFavorites is null)
                return TypedResults.NotFound(new { message = "User not found." });

            return TypedResults.Ok(userFavorites);
        }
    }
}
