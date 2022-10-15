using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using BuildingSoftwareLabaratoryWork.Models;

namespace BuildingSoftwareLabaratoryWork;

public static class Schema
{
    private static Dictionary<string, int> _commands;

    public static void Init(Dictionary<string,int> commands)
    {
        _commands = commands;
    }
    
   public static async Task CreateSchema()
{
    //TODO : 1) if user adds compare commands then we gotta ask him about flow, they should specify what happens if condition is
    // TODO: true and when false 

    Console.WriteLine("Write up ids of commands with comma as separator");

    var chosenCommandsIds = Console.ReadLine();

    Console.WriteLine("Please, enter filename in which schemas will be stored");

    var filename = Console.ReadLine();

    await File.AppendAllTextAsync(filename ?? throw new InvalidOperationException(), JsonSerializer.Serialize(new FileDataViewModel
    {
        Id = Guid.NewGuid(),
        Schema = string.Join("-", chosenCommandsIds?.Split(",")
            .Select(commandId => _commands
                .FirstOrDefault(c => c.Value.Equals(int.Parse(commandId))))!)
    }));

    Console.WriteLine("All went good");
    
    // new
    // {
    // //     Id = Guid.NewGuid().ToString(),
    //     Schema = string.Join(" ", chosenCommandsIds?.Split(",")
    //         .Select(commandId => _commands
    //             .FirstOrDefault(c => c.Value.Equals(int.Parse(commandId))))!)
    // } + "\n"
}

   public static async Task ModifySchemas()
   {
       Console.WriteLine("Please, enter filename");
       
       var filename = Console.ReadLine();

       Console.WriteLine("Please, enter schema id");

       var schemaId = Console.ReadLine(); // Guid

       var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;

       // await using var fs = new FileStream($"{path}\\{filename}", FileMode.Open);
       //
       // var schemas = await JsonSerializer
       //     .DeserializeAsync<List<FileDataViewModel>>(fs);

       var lines = await File.ReadAllLinesAsync(filename!);

       var schema = "";
       
       var index = -1;
       
       var counter = 0;

       foreach (var line in lines)
       {
           var parsedLine = JsonSerializer.Deserialize<FileDataViewModel>(line);

           if (parsedLine!.Id.Equals(Guid.Parse(schemaId ?? throw new InvalidOperationException())))
           {
               schema = parsedLine.Schema;
               index = counter;
           }

           counter++;
       }

       if (string.IsNullOrEmpty(schema))
       {
           Console.WriteLine("Couldn't find schema");
           return;
       }

       var schemasArray = schema.Split("-");

       Console.WriteLine("Please, enter type of modification: 1 - change all commands by id; 2 - delete commands by id");

       var userResponse = Console.ReadLine();

       Console.WriteLine("Please, enter command id");
       
       var commandId = Console.ReadLine();

       var updatedCommandsIds = "";

       if (userResponse!.Equals("1"))
       {
           Console.WriteLine("Please, enter new command id to replace old ones");
           
           var newCommandId = Console.ReadLine();

           var tmp = Regex.Matches(schema, @"\d+");

           var tmpArray =
               tmp.Cast<Match>().Select(m => m.Value).ToList();

           for (var i = 0; i < tmpArray.Count; i++)
           {
               if (tmpArray[i].Equals(commandId))
               {
                   tmpArray[i] = newCommandId!;
               }
           }

           updatedCommandsIds = string.Join(",", tmpArray);

           //
           // var newCommand = _commands
           //     .First(x => x.Value.Equals(Int32.Parse(newCommandId!))).Key;
           //
           // schemasArray = schemasArray.Where(x => x.Contains($"{commandId}"))
           //     .Select(c => new Regex(@"\w").Replace(c,newCommand)).ToArray();
       }
       else
       {
           var tmp = Regex.Matches(schema, @"\d+");
           
           var tmpArray =
               tmp.Cast<Match>().Select(m => m.Value).ToList();
           
           updatedCommandsIds = string.Join(",", tmpArray.Where(x => x != commandId));
           //schemasArray = schemasArray.Where(x => x.Contains($"{commandId}") == false).ToArray();
       }

       lines[index] = JsonSerializer.Serialize(new FileDataViewModel
       {
           Id = Guid.Parse(schemaId!),
           Schema = string.Join("-", updatedCommandsIds?.Split(",").Where(c => string.IsNullOrEmpty(c) == false)
               .Select(id =>  _commands
                   .FirstOrDefault(c => c.Value.Equals(int.Parse(id))))!)
       });

       await File.WriteAllLinesAsync(filename!, lines);

       Console.WriteLine("All went good while modifying schema");
   }
   

public static void ExecuteSchemaById()
{
    // foreach (var chosenCommandId in chosenCommandsIds!)
    // {
    //     switch (chosenCommandId)
    //     {
    //         case "1":
    //             var data = Operations.ReadOneVariableValueToAssignForOther();
    //             Operations.AssignOneVariableValueToOther(data.V1, data.V2, state);
    //             break;
    //         case "2":
    //             var dataFromUser = Operations.ReadConstantToAssignToVariable();
    //             Operations.AssignConstantValueToOther(dataFromUser.V, dataFromUser.C, state, constants);
    //             break;
    //         case "3":
    //             var readData = Operations.ReadToAssign();
    //             Operations.AssignValue(readData.Name, readData.Value, state);
    //             break;
    //         case "4":
    //             Operations.Print(Operations.ReadValueToPrint().V,state);
    //             break;
    //         case "5":
    //             var stringBuilder = new StringBuilder();
    //
    //             foreach (var item in state)
    //             {
    //                 stringBuilder.Append($"{item.Key} - {item.Value}");
    //             }
    //
    //             Console.WriteLine(stringBuilder);
    //             break;
    //         case "6":
    //             var constantsResponse = new StringBuilder();
    //
    //             foreach (var item in constants)
    //             {
    //                 constantsResponse.Append($"{item.Key} - {item.Value}");
    //             }
    //
    //             Console.WriteLine(constantsResponse);
    //             break;
    //     }
    // }
}
}