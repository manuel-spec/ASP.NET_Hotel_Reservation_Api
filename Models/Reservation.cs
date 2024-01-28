using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TODOAPI.Models;

public class RoomReservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("Id")]
    public String? Id { get; set; }

    [BsonRequired]
    [BsonElement("GuestName")]
    public required string GuestName { get; set; }

    [BsonRequired]
    [BsonElement("CheckInDate")]
    public DateTime CheckInDate { get; set; }

    [BsonRequired]
    [BsonElement("CheckOutDate")]
    public DateTime CheckOutDate { get; set; }


}

