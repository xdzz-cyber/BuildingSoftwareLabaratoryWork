using System.Text;
using BuildingSoftwareLabaratoryWork.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildingSoftwareLabaratoryWork;

public static class Worker
{
    private static Dictionary<string, int> _commands;
    private static MongoClient _mongoClient;

    public static void Init(Dictionary<string, int> commands, MongoClient mongoClient)
    {
        _commands = commands;
        _mongoClient = mongoClient;
    }

    private static IMongoCollection<SchemaModel> GetSchemas()
    {
        var testDb = _mongoClient.GetDatabase("TEST");

        return testDb.GetCollection<SchemaModel>("schemas");
    }

    private static (List<string>, List<string>) ReturnRequestedCommandsIdsIfConditionExist()
    {
        Console.WriteLine("Please, enter commands ids that will be executed in case of true");

        var commandsIdsIfTrue = Console.ReadLine()!.Split(",").ToList();

        Console.WriteLine("Please, enter commands ids that will be executed in case of false");

        var commandsIdsIfFalse = Console.ReadLine()!.Split(",").ToList();

        return (commandsIdsIfFalse, commandsIdsIfTrue);
    }

    private static void InsertNodeChildren(SchemaModel root)
    {
        var commandsIdsIfConditionExists = ReturnRequestedCommandsIdsIfConditionExist();

        foreach (var commandIdIfTrue in commandsIdsIfConditionExists.Item2)
        {
            var newRoot = new SchemaModel
            {
                Id = ObjectId.GenerateNewId(),
                Operation = _commands
                    .FirstOrDefault(c => c.Value
                        .Equals(int.Parse($"{commandIdIfTrue}"))).Key,
                LeftChildren = null,
                RightChildren = null
            };
            
            root.RightChildren = root.RightChildren is null
                ? new List<SchemaModel>
                {
                    newRoot
                } : root.RightChildren.Concat(new []{newRoot}).ToList();
            
            if (commandIdIfTrue.Equals("2") || commandIdIfTrue.Equals("3"))
            {
                InsertNodeChildren(newRoot);
            }
        }

        foreach (var commandIdIfFalse in commandsIdsIfConditionExists.Item1)
        {
            var newRoot = new SchemaModel
            {
                Id = ObjectId.GenerateNewId(),
                Operation = _commands
                    .FirstOrDefault(c => c.Value
                        .Equals(int.Parse($"{commandIdIfFalse}"))).Key,
                LeftChildren = null,
                RightChildren = null
            };
                
            root.LeftChildren = root.LeftChildren is null
                ? new List<SchemaModel>
                {
                    newRoot
                } : root.LeftChildren.Concat(new []{newRoot}).ToList();
            
            if (commandIdIfFalse.Equals("2") || commandIdIfFalse.Equals("3"))
            {
                InsertNodeChildren(newRoot);
            }
        }
    }

    public static async Task CreateSchema()
    {
        var schemasCollectionObject = GetSchemas();
        
        Console.WriteLine("Write up ids of commands with comma as separator");

        var chosenCommandsIds = Console.ReadLine()!.Split(",");

        var newSchemasObject = new List<SchemaModel>();

        for (var i = 0; i < chosenCommandsIds.Length; i++)
        {
            var matchedCommand = _commands
                .FirstOrDefault(c => c.Value.Equals(int.Parse($"{chosenCommandsIds[i]}")));

            var newSchemaObject = new SchemaModel
            {
                Id = ObjectId.GenerateNewId(),
                Operation = matchedCommand.Key,
                LeftChildren = null,
                RightChildren = null
            };

            if (chosenCommandsIds[i].Equals("2") || chosenCommandsIds[i].Equals("3"))
            {
                InsertNodeChildren(newSchemaObject);
            }
            
            newSchemasObject.Add(newSchemaObject);
        }

        await schemasCollectionObject.InsertManyAsync(newSchemasObject);


        Console.WriteLine("All went good");
    }

    public static void ShowSchemas()
    {
        var schemasCollectionObject = GetSchemas();

        var schemas = schemasCollectionObject.AsQueryable().ToList();

        var response = new StringBuilder();

        foreach (var schema in schemas)
        {
            response.Append($"\n{schema.Id}-{schema.Operation}");
            
            if (schema.LeftChildren is not null || schema.RightChildren is not null)
            {
                GetAllChildrenInfo(schemas, response);
            }
        }

        var tmp = response.ToString();
        
        Console.WriteLine(tmp);
    }

    private static void GetAllChildrenInfo(List<SchemaModel> root, StringBuilder allLines)
    {
        foreach (var rootChild in root)
        {
            if (rootChild.LeftChildren is not null)
            {
                allLines.Append("\tLeftChildren:");
                    
                foreach (var leftChild in rootChild.LeftChildren)
                {
                    allLines.Append($"{leftChild.Id}-{leftChild.Operation}-->");    
                }

                GetAllChildrenInfo(rootChild.LeftChildren, allLines);
            }

            if (rootChild.RightChildren is null) continue;

            allLines.Append("\tRightChildren:");
                
            foreach (var rightChild in rootChild.RightChildren)
            {
                allLines.Append($"{rightChild.Id}-{rightChild.Operation}-->");    
            }

            GetAllChildrenInfo(rootChild.RightChildren, allLines);
        }
            
    }
}