using System.Collections.Concurrent;
using System.Text;
using BuildingSoftwareLabaratoryWork.Common;
using BuildingSoftwareLabaratoryWork.Models;

namespace BuildingSoftwareLabaratoryWork;

public static class Operations
{

    public static readonly Dictionary<string, Action> operations;
        // new() 
        // {{"Assign",() => {}}, {"CompareLess",2}, {"CompareEqual",3} , {"ReadNStore",4}, {"PrintValue",5}, {"ShowState",6}, 
        //     {"ShowConstants"} };

    static Operations()
    {
        var operationsName = new List<string>
        {
            "AssignValueToVariable", "AssignConstantValueToVariable" ,"CompareLess", "CompareEqual", "ReadNStore", "PrintValue",
            "ShowState", "ShowConstants"
        };
        
        operations = new Dictionary<string, Action>();

        foreach (var operationName in operationsName)
        {
            switch (operationName)
            {
                case "AssignValueToVariable":
                    operations.Add(operationName, () =>
                    {
                        var readData = ReadOneVariableValueToAssignForOther();
                        AssignOneVariableValueToOther(readData.V1, readData.V2, State.state);
                        //State.state[readData.V1] = State.state[readData.V2];
                    });
                    break;
                case "AssignConstantValueToVariable":
                    operations.Add(operationName, () =>
                    {
                        var readData = ReadConstantToAssignToVariable();
                        AssignConstantValueToOther(readData.V, readData.C, State.state, Constants.constants);
                        // State.state[readData.V] = Constants.constants[readData.C];
                    });
                    break;
                case "CompareLess":
                    break;
                case "CompareEqual":
                    break;
                case "ReadNStore":
                    operations.Add(operationName, () =>
                    {
                        var readData = ReadToAssign();
                        State.state[readData.Name] = readData.Value;
                    });
                    break;
                case "PrintValue":
                    operations.Add(operationName, () =>
                    {
                        var readData = ReadValueToPrint();
                        Print(readData.V, State.state);
                    });
                    break;
                case "ShowState":
                    operations.Add(operationName, ShowState);
                    break;
                case "ShowConstants":
                    operations.Add(operationName, ShowConstants);
                    break;
            }
        }
    }

    public static Action GetOperationByName(string operationName)
    {
        return operations[operationName];
    }
    public static  int Compare(string variableName, string constantName)
    {
        return State.state[variableName].CompareTo(State.state[constantName]);
    }
    
    private static ReadDataViewModel ReadToAssign()
    {
        Console.WriteLine("Please, enter value");

        var value = Console.ReadLine();

        return new ReadDataViewModel
        {
            Name = $"{Guid.NewGuid().ToString()}-{DateTime.Now}",
            Value = Int32.Parse(value ?? throw new InvalidOperationException())
        };
    }

    private static AssignOneVariableValueToOtherViewModel ReadOneVariableValueToAssignForOther()
    {
        Console.WriteLine("Please, enter V1 and V2 names with comma as separator");

        var values = Console.ReadLine()?.Split(",");

        return new AssignOneVariableValueToOtherViewModel
        {
            V1 = values![0],
            V2 = values[1]
        };
    }

    private static AssignConstantToVariableViewModel ReadConstantToAssignToVariable()
    {
        Console.WriteLine("Please, enter constant name C, and variable name V");

        var values = Console.ReadLine()?.Split(",");

        return new AssignConstantToVariableViewModel
        {
            C = values![0],
            V = values[1]
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

    private static void ShowConstants()
    {
        var response = new StringBuilder();

        foreach (var item in Constants.constants)
        {
            response.AppendLine($"{item.Key} = {item.Value}");
        }

        Console.WriteLine(response.ToString());
    }

    private static void ShowState()
    {
        var response = new StringBuilder();

        foreach (var item in State.state)
        {
            response.AppendLine($"{item.Key} = {item.Value}");
        }

        Console.WriteLine(response.ToString());
    }
    
    private static void AssignOneVariableValueToOther(string v1, string v2, ConcurrentDictionary<string,int> state)
    {
        state[v1] = state[v2];
    }

    private static void AssignConstantValueToOther(string v, string c, 
        ConcurrentDictionary<string,int> state, Dictionary<string, int> constants)
    {
        state[v] = constants[c];
    }

    private static  void Print(string variableName, ConcurrentDictionary<string, int> state) 
        => Console.WriteLine($"{variableName} has value {state[variableName]}");
    
}