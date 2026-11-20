using static System.Console;

namespace CSharp15FeaturesDemo;

class Program
{
    public static void Main(string[] args)
    {
        WriteLine("C# 15.0 Features Demo");

        Unions();
        DefaultUnionValue();
        ClosedHierarchies();
        CollectionExpressionArguments();
        ExtensionIndexers();
        LabeledBreakAndContinue();
    }

    // 1. Union types: a closed set of unrelated case types, matched exhaustively
    static void Unions()
    {
        WriteLine("\n-- union --");

        Result ok = new Success(5);               // implicit conversion from each case
        Result error = new Failure("no balance");
        WriteLine(Describe(ok));                  // ok 5
        WriteLine(Describe(error));               // no balance

        IntOrString a = 5;                        // cases need no common base type
        IntOrString b = "hello";
        WriteLine($"{a.Value} / {b.Value}");      // 5 / hello

        Length total = new Meters(1);
        WriteLine(total.Add(new Feet(10)).InMeters); // 4.048
    }

    static string Describe(Result result) => result switch
    {
        Success s => $"ok {s.Value}",
        Failure f => f.Reason,                    // exhaustive: no _ arm needed
    };

    // A generated union is a struct: default(Pet) holds null
    static void DefaultUnionValue()
    {
        WriteLine("\n-- default of a union --");

        Pet[] cage = new Pet[1];
        WriteLine(PetName(cage[0]));              // empty
        WriteLine(PetName(new Dog("Rex")));       // Rex
    }

    static string PetName(Pet pet) => pet switch
    {
        Dog d => d.Name,
        Cat c => c.Name,
        null => "empty",                          // prefer null over _ to keep exhaustiveness
    };

    // 2. Closed hierarchies: direct subclasses only in this assembly
    static void ClosedHierarchies()
    {
        WriteLine("\n-- closed --");

        WriteLine(DescribeGate(new Open(50)));    // open at 50%
        WriteLine(DescribeGate(new Shut()));      // shut
    }

    static string DescribeGate(Gate gate) => gate switch
    {
        Shut => "shut",
        Open(var percent) => $"open at {percent}%", // no default arm
    };

    // 3. with(...) passes arguments to the collection's constructor
    static void CollectionExpressionArguments()
    {
        WriteLine("\n-- with(...) in collection expressions --");

        string[] values = ["one", "two"];
        List<string> names = [with(capacity: values.Length * 2), .. values];
        WriteLine(names.Capacity);                // 4

        HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
        WriteLine(set.Count);                     // 1
    }

    // 4. Extension indexers complete the extension members of C# 14
    static void ExtensionIndexers()
    {
        WriteLine("\n-- extension indexers --");

        IEnumerable<int> numbers = Enumerable.Range(1, 10);
        WriteLine(numbers[2]);                    // 3
    }

    // 5. break and continue with a label, without flags or goto
    static void LabeledBreakAndContinue()
    {
        WriteLine("\n-- labeled break/continue --");

        int[,] grid = { { 1, 0 }, { 2, 3 } };

        outer: for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                if (grid[row, col] == 0) continue outer; // next row
                if (grid[row, col] == 3) break outer;    // leave both loops
                WriteLine(grid[row, col]);               // 1, 2
            }
        }
    }
}

public record Success(int Value);
public record Failure(string Reason);
public union Result(Success, Failure);

public union IntOrString(int, string);

public record Meters(double Value);
public record Feet(double Value);

public union Length(Meters, Feet)
{
    public double InMeters => this switch
    {
        Meters m => m.Value,
        Feet f => f.Value * 0.3048,
        _ => throw new InvalidOperationException("No value"),
    };

    public Length Add(Length other) => new Meters(InMeters + other.InMeters);
}

public record Dog(string Name);
public record Cat(string Name);
public union Pet(Dog, Cat);

public closed record class Gate;
public record class Shut : Gate;
public record class Open(float Percent) : Gate;

public static class Sequences
{
    extension(IEnumerable<int> source)
    {
        public int this[int index] => source.ElementAt(index);
    }
}
