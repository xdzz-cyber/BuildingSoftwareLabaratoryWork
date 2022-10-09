using System.Collections.Concurrent;
using BuildingSoftwareLabaratoryWork;

//var tasks = new ConcurrentQueue<Task>();
var state = new ConcurrentDictionary<string, int>();

Console.WriteLine("1 - create new schema\n2 - show schemas\n3 - show schema details by id" +
                  "\n4 - edit schema by id\n5 - save schema to file by id\n6 - execute schema by id\ntest schema - 7\n8 - exit");

var response = Console.ReadLine();
Schema.Init(Operations.operations);
while (response != "8")
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
        default:
            Console.WriteLine("Please, enter correct command");
            break;
    }
    
    Console.WriteLine("Enter new command");
    response = Console.ReadLine();
}

