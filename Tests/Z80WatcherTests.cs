using System;
using System.Diagnostics;
using System.Text;
using Konamiman.Z80dotNet;
using Konamiman.ZTest;
using NUnit.Framework;

namespace Konamiman.ZTests.Tests
{
    public class Z80WatcherTests
    {
        private Z80Processor Z80 { get; set; }
        private Z80Watcher Sut { get; set; }

        private readonly byte[] helloWorld = 
        {
            0x21, 0x0C, 0x01,   //LD HL,data
            0x7E,   //LD A,(HL)
            0xB7,   //OR A
            0xC8,   //RET Z
            0xCD, 0xA2, 0x00,   //CALL 00A2h
            0x23,   //INC HL
            0x18, 0xF7, //JR F8h
            0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x21, 0x21, 0x00    //db "Hello!",0
        };

        [SetUp]
        public void Setup()
        {
            Z80 = new Z80Processor();
            Z80.AutoStopOnRetWithStackEmpty = true;
            Sut = new Z80Watcher(Z80);
        }

        [Test]
        public void Can_create_instances()
        {
            Assert.IsNotNull(Sut);
        }

        [Test]
        public void Hello_World()
        {
            Sut
                .BeforeExecutingAt(0x100)
                .Do(context => Debug.WriteLine("Let's go!"));

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ThenReturn();
            
            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Before_and_after_instruction_handles()
        {
            Sut
                .BeforeExecuting()
                .Do(context => Debug.WriteLine($"Before: {context.Address:X}, times: {context.TimesReached}"));

            Sut
                .AfterExecuting()
                .Do(context => Debug.WriteLine($"After:  {context.Address:X}, times: {context.TimesReached}"));

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.WriteLine($"--- 00A2: {context.TimesReached} times"))
                .Do(context => { if(context.TimesReached > 3) context.TimesReached = 0; })
                .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Stopping_execution()
        {
            Sut
                .AfterExecutingAt(0x103)
                .ThenStopExecution();

            Sut
                .BeforeExecuting()
                .Do(context => Debug.WriteLine($"Before: {context.Address:X}"));

            Sut
                .AfterExecuting()
                .Do(context => Debug.WriteLine($"After:  {context.Address:X}"));

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Changing_read_value_after()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Named("TalYCual")
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ThenReturn();

            Sut
                .AfterReadingMemory(context => context.Address >= 0x010C)
                .ThenReplaceObtainedValueWith(context => (byte)(context.Value + 1));

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Changing_read_value_before()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ThenReturn();

            Sut
                .BeforeReadingMemory(context => context.Address >= 0x010C)
                .SuppressMemoryAccessAndReturn(context => (byte)(context.Address & 0xFF));

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Monitoring_memory_reads()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .ThenReturn();

            Sut
                .AfterReadingMemory()
                .Do(context => Debug.WriteLine($"{context.Address:X} = {context.Value:X}"));

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Exception_in_matcher()
        {
            Sut
                .BeforeExecuting(context => {throw new Exception("Buh!!");})
                .Named("Buerh")
                .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;

            try
            {
                Z80.Continue();
            }
            catch(WatchExecutionException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.WatchName);
                Debug.WriteLine(ex.WhenExecutingMatcher);
            }
        }

        [Test]
        public void Exception_in_callback()
        {
            Sut
                .BeforeExecuting()
                .Named("Buerh")
                .Do(context => {throw new Exception("Buh!!");})
                .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;

            try
            {
                Z80.Continue();
            }
            catch(WatchExecutionException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.WatchName);
                Debug.WriteLine(ex.WhenExecutingMatcher);
            }

            Sut.VerifyAllExpectations();
        }

        [Test]
        public void Verifying_expectations()
        {
            Sut
                .BeforeExecutingAt(0x00A2)
                .Named("Buerh")
                .ThenReturn()
                .NotExpected();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;

            Z80.Continue();

            try
            {
                Sut.VerifyAllExpectations();
            }
            catch(ExpectationFailedException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.WatchName);
                Debug.WriteLine(ex.MinReachesRequired);
                Debug.WriteLine(ex.MaxReachesRequired);
            }

            Sut.ResetAllReachCounts();

            Sut.VerifyAllExpectations();
        }

        [Test]
        public void Extending_handles()
        {
            var handle = Sut
                .BeforeExecutingAt(0x00A2)
                .ThenReturn();

            handle.PrintAddress();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;

            Z80.Continue();
        }

        [Test]
        public void Extending_handles_2()
        {
            Sut
                .BeforeExecutingAt(0x00A2)
                .PrintAddress2()
                .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;

            Z80.Continue();
        }

        [Test]
        public void Using_symbols_dictionary()
        {
            Sut.SymbolsDictionary["CHPUT"] = 0x00A2;
            
            Sut
                .BeforeExecutingAt(0x100)
                .Do(context => Debug.WriteLine("Let's go!"));

            Sut
                .BeforeExecutingAt("CHPUT")
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ThenReturn();
            
            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Changing_read_value_using_symbols()
        {
            Sut.SymbolsDictionary.Add("DATA", 0x010C);

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ThenReturn();

            Sut
                .BeforeReadingMemory(context => context.Address >= context.Symbols["DATA"])
                .SuppressMemoryAccessAndReturn(context => (byte)(context.Address & 0xFF));

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }

        [Test]
        public void Context_extensions()
        {
            Sut
                .BeforeExecutingAt(0x100)
                .Do(context => Debug.WriteLine("Let's go!"));

            Sut
                .BeforeExecutingAt(0x00A2)
                .Do(context => context.DebugCharAsAcii())
                .ThenReturn();
            
            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }
    }
}
