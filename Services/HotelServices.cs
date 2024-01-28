using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Org.BouncyCastle.Tls;
using TodoApi.DatabaseSettings;
using TodoApi.Models;
using TODOAPI.Models;


namespace TODOAPI.Services;

public class ReservationServices
{
    private readonly IConfiguration _configuration;
    private readonly IMongoCollection<Hotel> _hotelCollections;
    private readonly IMongoCollection<RoomReservation> _reservationCollections;



    public ReservationServices(IOptions<ReservationDBSettings> hotelDBSettings, IConfiguration configuration)
    {
        _configuration = configuration;
        MongoClient client = new MongoClient(hotelDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(hotelDBSettings.Value.DatabaseName);
        _hotelCollections = database.GetCollection<Hotel>(hotelDBSettings.Value.HotelCollectionName);
        _reservationCollections = database.GetCollection<RoomReservation>(hotelDBSettings.Value.ReservationCollectionName);


    }

    public async Task<List<RoomReservation>> GetAllAsync()
    {
        return await _reservationCollections.Find(r => true).ToListAsync();
    }

    public async Task<RoomReservation> GetAsync(String id)
    {

        return await _reservationCollections.Find(r => r.Id == id).FirstOrDefaultAsync() ?? throw new InvalidDataException($"The reservation couldn't not be found");


    }
    public async Task<RoomReservation> GetByNameAsync(String guestname)
    {

        return await _reservationCollections.Find(r => r.GuestName == guestname).FirstOrDefaultAsync() ?? throw new InvalidDataException($"The reservation couldn't not be found");

    }


    public async Task CreateAsync(RoomReservation reservation)
    {

        await _reservationCollections.InsertOneAsync(reservation);
        return;

    }
    public async Task UpdateAsync(String id, RoomReservation reservation)
    {
        var Updated = await _reservationCollections.Find(r => r.Id == id).FirstOrDefaultAsync() ?? throw new InvalidDataException($"The reservation couldn't not be found");
        Updated.GuestName = reservation.GuestName;
        Updated.CheckInDate = reservation.CheckInDate;
        Updated.CheckOutDate = reservation.CheckOutDate;

        await _reservationCollections.ReplaceOneAsync(r => r.Id == id, Updated);
        return;
    }

    public async Task CancelReservationAsync(String id)
    {

        var removedreservation = await _reservationCollections.Find(r => r.Id == id).FirstOrDefaultAsync();

        if (removedreservation != null)
        {
            await _reservationCollections.DeleteOneAsync(x => x.Id == id);
            return;
        }
        else
        {

            throw new InvalidDataException($"the reservation couldnt be found");
        }


    }


    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["JwtSettings:Key"];

        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new ApplicationException("JWT Key is missing or empty in configuration.");
        }

        var tokenKey = Encoding.ASCII.GetBytes(jwtKey);

        try
        {
            // Decode the JWT token without validation
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false, // Disable signature validation
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            }, out _);

            // Optionally, you can access the decoded claims here
            // var emailClaim = claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value;

            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }



}


