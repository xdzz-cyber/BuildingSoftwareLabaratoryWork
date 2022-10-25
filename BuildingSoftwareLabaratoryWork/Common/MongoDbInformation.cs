using MongoDB.Driver;

namespace BuildingSoftwareLabaratoryWork.Common;

public static class MongoDbInformation
{
    public static MongoClient GetClient()
        => new();
}