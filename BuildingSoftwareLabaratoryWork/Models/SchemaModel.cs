using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BuildingSoftwareLabaratoryWork.Models;

public class SchemaModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    
    public List<SchemaModel>? LeftChildren { get; set; }
    public List<SchemaModel>? RightChildren { get; set; }
    public string Operation { get; set; } = null!;
}