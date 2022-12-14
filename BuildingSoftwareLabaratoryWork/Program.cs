using System.Collections.Concurrent;
using BuildingSoftwareLabaratoryWork;
using BuildingSoftwareLabaratoryWork.Common;

var state = new ConcurrentDictionary<string, int>();

Console.WriteLine("1 - create new schema\n2 - show schemas" +
                  "\n3 - modify schema by id\n4 - execute schemas by id\n5 - test schema by id\n6 - exit");

var response = Console.ReadLine();
Worker.Init(Operations.operations, state , MongoDbInformation.GetClient());

while (response != "6")
{
    // making one action
    switch (response)
    {
        case "1":
            Console.WriteLine($"You can add up to 100 commands in single schema. Commands available are: " +
                              $"{string.Join(", ", Operations.operations.Select(c => $"{c.Key}"))}");
            
            Worker.CreateSchema().GetAwaiter().GetResult();
            break;
        case "2":
            Worker.ShowSchemas();
            break;
        case "3":
            Worker.ModifySchema().GetAwaiter().GetResult();
            break;
        case "4":
            Worker.ExecuteSchemasByIds().GetAwaiter().GetResult();
            break;
        case "5":
            Worker.TestSchema();
            break;
    }
    
    Console.WriteLine("1 - create new schema\n2 - show schemas" +
                      "\n3 - modify schema by id\n4 - execute schemas by id\n5 - test schema by id\n6 - exit");
    
    Console.WriteLine("Enter new command");

    response = Console.ReadLine();
}

