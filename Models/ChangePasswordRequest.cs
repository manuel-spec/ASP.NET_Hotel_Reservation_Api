using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TodoApi.Models;
public class ChangePasswordRequest
{
    [BsonElement("UserId")]
    public required string UserEmail { get; set; }
    [BsonElement("CurrentPassword")]
    public required string CurrentPassword { get; set; }
    [BsonElement("NewPassword")]
    public required string NewPassword { get; set; }
}