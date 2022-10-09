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
    
    await File.AppendAllTextAsync(filename ?? throw new InvalidOperationException(), new
    {
        Id = Guid.NewGuid().ToString(),
        Schemas = string.Join(" ", chosenCommandsIds?.Split(",")
            .Select(commandId => _commands
                .FirstOrDefault(c => c.Value.Equals(int.Parse(commandId))))!)
    }.ToString() + "\n");

    Console.WriteLine("All went good");
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