using Microsoft.EntityFrameworkCore;
using StarWarsApi.Data;
using StarWarsApi.Helpers;
using StarWarsApi.Models;

namespace StarWarsApi.Endpoints;

public static class Planets
{
    public static RouteGroupBuilder MapPlanetEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapPost("/favorite/{planetId:int}", AddFavoritePlanetAsync);
        group.MapDelete("/favorite/{planetId:int}", DeleteFavoritePlanetAsync);

        return group;

        static async Task<IResult> GetAllAsync(StarWarsDbContext db) =>
            TypedResults.Ok(await db.Planets
                .Select(p => new PlanetResponseDto(p.Id, p.Name, p.Climate, p.Terrain))
                .ToListAsync());

        static async Task<IResult> GetByIdAsync(int id, StarWarsDbContext db)
        {
            var item = await db.Planets
                .Where(p => p.Id == id)
                .Select(p => new PlanetResponseDto(p.Id, p.Name, p.Climate, p.Terrain))
                .FirstOrDefaultAsync();

            return item is not null ? TypedResults.Ok(item) : TypedResults.NotFound();
        }

        static async Task<IResult> CreateAsync(CreatePlanetDto planet, StarWarsDbContext db)
        {
            Planet newPlanet = new()
            {
                Name = planet.Name,
                Climate = planet.Climate,
                Terrain = planet.Terrain
            };

            db.Planets.Add(newPlanet);
            await db.SaveChangesAsync();

            var response = new PlanetResponseDto(newPlanet.Id, newPlanet.Name, newPlanet.Climate, newPlanet.Terrain);
            return TypedResults.Created($"/api/planets/{newPlanet.Id}", response);
        }

        static async Task<IResult> UpdateAsync(int id, CreatePlanetDto updateData, StarWarsDbContext db)
        {
            var existingItem = await db.Planets.FindAsync(id);
            if (existingItem is null)
                return TypedResults.NotFound(new { message = "Planet not found." });

            existingItem.Name = updateData.Name;
            existingItem.Climate = updateData.Climate;
            existingItem.Terrain = updateData.Terrain;

            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> DeleteAsync(int id, StarWarsDbContext db)
        {
            var item = await db.Planets.FindAsync(id);
            if (item is null)
                return TypedResults.NotFound();

            db.Planets.Remove(item);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        static async Task<IResult> AddFavoritePlanetAsync(int planetId, StarWarsDbContext db, HttpContext httpContext)
        {
            if (!UserHelper.TryGetUserId(httpContext, out int userId))
                return TypedResults.BadRequest(new { message = "Invalid user." });

            var planetExists = await db.Planets.AnyAsync(p => p.Id == planetId);
            if (!planetExists)
                return TypedResults.NotFound(new { message = "Planet does not exist." });

            var alreadyFavorite = await db.FavoritePlanets.AnyAsync(fp => fp.UserId == userId && fp.PlanetId == planetId);
            if (alreadyFavorite)
                return TypedResults.BadRequest(new { message = "This planet is already in your favorites." });

            var favorite = new FavoritePlanet { UserId = userId, PlanetId = planetId };
            db.FavoritePlanets.Add(favorite);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new { message = "Planet added to favorites." });
        }

        static async Task<IResult> DeleteFavoritePlanetAsync(int planetId, StarWarsDbContext db, HttpContext httpContext)
        {
            if (!UserHelper.TryGetUserId(httpContext, out int userId))
                return TypedResults.BadRequest(new { message = "Invalid user." });

            var favorite = await db.FavoritePlanets.FindAsync(userId, planetId);
            if (favorite is null)
                return TypedResults.NotFound(new { message = "Planet was not in your favorites." });

            db.FavoritePlanets.Remove(favorite);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}
