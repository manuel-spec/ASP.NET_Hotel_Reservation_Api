

using MongoDB.Bson.Serialization.Attributes;

namespace TODOAPI.Models;

public class Hotel
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public String? Id { get; set; }

    public required String Name { get; set; }
    public required String Description { get; set; }
}

