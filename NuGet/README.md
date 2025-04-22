## ZWatcher

ZWatcher is a small component that sits around a simulated Z80 processor and allows to create a set of watchers, each of which will awake when a certain condition is met and will then invoke one or more callbacks. Under the hood this is done by subscribing to the various code execution and memory access events provided by [Z80.NET](https://github.com/Konamiman/Z80dotNet).

ZWatcher has been developed as a tool to help in the development of unit tests for Z80 code, however it can be useful to develop emulators as well, as a higher level alternative to the aforementioned Z80.NET's code execution and memory access events.


## Hello, world!

```C#
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

## How to use

For the full details see [the project README in GitHub](https://github.com/Konamiman/ZWatcher/blob/master/README.md).
