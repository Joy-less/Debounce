# Debounce

A thread-safe mechanism that ensures an operation is not active more than once at a time.

Whereas a lock waits for an operation to become available, a debounce throws an exception
or returns a value to indicate that the operation is not available.

## Usage

```cs
using ExtendedThreading;

public class ExampleClass {
    private readonly Debounce ExampleDebounce = new();

    public async Task SayHelloAsync() {
        using (ExampleDebounce.Activate()) {
            Console.WriteLine("Hello");

            await Task.Delay(TimeSpan.FromSeconds(1.0));
        }
    }
}
```
