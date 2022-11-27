namespace BuildingSoftwareLabaratoryWork.Common;

public static class TestOperations
{

    public static void GetInfoOfProceededPermutations(List<List<string>> schemasOperations, int k)
    {
        var columns = BuildColumns(schemasOperations);
        
        var permutationsForColumn = new List<List<string>>();

        var totalLengthOfOperationsCount = 0;

        var responseToContinue = "";

        for (var i = 0; i < columns.Count; i++)
        {

            if (responseToContinue!.Equals("Yes"))
            {
                break;
            }

            if (totalLengthOfOperationsCount >= k)
            {
                break;
            }
    
            var allPermutationsForCurrent = new List<string>();
    
            var shuffle = string.Join(",",columns[i].Split(",").Skip(0).Take(k - totalLengthOfOperationsCount));

            permutationsForColumn.Add(new List<string>());

            var maxCountOfElements = Math.Abs(totalLengthOfOperationsCount - k) == 1 
                ? columns[i].Split(",").Length : Factorial(columns[i].Split(",").Length);
    
            while (allPermutationsForCurrent.Count != maxCountOfElements)
            {
                while (allPermutationsForCurrent.Contains(shuffle))
                {
                    shuffle = string.Join(",",columns[i].Split(",")
                        .OrderBy(x => Guid.NewGuid()).Skip(0)
                        .Take(k - totalLengthOfOperationsCount));
                }
        
                allPermutationsForCurrent.Add(shuffle);
        
                var percent = Math.Round((decimal)(permutationsForColumn.Sum(l => l.Distinct().Count()) + allPermutationsForCurrent.Count)
                    / k * 100);
        
                Console.WriteLine($"The percent of done combinations = {percent}");
            }
    
            permutationsForColumn[i] = allPermutationsForCurrent;
    
            totalLengthOfOperationsCount += allPermutationsForCurrent.First().Split(",").Length;
            
            Console.WriteLine("Do you want to stop ? Yes or No");

            responseToContinue = Console.ReadLine();
        }
    }

    private static List<string> BuildColumns(List<List<string>> schemasOperations) // a,b,c,d; e,f
    {
        var columns = new List<string>();

        var index = 0;

        while (schemasOperations.Any(l => index < l.Count))
        {
            var tmp = "";
    
            schemasOperations.ForEach(l =>
            {
                if (index < l.Count) //tmp += l[index]
                {
                    tmp += $"{l[index]},";
                }
            });
    
            columns.Add(tmp.Trim(new []{','}));
    
            index++;
        }

        return columns.Where(c => !string.IsNullOrEmpty(c)).ToList();
    }
    private static int Factorial(int n)
    {
        if (n <= 1) return 1;

        return n * Factorial(n - 1);
    }
}