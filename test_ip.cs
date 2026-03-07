using System;
using IronPython.Hosting;

class Program
{
    static void Main()
    {
        var engine = Python.CreateEngine();
        var scope = engine.CreateScope();
        
        string code = @"```python
# Fa dire ""ciao"" al tuo personaggio
Player.Chat(""ciao"")
```";
        try
        {
            engine.Execute(code, scope);
            Console.WriteLine("Code executed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
