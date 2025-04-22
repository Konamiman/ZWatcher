# ZWatcher #

## What is this? ##

ZWatcher is a small component that sits around a simulated Z80 processor ([Z80.NET](https://github.com/Konamiman/Z80dotNet)) and allows to create a set of *watchers*, each of which will awake when a certain condition is met and will then invoke one or more callbacks; under the hood this is done by subscribing to the various code execution and memory access events provided by Z80.NET. It is written in C# and since version 1.1 it targets [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0), so it can be used with .NET Framework 4.6.1+ and .NET/.NET Core 2.0+.

ZWatcher has been developed as a tool to help in the development of unit tests for Z80 code, however it can be useful to develop emulators as well, as a higher level alternative to the aforementioned Z80.NET's code execution and memory access events.

ZWatcher is available [as a NuGet package](https://www.nuget.org/packages/ZWatcher) as well. Installing the ZWatcher package will install also Z80.NET, on which it depends.

## Hello, world! ##

```
#!csharp
using System;
using System.Linq;
using System.Text;
using Konamiman.Z80dotNet;
using Konamiman.ZWatcher;

namespace ZWatcherHelloWorld
{
    class Program
    {
        static void Main()
        {
            var z80 = new Z80Processor();
            z80.AutoStopOnRetWithStackEmpty = true;
            var watcher  = new Z80Watcher(z80);

            watcher.Symbols["CHPUT"] = 0x00A2;

            var programBytes = new byte[]
            {
                0x21, 0x0C, 0x00,  //LD HL,data
                0x7E,              //LOOP: LD A,(HL)
                0xB7,              //OR A
                0xC8,              //RET Z
                0xCD, 0xA2, 0x00,  //CALL CHPUT
                0x23,              //INC HL
                0x18, 0xF7         //JR LOOP
				                   //data: db ...
            };
            var message = "Hello, world!\0";
            var messageBytes = Encoding.ASCII.GetBytes(message);

            z80.Memory.SetContents(
			    0, programBytes.Concat(messageBytes).ToArray());

            watcher
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => Console.Write(
				    Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ExecuteRet()
                .ExpectedExactly(message.Length - 1);

            z80.Start();

            watcher.VerifyAllExpectations();
        }
    }
}
```

## How to use ##

First you create an instance of the `Z80Processor` class and configure it as appropriate (see the [Z80.NET](https://github.com/Konamiman/Z80dotNet)'s documentation). Then you create an instance of `Z80Watcher` passing the Z80 class as a constructor parameter. This class is the entry point for all the functionality provided by ZWatcher.

The `Z80Watcher` class contains a collection of methods for creating watches. There is one method for each type of watch, depending on what type of Z80 event is being watched. The method names should be self-explanatory:

* `BeforeFetchingInstruction`
* `BeforeExecuting`
* `AfterExecuting`
* `BeforeReadingMemory`
* `AfterReadingMemory`
* `BeforeWritingMemory`
* `AfterWritingMemory`
* `BeforeReadingPort`
* `AfterReadingPort`
* `BeforeWritingPort`
* `AfterWritingPort`

These methods accept a *matcher* in the constructor. A matcher is a delegate that gets a *context* object and returns true if the desired (being watched) condition is fulfilled, or false otherwise. The context object contains the current Z80 memory address, the instance of the Z80 processor being watched, the current instruction opcode bytes (for code execution watches) and the  value involved in the read/write operation (for memory and port access watches).

The return value for the watch creation methods is a *watch handle* that you can use to register *callbacks* for the watcher. A callback is a delegate that gets a context object (the same instance, actually, that is passed to the matching delegate) and returns nothing. You can register as many callbacks as you want for the same watcher, and each callback can do anything, from simply displaying debug information to modifying the internal Z80 state (tipically the memory or registers contents).

Callbacks are added primarily by using the `Do` method provided by the handler, but there are also some helper methods that create callbacks in a more readable way. For example, the handle returned by `BeforeWritingMemory` has a `ActuallyWrite(newValue)` method that is equivalent to `Do(context => context.Value = newValue)`.

All the public methods of the watch handlers return a reference to the handler itself, so that it is possible to chain several callback creations in a fluent interface. This is shown in the "Hello, world" example above; the same version without using the fluent interface would be as follows:

```
#!csharp
var handler = watcher.BeforeFetchingInstructionAt("CHPUT");
handler.Do(context => Console.Write(
    Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})));
handler.ExecuteRet();
handler.ExpectedExactly(message.Length - 1);
```

### Watch evaluation and execution under the hood ###

The following pseudocode shows the procedure followed by ZWatcher when a code execution or memory access event is fired by the watched `Z80Processor` class:

```
#!csharp
var context = new Context(address, Z80, others)
var matchingWatches = watchesForThisEvent.Where(watch => watch.IsMatch(context))
foreach(var watch in matchingWatches)
    watch.TimesReached++
    foreach(callback in watch.Callbacks)
	    callback(context)
```

As you can see, all the watches and its callbacks share a common context object. It is also worth noting that watches are evaluated in the order in which they are declared, and the same applies for the callbacks for each watch.


## Defining and verifying expectations ##

Each declared watch contains an internal counter of how many times the watch condition has been fulfilled (and thus its callbacks executed). This counter is included in the context, so that the matching delegate and the callbacks can check it and act depending on its value. However a much more powerful option is to define *expectations* for the number of times a watcher is reached, that can be verified all at once after the Z80 code execution has finished. (A watcher is considered to be 'reached' when its matching delegate is executed and it returns true)

You define an expectation by using one of the "Expected" methods exposed by the watch handles. There are several versions of the method available, depending on the type of expectation:

* `ExpectedBetween(minTimes, maxTimes)`
* `ExpectedAtLeast(minTimes)`
* `ExpectedExactly(exactTimes)`
* `Expected` - Equivalent to `ExpectedAtLeast(1)`
* `NotExpected` - Equivalent to `ExpectedExactly(0)`

The `ZWatcher` class exposes a `VerifyAllExpectations` method that loops through all watches comparing the number of times reached against the defined expectation for each watch. When it finds one that has been reached too few or too many times, it throws an `ExpectationFailedException`. If you are using a unit testing framework, this will cause the test to fail.

`ZWatcher` exposes also a `ResetAllReachCounts` method that will reset all the internal times reached counts to zero.


## Naming watches ##

ZWatcher will throw an exception when an expectation fails (`ExpectationFailedException`) and when one of the matching delegates or one of the callbacks throws an exception (`WatchExecutionException`). These exceptions will contain the name of the watch in the error message and in a property named `WatchName`.

By default, the name of a watch reflects its type, such as `AfterCodeExecution` or `BeforeReadingMemory`, but you can set a more readable name by using the `Named` method exposed by the watch handle. This is a good idea especially if you define more than one handle of the same type.


## Learning more ##

From here the best way to continue learning about the power of ZWatcher is to take a look at [the unit tests in the source code repository](Tests/Z80WatcherTests.cs). There you will also see a nice example of how to combine [Nestor80](https://github.com/Konamiman/Nestor80), Z80.NET and ZWatcher to test Z80 source code.


## Last but not least... ##

...if you like this project **[please consider donating!](http://www.konamiman.com/msx/msx-e.html#donate)** My kids need moar shoes!


## But who am I? ##

I'm [Konamiman, the MSX freak](http://www.konamiman.com). No more, no less.