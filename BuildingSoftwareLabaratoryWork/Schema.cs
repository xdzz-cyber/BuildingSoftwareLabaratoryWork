using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BuildingSoftwareLabaratoryWork.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BuildingSoftwareLabaratoryWork;

public static class Schema
{
    private static Dictionary<string, int> _commands;

    public static void Init(Dictionary<string,int> commands)
    {
        _commands = commands;
    }

    public static async Task ShowSchemas()
    {
        Console.WriteLine("Please, enter filename");

        var filename = Console.ReadLine();

        var rawSchemas = await GetSchemas(filename!);

        Console.WriteLine($"\n{rawSchemas}");
    }

    private static async Task<string> GetSchemas(string filename)
    {

        return await File.ReadAllTextAsync(filename);
    }
    
   public static async Task CreateSchema()
{
   
    Console.WriteLine("Write up ids of commands with comma as separator");

    var chosenCommandsIds = Console.ReadLine();

    var commandsIdsIfTrue = string.Empty;

    var commandsIdsIfFalse = string.Empty;

    foreach (var chosenCommandId in chosenCommandsIds!.Split(","))
    {
        if (!chosenCommandId.Equals("2") && !chosenCommandId.Equals("3")) continue;

        Console.WriteLine("Please, enter commands ids that will be executed in case of true");

        commandsIdsIfTrue = Console.ReadLine();

        Console.WriteLine("Please, enter commands ids that will be executed in case of false");

        commandsIdsIfFalse = Console.ReadLine();
    }

    Console.WriteLine("Please, enter filename in which schemas will be stored");

    var filename = Console.ReadLine();

    var schemaToBeInserted = new List<string?>();

    var baseSchemaId = Guid.NewGuid();
    
    foreach (var chosenCommandId in chosenCommandsIds.Split(","))
    {
        var matchedCommand =  _commands
            .FirstOrDefault(c => c.Value.Equals(int.Parse($"{chosenCommandId}")));

        var flag = chosenCommandId.Equals("2") || chosenCommandId.Equals("3");
        
        var tmpObj = new SchemaCommandViewModel
        {
            Id = matchedCommand.Value.ToString(),
            Name = matchedCommand.Key,
            TrueCaseScenario = flag ? string.Join("-", commandsIdsIfTrue!.Split(",")
                .Select(x => _commands
                    .FirstOrDefault(c => c.Value.Equals(int.Parse(x))))) : "",
            FalseCaseScenario = flag ? string.Join("-", commandsIdsIfFalse!.Split(",")
                .Select(x => _commands
                    .FirstOrDefault(c => c.Value.Equals(int.Parse(x))))) : "",
            BaseSchemaId = baseSchemaId
        };
        
        schemaToBeInserted.Add(JsonConvert.SerializeObject(tmpObj));
    }
    

    await File.AppendAllTextAsync(filename ?? throw new InvalidOperationException(), 
        string.Join("--", schemaToBeInserted) + "\n");
    

    Console.WriteLine("All went good");
}

   public static async Task ModifySchemas()
   {
       Console.WriteLine("Please, enter filename");
       
       var filename = Console.ReadLine();

       Console.WriteLine("Please, enter schema id");

       var schemaId = Console.ReadLine(); // Guid

       var lines = await File.ReadAllLinesAsync(filename!);

       var index = 0;
       
       var schema = string.Empty;

       foreach (var line in lines)
       {
           var tmp = line.Split("--");
        
           var parsedLine = tmp
               .FirstOrDefault(x => JsonConvert.DeserializeObject<SchemaCommandViewModel>(x)!.BaseSchemaId.Equals(Guid.Parse(schemaId)));
        

           if (parsedLine != null)
           {
               schema = line;
               break;
           }

           index++;
       }
       
       var commands = schema.Split("--");
       
       var parsedCommands = new List<SchemaCommandViewModel>();
       
       foreach (var command in commands)
       {
           parsedCommands.Add(JsonConvert.DeserializeObject<SchemaCommandViewModel>(command)!);
       }
       
       Console.WriteLine("Please, enter type of modification: 1 - change all commands by id; 2 - delete commands by id");
       
       var userResponse = Console.ReadLine();
       
       Console.WriteLine("Please, enter command id");

       var commandId = Console.ReadLine();
       
       var chosenCommand = _commands.First(c => c.Value.Equals(int.Parse(commandId!)));

       if (userResponse!.Equals("1"))
       {
           Console.WriteLine("Please, enter new command id");

           var newCommandId = Console.ReadLine();
           
           var newCommand = _commands.First(c => c.Value.Equals(int.Parse(newCommandId!)));

           foreach (var parsedCommand in parsedCommands)
           {
               if (parsedCommand.Id.Equals(chosenCommand.Value.ToString()))
               {
                   parsedCommand.Id = newCommand.Value.ToString();
                   parsedCommand.Name = newCommand.Key;
               }
           }
       }
       else
       {
           parsedCommands = parsedCommands.Where(c => c.Id != chosenCommand.Value.ToString()).ToList();
       }

       lines[index] = parsedCommands.Select(JsonConvert.SerializeObject).ToString()!;

       await File.WriteAllLinesAsync(filename!, lines);
       
       Console.WriteLine("All went good while modifying schema");


       // var schema = "";
       //
       // var index = -1;
       //
       // var counter = 0;
       //
       // foreach (var line in lines)
       // {
       //     var parsedLine = JsonSerializer.Deserialize<FileDataViewModel>(line);
       //
       //     if (parsedLine!.Id.Equals(Guid.Parse(schemaId ?? throw new InvalidOperationException())))
       //     {
       //         schema = parsedLine.Schema;
       //         index = counter;
       //     }
       //
       //     counter++;
       // }
       //
       // if (string.IsNullOrEmpty(schema))
       // {
       //     Console.WriteLine("Couldn't find schema");
       //     return;
       // }
       //
       // Console.WriteLine("Please, enter type of modification: 1 - change all commands by id; 2 - delete commands by id");
       //
       // var userResponse = Console.ReadLine();
       //
       // Console.WriteLine("Please, enter command id");
       //
       // var commandId = Console.ReadLine();
       //
       // var updatedCommandsIds = "";
       //
       // if (userResponse!.Equals("1"))
       // {
       //     Console.WriteLine("Please, enter new command id to replace old ones");
       //     
       //     var newCommandId = Console.ReadLine();
       //
       //     var tmp = Regex.Matches(schema, @"\d+");
       //
       //     var tmpArray =
       //         tmp.Cast<Match>().Select(m => m.Value).ToList();
       //
       //     for (var i = 0; i < tmpArray.Count; i++)
       //     {
       //         if (tmpArray[i].Equals(commandId))
       //         {
       //             tmpArray[i] = newCommandId!;
       //         }
       //     }
       //
       //     updatedCommandsIds = string.Join(",", tmpArray);
       // }
       // else
       // {
       //     var tmp = Regex.Matches(schema, @"\d+");
       //     
       //     var tmpArray =
       //         tmp.Cast<Match>().Select(m => m.Value).ToList();
       //     
       //     updatedCommandsIds = string.Join(",", tmpArray.Where(x => x != commandId));
       // }
       //
       // lines[index] = JsonSerializer.Serialize(new FileDataViewModel
       // {
       //     Id = Guid.Parse(schemaId!),
       //     Schema = string.Join("-", updatedCommandsIds?.Split(",").Where(c => string.IsNullOrEmpty(c) == false)
       //         .Select(id =>  _commands
       //             .FirstOrDefault(c => c.Value.Equals(int.Parse(id))))!)
       // });
       //
       // await File.WriteAllLinesAsync(filename!, lines);
       //
       // Console.WriteLine("All went good while modifying schema");
   }
   

    public static async Task ExecuteSchemaById()
{
    Console.WriteLine("Please, enter filename");
       
    var filename = Console.ReadLine();

    Console.WriteLine("Please, enter schema id");

    var schemaId = Console.ReadLine(); // Guid

    var lines = await File.ReadAllLinesAsync(filename!);

    var schema = string.Empty;

    foreach (var line in lines)
    {
        var tmp = line.Split("--");
        
        var parsedLine = tmp
            .FirstOrDefault(x => JsonConvert.DeserializeObject<SchemaCommandViewModel>(x)!.BaseSchemaId.Equals(Guid.Parse(schemaId)));
        

        if (parsedLine != null)
        {
            schema = line;
        }
    }
    
    var commands = schema.Split("--");
    
    foreach (var command in commands)
    {
        var tmp = JsonConvert.DeserializeObject<SchemaCommandViewModel>(command);
        Console.WriteLine(tmp);
    }

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