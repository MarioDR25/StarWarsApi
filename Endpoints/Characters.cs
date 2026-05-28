using Microsoft.EntityFrameworkCore;
using StarWarsApi.Data;
using StarWarsApi.Helpers;
using StarWarsApi.Models;

namespace StarWarsApi.Endpoints;

public static class Characters
{
    public static RouteGroupBuilder MapCharacterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapPost("/favorite/{characterId:int}", AddFavoriteCharacterAsync);
        group.MapDelete("/favorite/{characterId:int}", DeleteFavoriteCharacterAsync);

        return group;

        static async Task<IResult> GetAllAsync(StarWarsDbContext db) =>
            TypedResults.Ok(await db.Characters
                .Select(c => new CharacterResponseDto(c.Id, c.Name, c.Species, c.Gender))
                .ToListAsync());

        static async Task<IResult> GetByIdAsync(int id, StarWarsDbContext db)
        {
            var character = await db.Characters
                .Where(c => c.Id == id)
                .Select(c => new CharacterResponseDto(c.Id, c.Name, c.Species, c.Gender))
                .FirstOrDefaultAsync();

            return character is not null ? TypedResults.Ok(character) : TypedResults.NotFound();
        }

        static async Task<IResult> CreateAsync(CreateCharacterDto character, StarWarsDbContext db)
        {
            Character newCharacter = new()
            {
                Name = character.Name,
                Species = character.Species,
                Gender = character.Gender
            };

            db.Characters.Add(newCharacter);
            await db.SaveChangesAsync();

            var response = new CharacterResponseDto(newCharacter.Id, newCharacter.Name, newCharacter.Species, newCharacter.Gender);
            return TypedResults.Created($"/api/characters/{newCharacter.Id}", response);
        }

        static async Task<IResult> UpdateAsync(int id, CreateCharacterDto updateData, StarWarsDbContext db)
        {
            var existingItem = await db.Characters.FindAsync(id);
            if (existingItem is null)
                return TypedResults.NotFound(new { message = "Character not found." });

            existingItem.Name = updateData.Name;
            existingItem.Species = updateData.Species;
            existingItem.Gender = updateData.Gender;

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> DeleteAsync(int id, StarWarsDbContext db)
        {
            var character = await db.Characters.FindAsync(id);
            if (character is null)
                return TypedResults.NotFound();

            db.Characters.Remove(character);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> AddFavoriteCharacterAsync(int characterId, StarWarsDbContext db, HttpContext httpContext)
        {
            if (!UserHelper.TryGetUserId(httpContext, out int userId))
                return TypedResults.BadRequest(new { message = "Invalid user." });

            var characterExists = await db.Characters.AnyAsync(c => c.Id == characterId);
            if (!characterExists)
                return TypedResults.NotFound(new { message = "Character does not exist." });

            var alreadyFavorite = await db.FavoriteCharacters.AnyAsync(fc => fc.UserId == userId && fc.CharacterId == characterId);
            if (alreadyFavorite)
                return TypedResults.BadRequest(new { message = "This character is already in your favorites." });

            var favorite = new FavoriteCharacter { UserId = userId, CharacterId = characterId };
            db.FavoriteCharacters.Add(favorite);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new { message = "Character added to favorites." });
        }

        static async Task<IResult> DeleteFavoriteCharacterAsync(int characterId, StarWarsDbContext db, HttpContext httpContext)
        {
            if (!UserHelper.TryGetUserId(httpContext, out int userId))
                return TypedResults.BadRequest(new { message = "Invalid user." });

            var favorite = await db.FavoriteCharacters.FindAsync(userId, characterId);
            if (favorite is null)
                return TypedResults.NotFound(new { message = "Character was not in your favorites." });

            db.FavoriteCharacters.Remove(favorite);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}
