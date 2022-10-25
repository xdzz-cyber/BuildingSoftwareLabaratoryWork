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

    public static async Task CreateSchema()
    {
        var schemasCollectionObject = GetSchemas();
        //
        // var schemasCollectionList = 
        //     await schemasCollectionObject.Find(new BsonDocument()).ToListAsync();

        // ----------
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
                while (true)
                {
                    var commandsIdsIfConditionExists = ReturnRequestedCommandsIdsIfConditionExist();

                    foreach (var commandIdIfTrue in commandsIdsIfConditionExists.Item2) // 1,2,-3,4-5,6; 1,2,3,2,1
                    {
                        if (commandsIdsIfConditionExists.Item2.Any(c => c.Equals("2") || c.Equals("3")))
                        {
                            newSchemaObject.RightChildren = newSchemaObject.RightChildren is null
                                ? new List<SchemaModel>
                                {
                                    new()
                                    {
                                        Id = ObjectId.GenerateNewId(),
                                        Operation = _commands
                                            .FirstOrDefault(c => c.Value
                                                .Equals(int.Parse($"{commandIdIfTrue}"))).Key,
                                        LeftChildren = null,
                                        RightChildren = null
                                    }
                                } : newSchemaObject.RightChildren.Concat(new []{new SchemaModel
                                {
                                    Id = ObjectId.GenerateNewId(),
                                    Operation = _commands
                                        .FirstOrDefault(c => c.Value
                                            .Equals(int.Parse($"{commandIdIfTrue}"))).Key,
                                    LeftChildren = null,
                                    RightChildren = null   
                                }}).ToList();
                        }
                        else
                        {
                            
                        }  
                    }

                    foreach (var commandIdIfFalse in commandsIdsIfConditionExists.Item1)
                    {
                        if (commandsIdsIfConditionExists.Item2.Any(c => c.Equals("2") || c.Equals("3")))
                        {
                            newSchemaObject.LeftChildren = newSchemaObject.LeftChildren is null
                                ? new List<SchemaModel>
                                {
                                    new()
                                    {
                                        Id = ObjectId.GenerateNewId(),
                                        Operation = _commands
                                            .FirstOrDefault(c => c.Value
                                                .Equals(int.Parse($"{commandIdIfFalse}"))).Key,
                                        LeftChildren = null,
                                        RightChildren = null
                                    }
                                } : newSchemaObject.LeftChildren.Concat(new []{new SchemaModel
                                {
                                    Id = ObjectId.GenerateNewId(),
                                    Operation = _commands
                                        .FirstOrDefault(c => c.Value
                                            .Equals(int.Parse($"{commandIdIfFalse}"))).Key,
                                    LeftChildren = null,
                                    RightChildren = null   
                                }}).ToList();
                        }
                        else
                        {
                            
                        }  
                    }

                    if (commandsIdsIfConditionExists.Item2.Any(x => x.Equals("2") || x.Equals("3"))
                        || commandsIdsIfConditionExists.Item1.Any(y => y.Equals("2") || y.Equals("3")))
                        break;
                }
            }

            newSchemasObject.Add(newSchemaObject);
        }

        await schemasCollectionObject.InsertManyAsync(newSchemasObject);

        // await schemasCollectionObject.InsertManyAsync(chosenCommandsIds!.Split(",")
        //     .Select(x => new SchemaModel
        //     {
        //         Id = ObjectId.GenerateNewId(),
        //         Operation = _commands
        //             .FirstOrDefault(c => c.Value.Equals(int.Parse($"{x}"))).Key,
        //         LeftChild = null,
        //         RightChild = null
        //     }).ToList());


        // var commandsIdsIfTrue = string.Empty;
        //
        // var commandsIdsIfFalse = string.Empty;
        //
        // foreach (var chosenCommandId in chosenCommandsIds!.Split(","))
        // {
        //     if (!chosenCommandId.Equals("2") && !chosenCommandId.Equals("3")) continue;
        //
        //     Console.WriteLine("Please, enter commands ids that will be executed in case of true");
        //
        //     commandsIdsIfTrue = Console.ReadLine();
        //
        //     Console.WriteLine("Please, enter commands ids that will be executed in case of false");
        //
        //     commandsIdsIfFalse = Console.ReadLine();
        // }


        //var schemaToBeInserted = new List<string?>();

        // var baseSchemaId = Guid.NewGuid();
        //
        // foreach (var chosenCommandId in chosenCommandsIds.Split(","))
        // {
        //     var matchedCommand = _commands
        //         .FirstOrDefault(c => c.Value.Equals(int.Parse($"{chosenCommandId}")));
        //
        //     var flag = chosenCommandId.Equals("2") || chosenCommandId.Equals("3");
        //
        //     var tmpObj = new SchemaCommandViewModel
        //     {
        //         Id = matchedCommand.Value.ToString(),
        //         Name = matchedCommand.Key,
        //         TrueCaseScenario = flag
        //             ? string.Join("-", commandsIdsIfTrue!.Split(",")
        //                 .Select(x => _commands
        //                     .FirstOrDefault(c => c.Value.Equals(int.Parse(x)))))
        //             : "",
        //         FalseCaseScenario = flag
        //             ? string.Join("-", commandsIdsIfFalse!.Split(",")
        //                 .Select(x => _commands
        //                     .FirstOrDefault(c => c.Value.Equals(int.Parse(x)))))
        //             : "",
        //         BaseSchemaId = baseSchemaId
        //     };
        //
        //     schemaToBeInserted.Add(JsonConvert.SerializeObject(tmpObj));
        // }


        // await File.AppendAllTextAsync(filename ?? throw new InvalidOperationException(),
        //     string.Join("--", schemaToBeInserted) + "\n");


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