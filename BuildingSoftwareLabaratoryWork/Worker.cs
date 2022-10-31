using System.Collections.Concurrent;
using System.Text;
using BuildingSoftwareLabaratoryWork.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildingSoftwareLabaratoryWork;

public static class Worker
{
    private static Dictionary<string, Action> _commands;
    private static MongoClient _mongoClient;
    private static ConcurrentDictionary<string, int> _state;

    public static void Init(Dictionary<string, Action> commands, ConcurrentDictionary<string, int> state,MongoClient mongoClient)
    {
        _commands = commands;
        _state = state;
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
                RightChildren = null,
                Next = null
            };

            if (chosenCommandsIds[i].Equals("2") || chosenCommandsIds[i].Equals("3"))
            {
                InsertNodeChildren(newSchemaObject);
            }
            
            if (i > 0)
            {
                newSchemasObject[i - 1].Next = newSchemaObject;
            }
            
            newSchemasObject.Add(newSchemaObject);
        }

        await schemasCollectionObject.InsertOneAsync(newSchemasObject.First());


        Console.WriteLine("All went good");
    }

    public static async Task ModifySchema()
    {
        var schemasCollectionObject = GetSchemas();
        
        Console.WriteLine("Write up id of the schema");

        var schemaId = Console.ReadLine()!;

        var foundSchema = schemasCollectionObject.AsQueryable()
            .ToList().FirstOrDefault(schema => schemaId == schema.Id.ToString());

        if (foundSchema is null)
        {
            Console.WriteLine("Bad schema id given");
            return;
        }
        
        Console.WriteLine("Please, entry new command id");

        var newCommandId = Console.ReadLine();
        
        var filter = Builders<SchemaModel>.Filter.Eq("Id", foundSchema.Id);

        var matchedCommandOperationName =
            _commands.FirstOrDefault(c => c.Value.Equals(int.Parse($"{newCommandId}"))).Key;

        foundSchema.LeftChildren = foundSchema.RightChildren = null;

        foundSchema.Operation = matchedCommandOperationName;
        
        if (newCommandId!.Equals("2") || newCommandId.Equals("3"))
        {
            InsertNodeChildren(foundSchema);
        }

        await schemasCollectionObject.ReplaceOneAsync(filter, foundSchema);

        Console.WriteLine("All went good while updating");
    }

    public static Task ExecuteSchemasByIds()
    {
        Console.WriteLine("Please, enter schemas ids to execute them with comma as separator");

        var schemasIds = Console.ReadLine()!.Split(",");
        
        var schemasCollection = GetSchemas()
            .AsQueryable().ToList();

        foreach (var schema in schemasCollection.Where(schema => schemasIds.Contains(schema.Id.ToString())))
        {
            WaitCallback commonDelegate = _ => Operations.GetOperationByName(schema.Operation); // just for initialization purposes
            
            var schemaCopy = schema;
            
            while (schemaCopy.Next is not null)
            {
                commonDelegate += _ => Operations.GetOperationByName(schemaCopy.Operation);

                if (schemaCopy.Operation.Equals("CompareLess") || schemaCopy.Operation.Equals("CompareEqual"))
                {
                 
                    Console.WriteLine("Please, enter value name to compare with comma as separator");

                    var variableName = Console.ReadLine();
                
                    Console.WriteLine("Please, enter constant name to compare with comma as separator");

                    var constantName = Console.ReadLine();

                    var result = schema.Operation.Equals("CompareLess")
                        ? Operations.Compare(variableName!, constantName!) == -1
                        : Operations.Compare(variableName!, constantName!) == 0;

                    var commandsToBeExecutedBasedOnResult = result 
                        ? schemaCopy.RightChildren : schemaCopy.LeftChildren;

                    commonDelegate = commandsToBeExecutedBasedOnResult!
                        .Aggregate(commonDelegate, (current, commandToBeExecutedBasedOnResult) 
                            => current + (_ => Operations.GetOperationByName(commandToBeExecutedBasedOnResult.Operation)));   
                }
                schemaCopy = schemaCopy.Next;
            }
            ThreadPool.QueueUserWorkItem(commonDelegate);   
        }

        return Task.CompletedTask; //.WaitAsync(new CancellationToken());
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
                GetAllChildrenInfo(schema, response); //schemas
            }

            while (schema.Next is not null)
            {
                response.Append($"--> Next = {schema.Id}-{schema.Next.Operation}");
                
                GetAllChildrenInfo(schema.Next, response);

                schema.Next = schema.Next.Next;
            }
        }

        var tmp = response.ToString();
        
        Console.WriteLine(tmp);
    }

    private static void GetAllChildrenInfo(SchemaModel root, StringBuilder allLines)
    {
        if (root.LeftChildren is not null)
        {
            allLines.Append("\tLeftChildren:");
                    
            foreach (var leftChild in root.LeftChildren)
            {
                allLines.Append($"{leftChild.Id}-{leftChild.Operation}-->");    
            }

            GetAllChildrenInfo(root.LeftChildren.First(), allLines);
        }

        if (root.RightChildren is not null)
        {
            allLines.Append("\tRightChildren:");
                
            foreach (var rightChild in root.RightChildren)
            {
                allLines.Append($"{rightChild.Id}-{rightChild.Operation}-->");    
            }

            GetAllChildrenInfo(root.RightChildren.First(), allLines);
        }

    }
}