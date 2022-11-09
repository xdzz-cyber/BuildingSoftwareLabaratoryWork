using System.Collections.Concurrent;
using System.Text;
using BuildingSoftwareLabaratoryWork.Common;
using BuildingSoftwareLabaratoryWork.Models;

namespace BuildingSoftwareLabaratoryWork;

public static class Operations
{
    public static readonly Dictionary<string, Action?> operations;
    // new() 
    // {{"Assign",() => {}}, {"CompareLess",2}, {"CompareEqual",3} , {"ReadNStore",4}, {"PrintValue",5}, {"ShowState",6}, 
    //     {"ShowConstants"} };

    static Operations()
    {
        var operationsName = new List<string>
        {
            "AssignValueToVariable", "AssignConstantValueToVariable", "CompareLess", "CompareEqual", "ReadNStore",
            "PrintValue",
            "ShowState", "ShowConstants"
        };

        operations = new Dictionary<string, Action?>();

        foreach (var operationName in operationsName)
            switch (operationName)
            {
                case "AssignValueToVariable":
                    operations.Add(operationName, Value1);
                    break;
                case "AssignConstantValueToVariable":
                    operations.Add(operationName, Action);
                    break;
                case "CompareLess":
                    operations.Add(operationName, null);
                    break;
                case "CompareEqual":
                    operations.Add(operationName, null);
                    break;
                case "ReadNStore":
                    operations.Add(operationName, Value);
                    break;
                case "PrintValue":
                    operations.Add(operationName, Action1);
                    break;
                case "ShowState":
                    operations.Add(operationName, ShowState);
                    break;
                case "ShowConstants":
                    operations.Add(operationName, ShowConstants);
                    break;
            }
    }

    private static void Action1()
    {
        var readData = ReadValueToPrint();
        
        // Console.WriteLine("Please, enter variable name, V");

        //var value = Console.ReadLine();
        Print(readData.V, State.state);
        //Console.WriteLine($"{value} has value {State.state[value!]}");
    }

    private static void Value1()
    {
        var readData = ReadOneVariableValueToAssignForOther();
        
        //Console.WriteLine("Please, enter V1 and V2 names with comma as separator");

        //var values = Console.ReadLine();

        //var response = values?.Split(",");
        
        //State.state[response![0]] = State.state[response[1]];
        AssignOneVariableValueToOther(readData.V1, readData.V2, State.state);
    }

    private static void Action()
    {
        var readData = ReadConstantToAssignToVariable();
        
        //Console.WriteLine("Please, enter constant name C, and variable name V");

        //var values = Console.ReadLine();

        //var response = values?.Split(",");

        AssignConstantValueToOther(readData.V, readData.C, State.state, Constants.constants);
        //State.state[response![1]] = Constants.constants[response[0]];
    }

    private static void Value()
    {
        var readData = ReadToAssign();
        State.state[readData.Name] = readData.Value;
        //Console.WriteLine("Please, enter value");

        //var value = Console.ReadLine();
        //State.state[$"{Guid.NewGuid().ToString()}{new Random().Next(1, 10000)}"] = int.Parse(value!);
    }

    public static bool GetResultOfCompareOperation(SchemaModel schema)
    {
        Console.WriteLine($"Please, enter value name to compare " +
                          $"{(schema.Operation.Equals("CompareLess") ? "if less" : "if equals")} with comma as separator");

        var variableName = Console.ReadLine();

        Console.WriteLine("Please, enter constant name to compare with comma as separator");

        var constantName = Console.ReadLine();

        return schema.Operation.Equals("CompareLess")
            ? Compare(variableName!, constantName!) == -1
            : Compare(variableName!, constantName!) == 0;
    }

    public static Action? GetOperationByName(string operationName)
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        return operations[operationName];
    }

    private static int Compare(string variableName, string constantName)
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        return State.state[variableName].CompareTo(Constants.constants[constantName]);
    }

    private static ReadDataViewModel ReadToAssign()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        Console.WriteLine("Please, enter value");

        var value = Console.ReadLine();

        return new ReadDataViewModel
        {
            Name = $"{Guid.NewGuid().ToString()}{new Random().Next(1, 10000)}",
            Value = int.Parse(value ?? throw new InvalidOperationException())
        };
    }

    private static AssignOneVariableValueToOtherViewModel ReadOneVariableValueToAssignForOther()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        Console.WriteLine("Please, enter V1 and V2 names with comma as separator");

        var values = Console.ReadLine();

        var response = values?.Split(",");

        return new AssignOneVariableValueToOtherViewModel
        {
            V1 = response![0],
            V2 = response[1]
        };
    }

    private static AssignConstantToVariableViewModel ReadConstantToAssignToVariable()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        Console.WriteLine("Please, enter constant name C, and variable name V");

        var values = Console.ReadLine();

        var response = values?.Split(",");

        return new AssignConstantToVariableViewModel
        {
            C = response![0],
            V = response[1]
        };
    }

    private static ReadValueToPrintViewModel ReadValueToPrint()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        Console.WriteLine("Please, enter variable name, V");

        var value = Console.ReadLine();

        return new ReadValueToPrintViewModel
        {
            V = value!
        };
    }

    private static void ShowConstants()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        var response = new StringBuilder();

        foreach (var item in Constants.constants) response.AppendLine($"{item.Key} = {item.Value}");

        Console.WriteLine(response.ToString());
    }

    private static void ShowState()
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        var response = new StringBuilder();

        foreach (var item in State.state) response.AppendLine($"{item.Key} = {item.Value}");

        Console.WriteLine(response.ToString());
    }

    private static void AssignOneVariableValueToOther(string v1, string v2, ConcurrentDictionary<string, int> state)
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        state[v1] = state[v2];
    }

    private static void AssignConstantValueToOther(string v, string c,
        ConcurrentDictionary<string, int> state, Dictionary<string, int> constants)
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        state[v] = constants[c];
    }

    private static void Print(string variableName, ConcurrentDictionary<string, int> state)
    {
        //Console.WriteLine($"Current thread id = {Environment.CurrentManagedThreadId}");

        Console.WriteLine($"{variableName} has value {state[variableName]}");
    }
}