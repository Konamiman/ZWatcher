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
        public void Stopping_execution()
        {
            Sut
                .AfterExecutingAt(0x103)
                .Do(context => context.RequestExecutionStop());

            //Throws:
            //Sut
            //   .BeforeExecutingAt(0x103)
            //   .Do(context => context.RequestExecutionStop());

            Sut
                .BeforeExecuting()
                .Do(context => Debug.WriteLine($"Before: {context.Address:X}"));

            Sut
                .AfterExecuting()
                .Do(context => Debug.WriteLine($"After:  {context.Address:X}"));

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .ThenReturn();

            //Throws:
            //Sut
            //   .AfterExecuting(context => context.Address == 0x00A2)
            //   .ThenReturn();

            Z80.Memory.SetContents(0x0100, helloWorld);

            Z80.Reset();
            Z80.Registers.PC = 0x0100;
            Z80.Continue();
        }
    }
}
