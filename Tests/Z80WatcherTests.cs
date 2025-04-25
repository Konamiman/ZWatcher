using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Konamiman.Nestor80.Assembler;
using Konamiman.Z80dotNet;
using Konamiman.ZWatcher.Contexts;
using NUnit.Framework;

namespace Konamiman.ZWatcher.Tests
{
    public class Z80WatcherTests
    {
        private Z80Processor Z80 { get; set; }
        private Z80Watcher Sut { get; set; }

        [SetUp]
        public void Setup()
        {
            Z80 = new Z80Processor {
                AutoStopOnRetWithStackEmpty = true
            };
            Sut = new Z80Watcher(Z80);
        }

        [TearDown]
        public void TearDown()
        {
            Sut.Dispose();
        }

        [Test]
        public void Can_create_instances()
        {
            Assert.That(Sut, Is.Not.Null);
        }

        private const int ProgramAddress = 0x100;

        private const string helloWorld = "Hello, world!";
        private readonly string helloWorldProgram =
            $@"
CHPUT:  equ 00A2h

 ld hl,DATA
LOOP: ld a,(hl)
 or a
 ret z
 call CHPUT
 inc hl
 jr LOOP

DATA: db ""{helloWorld}"",0";

        private bool IsDataByte(IContext context)
        {
            return context.Address >= context.Symbols["DATA"] && context.Address < (context.Symbols["DATA"] + helloWorld.Length);
        }

        [Test]
        public void Can_stub_routines()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);

            Assert.That(Encoding.ASCII.GetString(printedChars.ToArray()), Is.EqualTo(helloWorld));
        }

        [Test]
        public void Can_act_before_executing_instruction()
        {
            byte[] opcodeBytes = null;

            var program = @"
         ld a,34h
         ret";

            Sut
                .BeforeExecutingAt(ProgramAddress)
                .Do(context => opcodeBytes = context.Opcode);

            AssembleAndExecute(program);

            Assert.That(opcodeBytes, Is.EqualTo(new byte[] { 0x3E, 0x34 }));
        }

        [Test]
        public void Can_stop_execution()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeExecutingAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            Sut
                .AfterExecuting(context =>
                    context.Address == context.Symbols["CHPUT"] &&
                    context.Z80.Registers.A == Ascii(','))
                .StopExecution();

            AssembleAndExecute(helloWorldProgram);

            Assert.That(Encoding.ASCII.GetString(printedChars.ToArray()), Is.EqualTo("Hello,"));
        }
        
        private byte Ascii(char theChar)
        {
            return Encoding.ASCII.GetBytes(new[] {theChar})[0];
        }

        [Test]
        public void Can_replace_value_read_from_memory_before_read()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            Sut
                .BeforeReadingMemory(IsDataByte)
                .SuppressMemoryAccessAndReturn(Ascii('A'));

            AssembleAndExecute(helloWorldProgram);

            Assert.That(Encoding.ASCII.GetString(printedChars.ToArray()), Is.EqualTo(new string('A', helloWorld.Length)));
        }

        [Test]
        public void Can_replace_value_read_from_memory_after_read()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            Sut
                .AfterReadingMemory(context =>
                    IsDataByte(context) &&
                    context.Value != 0)
                .ReplaceObtainedValueWith(Ascii('A'));

            AssembleAndExecute(helloWorldProgram);

            Assert.That(Encoding.ASCII.GetString(printedChars.ToArray()), Is.EqualTo(new string('A', helloWorld.Length)));
        }

        private string writeMemoryProgram =
            $@"
 ld ix,DATA
 ld (ix),10
 ld (ix+1),20
 ld (ix+2),30
 ld (ix+3),40
 ret
DATA:   db 1,2,3,4
";

        [Test]
        public void Can_suppress_memory_write()
        {
            Sut
                .BeforeWritingMemory(IsDataByte)
                .SuppressWrite();

            AssembleAndExecute(writeMemoryProgram);

            Assert.That(Z80.Memory.GetContents(Sut.Symbols["DATA"], 4), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void Can_replace_value_written_to_memory()
        {
            Sut
                .BeforeWritingMemory(IsDataByte)
                .ActuallyWrite(context => (byte)(context.Value + 1));

            AssembleAndExecute(writeMemoryProgram);

            Assert.That(Z80.Memory.GetContents(Sut.Symbols["DATA"], 4), Is.EqualTo(new byte[] { 11, 21, 31, 41 }));
        }

        [Test]
        public void Can_act_after_writing_to_memory()
        {
            var writenValues = new List<byte>();

            Sut
                .AfterWritingMemory(IsDataByte)
                .Do(context => writenValues.Add((byte)context.Value));

            AssembleAndExecute(writeMemoryProgram);

            Assert.That(writenValues, Is.EqualTo(new byte[] { 10, 20, 30, 40 }));
        }

        private string readPortsProgram =
            $@"
 ld ix,DATA
 in a,(10)
 ld (ix),a
 in a,(11)
 ld (ix+1),a
 in a,(12)
 ld (ix+2),a
 in a,(13)
 ld (ix+3),a
 ret
DATA: db 0,0,0,0
 ret
";

        [Test]
        public void Can_replace_value_read_from_port_before_read()
        {
            Sut
                .BeforeReadingPort(context => context.Address >= 10)
                .SuppressMemoryAccessAndReturn(context => (byte)context.TimesReached);

            AssembleAndExecute(readPortsProgram);

            Assert.That(Z80.Memory.GetContents(Sut.Symbols["DATA"], 4), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void Can_replace_value_read_from_port_after_read()
        {
            Sut
                .AfterReadingPort(context => context.Address >= 10)
                .ReplaceObtainedValueWith(context => (byte)context.TimesReached);

            AssembleAndExecute(readPortsProgram);

            Assert.That(Z80.Memory.GetContents(Sut.Symbols["DATA"], 4), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        }
        
        private string writePortsProgram =
            $@"
 ld a,1
 out (10),a
 inc a
 out (11),a
 inc a
 out (12),a
 inc a
 out (13),a
 ret
";

        [Test]
        public void Can_suppress_port_write()
        {
            Sut
                .BeforeWritingPort(context => context.Address >= 12)
                .SuppressWrite();

            AssembleAndExecute(writePortsProgram);

            Assert.That(Z80.PortsSpace.GetContents(10, 4), Is.EqualTo(new byte[] { 1, 2, 0, 0 }));
        }

        [Test]
        public void Can_replace_values_written_to_port()
        {
            Sut
                .BeforeWritingPort(context => context.Address >= 10)
                .ActuallyWrite(context => (byte)(context.Value + 10));

            AssembleAndExecute(writePortsProgram);

            Assert.That(Z80.PortsSpace.GetContents(10, 4), Is.EqualTo(new byte[] { 11, 12, 13, 14 }));
        }

        [Test]
        public void Can_act_after_writting_to_port()
        {
            var writtenValues = new List<byte>();

            Sut
                .AfterWritingPort(context => context.Address >= 10)
                .Do(context => writtenValues.Add((byte)context.Value));

            AssembleAndExecute(writePortsProgram);

            Assert.That(writtenValues, Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void Can_inspect_number_of_times_reached()
        {
            var printedChars = new List<byte>();
            
            Sut
                .AfterReadingMemory(IsDataByte)
                .Do(context => {
                    if(context.TimesReached >= 8 && context.TimesReached <= 12)
                        context.Value = Ascii('*');
                });

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            //Sut.BeforeExecutingAt("CHPUT").ExecuteRet();

            //Z80.Memory[0xA2] = 0xC9;
            AssembleAndExecute(helloWorldProgram);

            Assert.That(Encoding.ASCII.GetString(printedChars.ToArray()), Is.EqualTo("Hello, *****!"));
        }

        [Test]
        public void Can_set_and_verify_expectations()
        {
            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .ExpectedExactly(helloWorld.Length)
                .ExecuteRet();

            Sut
                .BeforeFetchingInstructionAt("LOOP")
                .ExpectedAtLeast(1);

            Sut
                .BeforeFetchingInstructionAt(0xFFFF)
                .NotExpected();

            AssembleAndExecute(helloWorldProgram);

            Sut.VerifyAllExpectations();
        }

        [Test]
        public void Unmet_expectations_cause_ExpectationFailedException()
        {
            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .ExpectedBetween(100, 200)
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);

            var exception = Assert.Throws<ExpectationFailedException>(() => Sut.VerifyAllExpectations());

            Assert.That(exception.MinReachesRequired, Is.EqualTo(100));
            Assert.That(exception.MaxReachesRequired, Is.EqualTo(200));
            Assert.That(exception.ActualReaches, Is.EqualTo(helloWorld.Length));
            Assert.That(exception.WatchName, Is.EqualTo("BeforeInstructionFetch"));
        }

        [Test]
        public void Can_name_watches_to_help_troubleshooting_problems()
        {
            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .NotExpected()
                .ExecuteRet()
                .Named("BeforeCHPUT");

            AssembleAndExecute(helloWorldProgram);

            var exception = Assert.Throws<ExpectationFailedException>(() => Sut.VerifyAllExpectations());

            Assert.That(exception.WatchName, Is.EqualTo("BeforeCHPUT"));
        }

        [Test]
        public void Exception_in_watch_matcher_cause_WatchExecutionException()
        {
            Sut
                .BeforeFetchingInstruction(context => {
                    if(context.Address == ProgramAddress)
                        throw new InvalidOperationException("Invalid!!!");
                    return false;
                });

            var exception = Assert.Throws<WatchExecutionException>(() => AssembleAndExecute(helloWorldProgram));

            Assert.That(exception.WhenExecutingMatcher, Is.True);
            Assert.That(exception.Context.Address, Is.EqualTo(ProgramAddress));
        }

        [Test]
        public void Exception_in_callback_cause_WatchExecutionException()
        {
            Sut
                .BeforeFetchingInstructionAt(ProgramAddress)
                .Do(context => { throw new InvalidOperationException("Invalid!!!"); });

            var exception = Assert.Throws<WatchExecutionException>(() => AssembleAndExecute(helloWorldProgram));

            Assert.That(exception.WhenExecutingMatcher, Is.False);
            Assert.That(exception.Context.Address, Is.EqualTo(ProgramAddress));
        }

        [Test]
        public void Can_define_custom_callbacks_with_extension_methods()
        {
            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .ExecuteRet();

            Sut
                .BeforeExecuting()
                .PrintOpcode()
                .PrintA();

            AssembleAndExecute(helloWorldProgram);
        }
        [Test]
        public void RemoveAllWatches_removes_watches()
        {
            var printedChars = new List<byte>();
            var memoryReads = 0;

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            Sut
                .BeforeReadingMemory()
                .Do(context => memoryReads++);

            Sut.RemoveAllWatches();

            AssembleAndExecute(helloWorldProgram);

            Assert.That(printedChars, Is.Empty);
            Assert.That(memoryReads, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_removes_watches()
        {
            var printedChars = new List<byte>();
            var memoryReads = 0;

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            Sut
                .BeforeReadingMemory()
                .Do(context => memoryReads++);

            Sut.Dispose();

            AssembleAndExecute(helloWorldProgram);

            Assert.That(printedChars, Is.Empty);
            Assert.That(memoryReads, Is.EqualTo(0));
        }

        [Test]
        public void After_Dispose_all_methods_throw()
        {
            Sut.Dispose();
            Sut.Dispose(); //Dispose() can be executed multiple times
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeFetchingInstruction());
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeFetchingInstructionAt("CHPUT"));
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeExecuting());
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeExecutingAt(0xFFFF));
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeReadingMemory());
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeWritingMemory());
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeReadingPort());
            Assert.Throws<ObjectDisposedException>(() => Sut.BeforeWritingPort());
            Assert.Throws<ObjectDisposedException>(() => Sut.VerifyAllExpectations());
            Assert.Throws<ObjectDisposedException>(() => Sut.ResetAllReachCounts());
        }

        /// <summary>
        /// Assembles the specified Z80 source, loads it at the specified address, and executes it.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="address"></param>
        private void AssembleAndExecute(string program, ushort address = ProgramAddress)
        {
            program = $" org 0{address:X}h\r\n{program}";

            var assemblyConfig = new AssemblyConfiguration() {
                BuildType = BuildType.Absolute
            };
            var assemblyResult = AssemblySourceProcessor.Assemble(program, assemblyConfig);
            if(assemblyResult.HasErrors) {
                Assert.Fail(
                    "Assembly errrors!\r\n" +
                    string.Join(
                        "\r\n", 
                        assemblyResult.Errors.Select(e => $"{e.LineNumber-1}: {e.Message}").ToArray()));
            }

            var outputStream = new MemoryStream();
            OutputGenerator.GenerateAbsolute(assemblyResult, outputStream);

            foreach(var symbol in assemblyResult.Symbols) {
                Sut.Symbols[symbol.Name] = symbol.Value;
            }

            Z80.Memory.SetContents(address, outputStream.ToArray());
            Z80.Reset();
            Z80.Registers.PC = address;
            Z80.Continue();
        }
    }
}
