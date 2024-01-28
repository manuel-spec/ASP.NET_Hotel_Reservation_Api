using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace TodoApi.Models;

public class User
{
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("Id")]
    public string? Id { get; set; }

    [BsonElement("Email")]
    public required string Email { get; set; }

    [BsonElement("Password")]
    public required string Password { get; set; }

    [BsonElement("Name")]
    public string? Name { get; set; }

    [BsonElement("IsEmailVerified")]
    public bool IsEmailVerified { get; set; }

    [BsonElement("EmailVerificationToken")]
    public string? EmailVerificationToken { get; set; }
   



    
    
}
