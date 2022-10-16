using System.Collections.Concurrent;
using BuildingSoftwareLabaratoryWork;

//var tasks = new ConcurrentQueue<Task>();
var state = new ConcurrentDictionary<string, int>();

Console.WriteLine("1 - create new schema\n2 - show schemas" +
                  "\n3 - modify schema by id\n4 - execute schema by id\n5 - test schema by id\n6 - exit");

var response = Console.ReadLine();
Schema.Init(Operations.operations);
while (response != "6")
{
    // making one action
    switch (response)
    {
        case "1":
            Console.WriteLine($"You can add up to 100 commands in single schema. Commands available are: " +
                              $"{string.Join(", ", Operations.operations.Select(c => $"{c.Key} - {c.Value}"))}");
            
            //tasks.Append(Task.Run(CreateSchema));
            Schema.CreateSchema().GetAwaiter().GetResult();
            break;
        case "2":
            Schema.ShowSchemas().GetAwaiter().GetResult();
            break;
        case "3":
            Schema.ModifySchemas().GetAwaiter().GetResult();
            break;
        case "4":
            Schema.ExecuteSchemaById().GetAwaiter().GetResult();
            break;
        default:
            Console.WriteLine("Please, enter correct command");
            break;
    }
    
    Console.WriteLine("1 - create new schema\n2 - show schemas" +
                      "\n3 - modify schema by id\n4 - execute schema by id\n5 - test schema by id\n6 - exit");
    
    Console.WriteLine("Enter new command");

    response = Console.ReadLine();
}

