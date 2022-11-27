using System.Collections.Concurrent;
using System.Text;
using BuildingSoftwareLabaratoryWork.Common;
using BuildingSoftwareLabaratoryWork.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace BuildingSoftwareLabaratoryWork;

public static class Worker
{
    private static Dictionary<string, Action<string?>?> _commands;
    private static MongoClient _mongoClient;
    private static ConcurrentDictionary<string, int> _state;
    private static ConcurrentDictionary<string, string> _testValuesForSpecificSchema;
    private static ConcurrentDictionary<string, List<string>> _schemasOperationsForTesting;
    private static readonly object ConsoleWriterLock = new();

    public static void Init(Dictionary<string, Action<string?>?> commands, ConcurrentDictionary<string, int> state,
        MongoClient mongoClient)
    {
        _commands = commands;
        _state = state;
        _mongoClient = mongoClient;
        _testValuesForSpecificSchema = new ConcurrentDictionary<string, string>();
        _schemasOperationsForTesting = new ConcurrentDictionary<string, List<string>>();
    }

    private static IMongoCollection<SchemaModel> GetSchemas()
    {
        var testDb = _mongoClient.GetDatabase("TEST");

        return testDb.GetCollection<SchemaModel>("schemas");
    }

    private static (List<string>, List<string>) ReturnRequestedCommandsIfConditionExist()
    {
        Console.WriteLine("Please, enter commands that will be executed in case of true");

        var commandsIfTrue = Console.ReadLine()!.Split(",").ToList();

        Console.WriteLine("Please, enter commands that will be executed in case of false");

        var commandsIfFalse = Console.ReadLine()!.Split(",").ToList();

        return (commandsIfFalse, commandsIfTrue);
    }

    private static void InsertNodeChildren(SchemaModel root)
    {
        var commandsIfConditionExists = ReturnRequestedCommandsIfConditionExist();

        foreach (var commandIfTrue in commandsIfConditionExists.Item2)
        {
            var newObjectId = ObjectId.GenerateNewId();

            var newRoot = new SchemaModel
            {
                Id = newObjectId,
                Operation = _commands
                    .FirstOrDefault(c => c.Key.Equals(commandIfTrue)).Key,
                LeftChildren = null,
                RightChildren = null,
            };

            root.RightChildren = root.RightChildren is null
                ? new List<SchemaModel>
                {
                    newRoot
                }
                : root.RightChildren.Concat(new[] {newRoot}).ToList();

            if (commandIfTrue.Equals("CompareLess") || commandIfTrue.Equals("CompareEqual"))
                InsertNodeChildren(newRoot);
        }

        foreach (var commandIfFalse in commandsIfConditionExists.Item1)
        {
            var newObjectId = ObjectId.GenerateNewId();

            var newRoot = new SchemaModel
            {
                Id = newObjectId,
                Operation = _commands
                    .FirstOrDefault(c => c.Key.Equals(commandIfFalse)).Key,
                LeftChildren = null,
                RightChildren = null
            };

            root.LeftChildren = root.LeftChildren is null
                ? new List<SchemaModel>
                {
                    newRoot
                }
                : root.LeftChildren.Concat(new[] {newRoot}).ToList();

            if (commandIfFalse.Equals("CompareLess") || commandIfFalse.Equals("CompareEqual"))
                InsertNodeChildren(newRoot);
        }
    }

    public static async Task CreateSchema()
    {
        var schemasCollectionObject = GetSchemas();

        Console.WriteLine("Write up commands with comma as separator");

        var chosenCommands = Console.ReadLine()!.Split(",");

        var newSchemasObject = new List<SchemaModel>();

        for (var i = 0; i < chosenCommands.Length; i++)
        {
            var matchedCommand = _commands
                .FirstOrDefault(c => c.Key.Equals(chosenCommands[i]));

            var newObjectId = ObjectId.GenerateNewId();

            var newSchemaObject = new SchemaModel
            {
                Id = newObjectId,
                Operation = matchedCommand.Key,
                LeftChildren = null,
                RightChildren = null,
                Next = null
            };

            if (chosenCommands[i].Equals("CompareLess") || chosenCommands[i].Equals("CompareEqual"))
                InsertNodeChildren(newSchemaObject);

            if (i > 0) newSchemasObject[i - 1].Next = newSchemaObject;

            newSchemasObject.Add(newSchemaObject);
        }

        await schemasCollectionObject.InsertOneAsync(newSchemasObject.First());


        Console.WriteLine("All went good");
    }

    private static (SchemaModel? parent, SchemaModel? child) FindSchemaAndItsParent(string schemaId, 
        List<SchemaModel> whereToFind, SchemaModel? parent = null)
    {

        foreach (var schema in whereToFind)
        {
            var baseParent = schema;
            
            if (schema.Id.ToString() == schemaId)
            {
                return (parent: parent, child: schema);
            }

            if (schema.LeftChildren is not null)
            {
                FindSchemaAndItsParent(schemaId, schema.LeftChildren, baseParent);
            }

            if (schema.RightChildren is not null)
            {
                FindSchemaAndItsParent(schemaId, schema.RightChildren, baseParent);
            }

            var schemaNext = schema.Next;

            while (schemaNext is not null)
            {
                if (schemaNext.Id.ToString() == schemaId)
                {
                    return (parent: baseParent, child: schemaNext);
                }

                if (schemaNext.LeftChildren is not null)
                {
                    var _ = FindSchemaAndItsParent(schemaId, schemaNext.LeftChildren, baseParent);

                    if (_.parent is not null || _.child is not null)
                    {
                        return _;
                    }
                }

                if (schemaNext.RightChildren is not null)
                {
                    var _ = FindSchemaAndItsParent(schemaId, schemaNext.RightChildren, baseParent);
                    
                    if (_.parent is not null || _.child is not null)
                    {
                        return _;
                    }
                }
                
                schemaNext = schemaNext.Next;
            }
        }

        return (parent:null, child:null);
    }

    public static async Task ModifySchema()
    {
        var schemasCollectionObject = GetSchemas();

        Console.WriteLine("Write up id of the schema");

        var schemaId = Console.ReadLine()!;

        var foundSchema = FindSchemaAndItsParent(schemaId, schemasCollectionObject.AsQueryable().ToList());
            //schemasCollectionObject.AsQueryable().ToList().
            //FirstOrDefault(schema => schemaId == schema.Id.ToString());
            
        if (foundSchema.parent is null && foundSchema.child is null)
        {
            Console.WriteLine("Bad schema id given");
            return;
        }

        Console.WriteLine("Please, entry new command");

        var newCommand = Console.ReadLine();

        var filter = Builders<SchemaModel>.Filter.Eq("Id", foundSchema.parent?.Id ?? foundSchema.child!.Id);

        var matchedCommandOperationName =
            _commands.FirstOrDefault(c => c.Key.Equals(newCommand)).Key;

        foundSchema.child!.LeftChildren = foundSchema.child.RightChildren = null;

        foundSchema.child.Operation = matchedCommandOperationName;

        //foundSchema.LeftChildren = foundSchema.RightChildren = null;

        //foundSchema.Operation = matchedCommandOperationName;

        if (newCommand!.Equals("CompareLess") || newCommand.Equals("CompareEqual")) InsertNodeChildren(foundSchema.child);

        await schemasCollectionObject.ReplaceOneAsync(filter, foundSchema.parent ?? foundSchema.child);

        Console.WriteLine("All went good while updating");
    }

    public static async Task ExecuteSchemasByIds(string schemaIdToBeExecuted = "",IReadOnlyList<string>? dataSet = null)
    {
        var schemasToBeExecuted = new List<SchemaModel?>();
        
        var schemasCollection = GetSchemas()
            .AsQueryable().ToList();
        
        if (schemaIdToBeExecuted == string.Empty)
        {
            Console.WriteLine("Please, enter schemas ids to execute them with comma as separator");

            var schemasIds = Console.ReadLine()!.Split(",");

            if (schemasIds.Length > 100)
            {
                Console.WriteLine("Limit of schemas to be executed exceeded(100 is max)");
                return;
            }

            foreach (var schemaId in schemasIds)
            {
                var schema = schemasCollection.FirstOrDefault(s => s.Id.ToString().Equals(schemaId));
                schemasToBeExecuted.Add(schema);
            }
        }
        else
        {
            schemasToBeExecuted.Add(schemasCollection
                .FirstOrDefault(s => s.Id.ToString().Equals(schemaIdToBeExecuted)));
        }
        
        RunSchemaOperations(schemasToBeExecuted!, dataSet);
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
                GetAllChildrenInfo(schema, response); //schemas

            while (schema.Next is not null)
            {
                response.Append($"--> Next = {schema.Next.Id}-{schema.Next.Operation}");

                GetAllChildrenInfo(schema.Next, response);

                schema.Next = schema.Next.Next;
            }
        }

        var tmp = response.ToString();

        Console.WriteLine(tmp);
    }

    private static void RunSchemaOperations(IEnumerable<SchemaModel> schemasToBeExecuted, 
        IReadOnlyList<string>? dataSet = null)
    {
        var currentCommandIndex = 0;
        
        var commandsThatRequireInputData = new List<string>
        {
            "AssignValueToVariable",
            "AssignConstantValueToVariable",
            "ReadNStore",
            "PrintValue",
            "CompareLess",
            "CompareEqual"
        };
        Parallel.ForEach(schemasToBeExecuted, schema =>
        {
            var schemaId = schema.Id.ToString();
            
            _schemasOperationsForTesting[schemaId] = new List<string>();
            
            while (true)
            {
                var schemaNext = schema.Next;
                
                _schemasOperationsForTesting[schemaId].Add(schema.Operation);
                
                if (schema.Operation.Equals("CompareLess") || schema.Operation.Equals("CompareEqual"))
                {
                    lock (ConsoleWriterLock)
                    {
                        var newOperationsBasedOnResult = Operations
                            .GetResultOfCompareOperation(schema.Operation, 
                                dataSet is not null
                                && commandsThatRequireInputData.Contains(schema.Operation) 
                                    ? dataSet[currentCommandIndex] : _testValuesForSpecificSchema
                                        .ContainsKey(schema.Id.ToString())
                                    ? _testValuesForSpecificSchema[schema.Id.ToString()] : "")
                            ? schema.RightChildren
                            : schema.LeftChildren;

                        
                            for (var i = 0; i < newOperationsBasedOnResult!.Count; i++)
                            {
                                newOperationsBasedOnResult[i].Next = i == newOperationsBasedOnResult.Count - 1 
                                    ? schemaNext : newOperationsBasedOnResult[i + 1];
                            }

                            schemaNext = newOperationsBasedOnResult.First();
                    }

                }
                else
                {
                    lock (ConsoleWriterLock)
                    {
                        Operations.GetOperationByName(schema.Operation)!.Invoke(dataSet is not null 
                            && commandsThatRequireInputData.Contains(schema.Operation) ? dataSet[currentCommandIndex]
                                : _testValuesForSpecificSchema.ContainsKey(schema.Id.ToString()) 
                                    ? _testValuesForSpecificSchema[schema.Id.ToString()] : null);
                    }
                }

                Console.WriteLine("Do you want to continue ? Yes or No");

                var whetherContinueResponse = Console.ReadLine();

                if (whetherContinueResponse!.Equals("No"))
                {
                    Console.WriteLine("Please, enter number k of max operations in single combination");

                    var k = int.Parse(Console.ReadLine()!);

                    var schemasOperations = _schemasOperationsForTesting
                        .Select(item => item.Value.ToList()).ToList();

                    TestOperations.GetInfoOfProceededPermutations(schemasOperations, k);
                    
                    return;
                }
                
                Thread.Sleep(300);

                if (schemaNext is null) break;

                if (commandsThatRequireInputData.Contains(schema.Operation))
                {
                    lock (ConsoleWriterLock)
                    {
                        if (dataSet is not null)
                        {
                            _testValuesForSpecificSchema[schema.Id.ToString()] = dataSet[currentCommandIndex];
                        }
                    }

                    currentCommandIndex++;
                }

                schema = schemaNext;
            }
        });
    }
    private static void GetAllChildrenInfo(SchemaModel root, StringBuilder allLines)
    {
        if (root.LeftChildren is not null)
        {
            allLines.Append("\tLeftChildren:");

            foreach (var leftChild in root.LeftChildren)
            {
                allLines.Append($"{leftChild.Id}-{leftChild.Operation}-->");
                GetAllChildrenInfo(leftChild, allLines);
            }

            // GetAllChildrenInfo(root.LeftChildren.First(), allLines);
        }

        if (root.RightChildren is not null)
        {
            allLines.Append("\tRightChildren:");

            foreach (var rightChild in root.RightChildren)
            {
                allLines.Append($"{rightChild.Id}-{rightChild.Operation}-->");
                GetAllChildrenInfo(rightChild, allLines);
            }
        }
        

        

        // GetAllChildrenInfo(root.RightChildren.First(), allLines);
    }

    public static void TestSchema()
    {
        Console.WriteLine("Please, enter schemas id to test them with comma as separator");

        var schemasId = Console.ReadLine()!.Split(",");

        var schemas = GetSchemas().AsQueryable().ToList();
        
        foreach (var schemaId in schemasId)
        {
            var schema = schemas.First(s => s.Id.ToString() == schemaId);
            
            Console.WriteLine("Please, enter number of test cases");

            var numberOfTestCases = int.Parse(Console.ReadLine()!);

            var currentTestNumber = 1;
            
            while (numberOfTestCases != 0)
            {
                Console.WriteLine("Please, enter data for each operation in such format: separate multiple " +
                                  "values for specific operation with comma and to distinguish between " +
                                  "each of them end data set with ;");

                var dataSetForOperations = Console.ReadLine()!.Split(";")
                    .Where(op => !string.IsNullOrEmpty(op)).ToArray();

                Console.WriteLine("Please, enter final state via specifying variables with their values in such format" +
                                  ": variableName=variableValue,secondVariableName=secondVariableValue");

                var expectedStateVariablesValuesAfterTests = Console.ReadLine()!.Split(",");

                ExecuteSchemasByIds(schema.Id.ToString(),dataSetForOperations).GetAwaiter().GetResult();

                var isStateCorrect = true;

                foreach (var expectedStateVariableValueAfterTests in expectedStateVariablesValuesAfterTests)
                {
                    var variableName = expectedStateVariableValueAfterTests.Split("=")[0];
                    
                    var variableValue = expectedStateVariableValueAfterTests.Split("=")[1];

                    if (State.state[variableName] == int.Parse(variableValue)) continue;
                    
                    isStateCorrect = false;
                    
                    break;
                }

                var output = isStateCorrect ? $"TestCase-{currentTestNumber} case has been passed successfully for schema" +
                                              $" with id = {schema.Id.ToString()}" 
                    : $"TestCase-{currentTestNumber} has failed for schema with id = {schema.Id.ToString()}";

                Console.WriteLine(output);

                numberOfTestCases -= 1;
                
                currentTestNumber++;
            }
            
        }
        
    }
}