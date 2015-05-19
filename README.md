# ZWatcher #

## What is this? ##

ZWatcher is a small component that sits around [a simulated Z80 processor](https://bitbucket.org/konamiman/z80dotnet) and allows to create a set of *watchers*, each of which will awake when a certain condition is met and will then invoke one or more callbacks. Under the hood this is done by subscribing to the various code execution and memory access events provided by Z80.NET.

ZWatcher has been developed as a tool that allows developing unit tests for Z80 code, however it can be useful to develop emulators as well, as a higher level alternative to the aforementioned Z80.NET's code execution and memory access events.


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

            z80.Memory.SetContents(0, programBytes.Concat(messageBytes).ToArray());

            watcher
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => Console.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ExecuteRet()
                .ExpectedExactly(message.Length - 1);

            z80.Start();

            watcher.VerifyAllExpectations();
        }
    }
}
```

## How to use ##

First you create an instance of the `Z80Processor` class and configure it as appropriate (see the [Z80.NET](https://bitbucket.org/konamiman/z80dotnet)'s documentation). Then you create an instance of `Z80Watcher` passing the Z80 class as a constructor parameter. This class is the entry point for all the functionality provided by ZWatcher.

The `Z80Watcher` class contains a collection of methods for creating watches. There is one method for each type of watch, depending on what type of Z80 event is being wathed. Their names should be self-explanatory:

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

The return value for the watch creation methods is a *watch handle* that you can use to register callbacks for the watcher. A callback is a delegate that gets a same context object (the same instance, in fact, that is passed to the matching delegate) and returns nothing. You can register as many callbacks as you want for the same watcher, and each callback can do anything, from simply displaying debug information to modifying the internal Z80 state (tipically the memory or registers contents).

Callbacks are added primarily by using the `Do` method provided by the watcher, but there are too some helper methods that create callbacks in a more readable way. For example, the handle returned by `BeforeWritingMemory` has a `ActuallyWrite(newValue)` method that is equivalent to `Do(context => context.Value = newValue)`.

All the callbacks for a watcher are invoked in the order in which they are created and get the same instance of the context object.

TO DO...

## Last but not least...

...if you like this project **[please consider donating!](http://www.konamiman.com#donate)** My kids need moar shoes!

## But who am I? ##

I'm [Konamiman, the MSX freak](http://www.konamiman.com). No more, no less.