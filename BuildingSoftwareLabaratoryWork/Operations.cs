using System.Collections.Concurrent;
using BuildingSoftwareLabaratoryWork.Models;

namespace BuildingSoftwareLabaratoryWork;

public static class Operations
{
    
    public static readonly Dictionary<string, int> operations = new() 
        {{"Assign" ,1}, {"Compare", 2} , {"ReadNStore", 3}, {"PrintValue",4}, {"ShowState",5}, {"ShowConstants",6} };

    public static ReadDataViewModel ReadToAssign()
    {
        Console.WriteLine("Please, enter value");

        var value = Console.ReadLine();

        return new ReadDataViewModel
        {
            Name = $"{Guid.NewGuid().ToString()}-{DateTime.Now}",
            Value = Int32.Parse(value ?? throw new InvalidOperationException())
        };
    }

    public static AssignOneVariableValueToOtherViewModel ReadOneVariableValueToAssignForOther()
    {
        Console.WriteLine("Please, enter V1 and V2 with comma as separator");

        var values = Console.ReadLine()?.Split(",");

        return new AssignOneVariableValueToOtherViewModel
        {
            V1 = values![0],
            V2 = values[1]
        };
    }

    public static AssignConstantToVariableViewModel ReadConstantToAssignToVariable()
    {
        Console.WriteLine("Please, enter constant name C, and variable name V");

        var values = Console.ReadLine()?.Split(",");

        return new AssignConstantToVariableViewModel
        {
            C = values![0],
            V = values[1]
        };
    }

    public static ReadValueToPrintViewModel ReadValueToPrint()
    {
        Console.WriteLine("Please, enter variable name, V");

        var value = Console.ReadLine();

        return new ReadValueToPrintViewModel
        {
            V = value!
        };
    }
    
    public static void AssignValue(string variableName, int value, ConcurrentDictionary<string,int> state)
    {
        state[variableName] = value;
    }

    public static void AssignOneVariableValueToOther(string v1, string v2, ConcurrentDictionary<string,int> state)
    {
        state[v1] = state[v2];
    }
    
    public static void AssignConstantValueToOther(string v, string c, 
        ConcurrentDictionary<string,int> state, Dictionary<string, int> constants)
    {
        state[v] = constants[c];
    }

    public static  void Print(string variableName, ConcurrentDictionary<string, int> state) 
        => Console.WriteLine($"{variableName} has value {state[variableName]}");

    public static  int Compare(string variableName, int comparer, Dictionary<string, int> state)
    {
        return state[variableName].CompareTo(comparer);
    }
}