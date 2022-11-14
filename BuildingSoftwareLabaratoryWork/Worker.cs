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
    private static readonly object ConsoleWriterLock = new object();

    public static void Init(Dictionary<string, Action?> commands, ConcurrentDictionary<string, int> state,
        MongoClient mongoClient)
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

    private static (List<string>, List<string>) ReturnRequestedCommandsIfConditionExist()
    {
        Console.WriteLine("Please, enter commands that will be executed in case of true");

        var commandsIfTrue = Console.ReadLine()!.Split(",").ToList();

        Console.WriteLine("Please, enter commands that will be executed in case of false");

        var commandsIfFalse = Console.ReadLine()!.Split(",").ToList();

        return (commandsIfFalse, commandsIfTrue);
    }

    private static void InsertNodeChildren(SchemaModel root) // ShowState, CompareLess, PrintValue, CompareLess,
    // Show Constants, ReadNStore
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

    public static async Task ExecuteSchemasByIds()
    {
        Console.WriteLine("Please, enter schemas ids to execute them with comma as separator");

        var schemasIds = Console.ReadLine()!.Split(",");

        var schemasCollection = GetSchemas()
            .AsQueryable().ToList();

        var threads = new List<Thread>();

        //var actions = new BlockingCollection<Action>();


       // var tasks = new List<List<Task>>();
       // var operations = new List<string>();
       var schemasToBeExecuted = new List<SchemaModel?>();
        foreach (var schemaId in schemasIds)
        {
            //WaitCallback commonDelegate = _ => Operations.GetOperationByName(schema.Operation); // just for initialization purposes
            //var commonDelegate = new Action<object>(_ => Console.Beep());
            var schema = schemasCollection.FirstOrDefault(s => s.Id.ToString().Equals(schemaId));
            schemasToBeExecuted.Add(schema);
            //var operationsToBeExecuted = new List<string> { };

           // while (schema is not null)
           // {
              //  operationsToBeExecuted.Add(schema.Operation);
                // if (schema.Operation.Equals("CompareLess") || schema.Operation.Equals("CompareEqual"))
                // {
                //     var newOperationsBasedOnResult = Operations.GetResultOfCompareOperation(schema!)
                //         ? schema!.RightChildren!.Select(y => y.Operation)
                //         : schema!.LeftChildren!.Select(y => y.Operation);
                //
                //     operationsToBeExecuted.AddRange(newOperationsBasedOnResult);
                // }
                // else
                // {
                //     operationsToBeExecuted.Add(schema.Operation);
                // }
                
                //commonDelegate += _ => Operations.GetOperationByName(schemaCopy.Operation);

                // if (schema.Operation.Equals("CompareLess") || schema.Operation.Equals("CompareEqual"))
                // {
                //  
                //     Console.WriteLine($"Please, enter value name to compare " +
                //                       $"{(schema.Operation.Equals("CompareLess") ? "if less" : "if equals")} with comma as separator");
                //
                //     var variableName = Console.ReadLine();
                //
                //     Console.WriteLine("Please, enter constant name to compare with comma as separator");
                //
                //     var constantName = Console.ReadLine();
                //
                //     var result = schema.Operation.Equals("CompareLess")
                //         ? Operations.Compare(variableName!, constantName!) == -1
                //         : Operations.Compare(variableName!, constantName!) == 0;
                //
                //     var commandsToBeExecutedBasedOnResult = result 
                //         ? schema.RightChildren : schema.LeftChildren;
                //
                //     operationsToBeExecuted.AddRange(commandsToBeExecutedBasedOnResult!
                //         .Select(x => x.Operation));
                // }

              //  if (schema.Next is null) break;
              //  schema = schema.Next;
           // }
            //operations.AddRange(operationsToBeExecuted);
            //operations.AddRange(operationsToBeExecuted.Select(Operations.GetOperationByName).ToArray()!);
            // threads.Add(operationsToBeExecuted.Select(x 
            //     => new Thread(new ThreadStart(Operations.GetOperationByName(x)!))).ToList());

            // threads.Add(new Thread(() =>
            // {
            //
            //     operationsToBeExecuted.ForEach(o =>
            //     {
            //         Thread.Sleep(100);
            //     });
            // }));
            // tasks.Add(operationsToBeExecuted
            //     .Select(x => new Task(Operations.GetOperationByName(x)!)).ToList());
            // var tasks = operationsToBeExecuted.Select(x => new Task(Operations.GetOperationByName(x)!));
            //
            // foreach (var task in tasks)
            // {
            //     task.RunSynchronously();
            // }
            // operationsToBeExecuted.ForEach(x =>
            // {
            // });

            // var newThread = new Thread(async () =>
            // {
            //     
            // });
            //
            // threads.Add(newThread);

            // var newThread = new Thread(() =>
            // {
            //     operationsToBeExecuted.ForEach(x =>
            //     {
            //         Operations.GetOperationByName(x)!.Invoke();
            //     });
            // });
            // operationsToBeExecuted.ForEach(x =>
            // {
            //     if (x.Equals("CompareLess") || x.Equals("CompareEqual"))
            //     {
            //         var newOperationsBasedOnResult = Operations.GetResultOfCompareOperation(schema!)
            //             ? schema!.RightChildren!.Select(y => y.Operation)
            //             : schema!.LeftChildren!.Select(y => y.Operation);
            //
            //         foreach (var newOperationBasedOnResult in newOperationsBasedOnResult)
            //         {
            //             //actions.Add(Operations.GetOperationByName(newOperationBasedOnResult)!);
            //         }
            //     }
            // });
            // newThread.Start();
            //ThreadPool.QueueUserWorkItem(commonDelegate);   
            // Task.WaitAll(operationsToBeExecuted.Select(x => new Task(Operations.GetOperationByName(x)!))
            //     .ToArray());
            //operationsToBeExecuted.ForEach(op => Operations.GetOperationByName(op));
            //threads.Add(newThread);
        }
        
        //Parallel.Invoke(operations.ToArray()); // TODO: 1) gotta process compare operations to not get exceptions
        //SchemaModel? currentPrevSchema = null;

        // foreach (var schema in schemasToBeExecuted)
        // {
        //     ThreadPool.QueueUserWorkItem(state =>
        //     {
        //         var schemaCopy = schema;
        //         while (true)
        //         {
        //             var schemaNext = schemaCopy!.Next;
        //             if (schemaCopy.Operation.Equals("CompareLess") || schemaCopy.Operation.Equals("CompareEqual"))
        //             {
        //                 var newOperationsBasedOnResult = Operations
        //                     .GetResultOfCompareOperation(schemaCopy.Operation)
        //                     ? schemaCopy.RightChildren
        //                     : schemaCopy.LeftChildren;
        //                 newOperationsBasedOnResult![^1].Next = schemaNext; //!.Add(schemaNext!);
        //                 schemaNext = newOperationsBasedOnResult.First();
        //             }
        //             else
        //             {
        //                 Operations.GetOperationByName(schemaCopy.Operation)!.Invoke();
        //                 Thread.Sleep(100);
        //             }  
        //             if (schemaNext is null) break;
        //             schemaCopy = schemaNext;
        //         }
        //     });
        // }
        
        RunSchemaOperations(schemasToBeExecuted!);

        //threads.ForEach(thread => thread.Start());
        // Parallel.ForEach(tasks, task =>
        // {
        //     task[startIndex].RunSynchronously();
        //     startIndex += 1;
        // });

        // foreach (var thread in threads)
        // {
        //     thread.Start();
        // }

        // for (var i = 0; i < threads.MaxBy(t => t.Count)!.Count; i++)
        // {
        //    threads.ForEach(t =>
        //    {
        //        if (i < t.Count)
        //        {
        //            t[i].Start();
        //            t[i].Join();
        //        }
        //    });
        // }

        // foreach (var action in actions)
        // {
        //     action?.DynamicInvoke();
        // }
        // while (actions.Count != 0)
        // {
        //     var action = actions.Take();
        //     action();
        // }

        // threads.ForEach(t =>
        // {
        //     t.Start();
        //     t.Join();
        // });
    }

    public static void ShowSchemas() // ShowState, CompareLess, PrintValue, CompareLess,
        // Show Constants, ReadNStore
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

    private static void RunSchemaOperations(IEnumerable<SchemaModel> schemasToBeExecuted)
    {
        Parallel.ForEach(schemasToBeExecuted, schema => 
        {
            while (true)
            {
                var schemaNext = schema.Next;
                if (schema.Operation.Equals("CompareLess") || schema.Operation.Equals("CompareEqual"))
                {
                    lock (ConsoleWriterLock)
                    {
                        var newOperationsBasedOnResult = Operations.GetResultOfCompareOperation(schema.Operation)
                            ? schema.RightChildren
                            : schema.LeftChildren;

                        
                            for (var i = 0; i < newOperationsBasedOnResult!.Count; i++)
                            {
                                newOperationsBasedOnResult[i].Next = i == newOperationsBasedOnResult.Count - 1 
                                    ? schemaNext : newOperationsBasedOnResult[i + 1];
                            }
                            //newOperationsBasedOnResult.First().Next = newOperationsBasedOnResult[1];
                        
                        schemaNext = newOperationsBasedOnResult.First();
                    }
                    
                }
                else
                {
                    lock (ConsoleWriterLock)
                    {
                        Operations.GetOperationByName(schema.Operation)!.Invoke();
                    }
                }  
                Thread.Sleep(300);

                if (schemaNext is null) break;
                
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
}