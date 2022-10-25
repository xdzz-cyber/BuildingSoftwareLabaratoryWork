using MongoDB.Driver;

namespace BuildingSoftwareLabaratoryWork.Common;

public static class MongoDbInformation
{
    public static MongoClient GetClient()
        => new("mongodb+srv://BHbrk:12345@cluster0.gtmed.azure.mongodb.net/?retryWrites=true&w=majority");
}