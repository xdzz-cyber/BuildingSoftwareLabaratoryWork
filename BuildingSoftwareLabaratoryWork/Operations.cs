using System.Collections.Concurrent;
using System.Text;
using BuildingSoftwareLabaratoryWork.Common;
using BuildingSoftwareLabaratoryWork.Models;

namespace BuildingSoftwareLabaratoryWork;

public static class Operations
{
    public static readonly Dictionary<string, Action<string?>?> operations;

    static Operations()
    {
        var operationsName = new List<string>
        {
            "AssignValueToVariable", "AssignConstantValueToVariable", "CompareLess", "CompareEqual", "ReadNStore",
            "PrintValue",
            "ShowState", "ShowConstants"
        };

        operations = new Dictionary<string, Action<string?>?>();

        foreach (var operationName in operationsName)
            switch (operationName)
            {
                case "AssignValueToVariable":
                    operations.Add(operationName, AssignValueToVariable);
                    break;
                case "AssignConstantValueToVariable":
                    operations.Add(operationName, AssignConstantValueToVariable);
                    break;
                case "CompareLess":
                    operations.Add(operationName, null);
                    break;
                case "CompareEqual":
                    operations.Add(operationName, null);
                    break;
                case "ReadNStore":
                    operations.Add(operationName, ReadNStore);
                    break;
                case "PrintValue":
                    operations.Add(operationName, PrintValue);
                    break;
                case "ShowState":
                    operations.Add(operationName, ShowState);
                    break;
                case "ShowConstants":
                    operations.Add(operationName, ShowConstants);
                    break;
            }
    }

    private static void PrintValue(string? testValue = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");

        var readData = new ReadValueToPrintViewModel();

        if (string.IsNullOrEmpty(testValue))
        {
            readData = ReadValueToPrint();
        }
        else
        {
            readData.V = testValue;
        }
        
        Print(readData.V, State.state);
    }

    private static void AssignValueToVariable(string? testValues = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");
        
        var readData = new AssignOneVariableValueToOtherViewModel();
        
        if (string.IsNullOrEmpty(testValues))
        {
            readData = ReadOneVariableValueToAssignForOther();    
        }
        else
        {
            readData.V1 = testValues!.Split(",")[0];
            readData.V2 = testValues.Split(",")[1];
        }
        
        AssignOneVariableValueToOther(readData.V1, readData.V2, State.state);
    }

    private static void AssignConstantValueToVariable(string? testValues = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");
        
        var readData = new AssignConstantToVariableViewModel();

        if (string.IsNullOrEmpty(testValues))
        {
            readData = ReadConstantToAssignToVariable();
        }
        else
        {
            readData.C = testValues!.Split(",")[0];
            readData.V = testValues.Split(",")[1];
        }

        AssignConstantValueToOther(readData.V, readData.C, State.state, Constants.constants);
    }

    private static void ReadNStore(string? testValues = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");
        
        var readData = new ReadDataViewModel();

        if (string.IsNullOrEmpty(testValues))
        {
            readData = ReadToAssign();
        }
        else
        {
            readData.Name = testValues.Split(",")[1];
            readData.Value = int.Parse(testValues.Split(",")[0]);
        }
        
        State.state[readData.Name] = readData.Value;
    }

    public static bool GetResultOfCompareOperation(string operationName, string testValues = "")
    {
        var values = testValues;
        
        if (string.IsNullOrEmpty(testValues))
        {
            Console.WriteLine($"Please, enter value name to compare " +
                              $"{(operationName.Equals("CompareLess") ? "if less" : "if equals")} and constant name with" +
                              $" comma as separator");
            values = Console.ReadLine();
        }

        return operationName.Equals("CompareLess")
            ? Compare(values!.Split(",")[0], values.Split(",")[1]) == -1
            : Compare(values!.Split(",")[0], values.Split(",")[1]) == 0;
    }

    public static Action<string?>? GetOperationByName(string operationName)
    {
        return operations[operationName];
    }

    private static int Compare(string variableName, string constantName)
    {
        return State.state[variableName].CompareTo(Constants.constants[constantName]);
    }

    private static ReadDataViewModel ReadToAssign()
    {
        Console.WriteLine("Please, enter value");

        var value = Console.ReadLine();

        Console.WriteLine("Please, enter name");

        var name = Console.ReadLine();

        return new ReadDataViewModel
        {
            Name = name!,
            Value = int.Parse(value ?? throw new InvalidOperationException())
        };
    }

    private static AssignOneVariableValueToOtherViewModel ReadOneVariableValueToAssignForOther()
    {
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
        Console.WriteLine("Please, enter variable name, V");

        var value = Console.ReadLine();

        return new ReadValueToPrintViewModel
        {
            V = value!
        };
    }

    private static void ShowConstants(string? s = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");

        var response = new StringBuilder();

        foreach (var item in Constants.constants) response.AppendLine($"{item.Key} = {item.Value}");

        Console.WriteLine(response.ToString());
    }

    private static void ShowState(string? s = "")
    {
        Console.WriteLine($"threadId = {Environment.CurrentManagedThreadId}");

        var response = new StringBuilder();

        foreach (var item in State.state) response.AppendLine($"{item.Key} = {item.Value}");

        Console.WriteLine(response.ToString());
    }

    private static void AssignOneVariableValueToOther(string v1, string v2, ConcurrentDictionary<string, int> state)
    {
        state[v1] = state[v2];
    }

    private static void AssignConstantValueToOther(string v, string c,
        ConcurrentDictionary<string, int> state, Dictionary<string, int> constants)
    {
        state[v] = constants[c];
    }

    private static void Print(string variableName, ConcurrentDictionary<string, int> state)
    {
        Console.WriteLine($"{variableName} has value {state[variableName]}");
    }
}