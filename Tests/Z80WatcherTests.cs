using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Konamiman.Z80dotNet;
using Konamiman.ZTest;
using NUnit.Framework;

namespace Konamiman.ZTests.Tests
{
    public class Z80WatcherTests
    {
        private Z80Processor Z80 { get; set; }
        private Z80Watcher Sut { get; set; }

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

        [Test]
        public void Can_stub_routines()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();
            
            AssembleAndExecute(helloWorldProgram);

            Assert.AreEqual(helloWorld, Encoding.ASCII.GetString(printedChars.ToArray()));
        }

        [Test]
        public void Can_act_before_executing_isntruction()
        {
            byte[] opcodeBytes = null;

            var program = @"
 ld a,34h
 ret";

            Sut
                .BeforeExecutingAt(ProgramAddress)
                .Do(context => opcodeBytes = context.Opcode);
            
            AssembleAndExecute(program);

            Assert.AreEqual(new byte[] { 0x3E, 0x34 }, opcodeBytes);
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

            Assert.AreEqual("Hello,", Encoding.ASCII.GetString(printedChars.ToArray()));
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
                .BeforeReadingMemory(context =>
                    context.Address >= context.Symbols["DATA"] &&
                    context.Address < context.Symbols["DATA"] + helloWorld.Length)
                .SuppressMemoryAccessAndReturn(Ascii('A'));

            AssembleAndExecute(helloWorldProgram);

            Assert.AreEqual(new string('A', helloWorld.Length), Encoding.ASCII.GetString(printedChars.ToArray()));
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
                    context.Address >= context.Symbols["DATA"] &&
                    context.Value != 0)
                .ReplaceObtainedValueWith(Ascii('A'));

            AssembleAndExecute(helloWorldProgram);

            Assert.AreEqual(new string('A', helloWorld.Length), Encoding.ASCII.GetString(printedChars.ToArray()));
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
                .BeforeWritingMemory(context =>
                    context.Address >= context.Symbols["DATA"])
                .SuppressWrite();

            AssembleAndExecute(writeMemoryProgram);

            Assert.AreEqual(new byte[] { 1,2,3,4 }, Z80.Memory.GetContents(Sut.Symbols["DATA"], 4));
        }

        [Test]
        public void Can_replace_value_written_to_memory()
        {
            Sut
                .BeforeWritingMemory(context =>
                    context.Address >= context.Symbols["DATA"])
                .ActuallyWrite(context => (byte)(context.Value + 1));

            AssembleAndExecute(writeMemoryProgram);

            Assert.AreEqual(new byte[] { 11,21,31,41 }, Z80.Memory.GetContents(Sut.Symbols["DATA"], 4));
        }

        [Test]
        public void Can_act_after_writing_to_memory()
        {
            var writenValues = new List<byte>();

            Sut
                .AfterWritingMemory(context =>
                    context.Address >= context.Symbols["DATA"])
                .Do(context => writenValues.Add((byte)context.Value));

            AssembleAndExecute(writeMemoryProgram);

            Assert.AreEqual(new byte[] { 10,20,30,40 }, writenValues);
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

            Assert.AreEqual(new byte[] { 1,2,3,4 }, Z80.Memory.GetContents(Sut.Symbols["DATA"], 4));
        }

        [Test]
        public void Can_replace_value_read_from_port_after_read()
        {
            Sut
                .AfterReadingPort(context => context.Address >= 10)
                .ReplaceObtainedValueWith(context => (byte)context.TimesReached);

            AssembleAndExecute(readPortsProgram);

            Assert.AreEqual(new byte[] { 1,2,3,4 }, Z80.Memory.GetContents(Sut.Symbols["DATA"], 4));
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

            Assert.AreEqual(new byte[] {1, 2, 0, 0}, Z80.PortsSpace.GetContents(10, 4));
        }

        [Test]
        public void Can_replace_values_written_to_port()
        {
            Sut
                .BeforeWritingPort(context => context.Address >= 10)
                .ActuallyWrite(context => (byte)(context.Value + 10));

            AssembleAndExecute(writePortsProgram);

            Assert.AreEqual(new byte[] {11, 12, 13, 14}, Z80.PortsSpace.GetContents(10, 4));
        }

        [Test]
        public void Can_act_after_writting_to_port()
        {
            var writtenValues = new List<byte>();

            Sut
                .AfterWritingPort(context => context.Address >= 10)
                .Do(context => writtenValues.Add((byte)context.Value));

            AssembleAndExecute(writePortsProgram);

            Assert.AreEqual(new byte[] {1, 2, 3, 4}, writtenValues);
        }

        [Test]
        public void Can_inspect_number_of_times_reached()
        {
            var printedChars = new List<byte>();

            Sut
                .AfterReadingMemory(context => context.Address >= context.Symbols["DATA"])
                .Do(context => {
                    if(context.TimesReached >= 8 && context.TimesReached <= 12)
                        context.Value = Ascii('*');
                });

            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);

            Assert.AreEqual("Hello, *****!", Encoding.ASCII.GetString(printedChars.ToArray()));
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

            Assert.AreEqual(100, exception.MinReachesRequired);
            Assert.AreEqual(200, exception.MaxReachesRequired);
            Assert.AreEqual(helloWorld.Length, exception.ActualReaches);
            Assert.AreEqual("BeforeInstructionFetch", exception.WatchName);
        }

        [Test]
        public void Can_name_watches_to_help_troubleshooting_problems()
        {
            Sut
                .BeforeFetchingInstructionAt("CHPUT")
                .ExpectedBetween(100, 200)
                .ExecuteRet()
                .Named("BeforeCHPUT");

            AssembleAndExecute(helloWorldProgram);

            var exception = Assert.Throws<ExpectationFailedException>(() => Sut.VerifyAllExpectations());

            Assert.AreEqual(100, exception.MinReachesRequired);
            Assert.AreEqual(200, exception.MaxReachesRequired);
            Assert.AreEqual(helloWorld.Length, exception.ActualReaches);
            Assert.AreEqual("BeforeCHPUT", exception.WatchName);
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

            Assert.IsTrue(exception.WhenExecutingMatcher);
            Assert.AreEqual(ProgramAddress, exception.Context.Address);
        }

        [Test]
        public void Exception_in_callback_cause_WatchExecutionException()
        {
            Sut
                .BeforeFetchingInstructionAt(ProgramAddress)
                .Do(context => { throw new InvalidOperationException("Invalid!!!"); });

            var exception = Assert.Throws<WatchExecutionException>(() => AssembleAndExecute(helloWorldProgram));

            Assert.IsFalse(exception.WhenExecutingMatcher);
            Assert.AreEqual(ProgramAddress, exception.Context.Address);
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

        /// <summary>
        /// Assembles the specified Z80 source, loads it at the specified address, and executes it.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="address"></param>
        private void AssembleAndExecute(string program, ushort address = ProgramAddress)
        {
            program = $" org 0{address:X}h\r\n{program}";

            File.WriteAllText("temp.asm", program);

            var startInfo = new ProcessStartInfo("sjasm.exe", "-s temp.asm") {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();
            proc.WaitForExit();
            
            if(proc.ExitCode != 0) {
                var output = proc.StandardOutput.ReadToEnd();
                output = RemoveFirstLine(output);
                output = Regex.Replace(output, @"temp\.asm\(([0-9]+)\) :", "Line $1 :");
                Assert.Fail($"Assembly process failed:\r\n\r\n{output}");
            }

            var symbolLines = File.ReadAllLines("temp.sym");
            foreach(var line in symbolLines) {
                var parts = line.Split(':');
                var symbol = parts[0];
                var hexValue = 
                    new string(parts[1].Replace("equ","").Where(c => char.IsDigit(c) || (c >= 'A' && c <= 'F')).ToArray());
                var value = Convert.ToUInt16(hexValue.TrimStart('0'), 16);
                Sut.Symbols[symbol] = value;
            }

            var bytes = File.ReadAllBytes("temp.out");
            Z80.Memory.SetContents(address, bytes);
            Z80.Reset();
            Z80.Registers.PC = address;
            Z80.Continue();
        }

        private static string RemoveFirstLine(string output)
        {
            return string.Join(
                Environment.NewLine, 
                output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray());
        }
    }
}
