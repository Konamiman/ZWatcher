using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
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

        private const string helloWorld = "Hello, world!";

        private readonly string helloWorldProgram =
            $@"
CHPUT:  equ 00A2h

 ld sp,07000h
 ld hl,DATA
LOOP: ld a,(hl)
 or a
 ret z
 call CHPUT
 inc hl
 jr LOOP

DATA: db ""{helloWorld}"",0";

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
        public void Can_stub_routines()
        {
            var printedChars = new List<byte>();

            Sut
                .BeforeExecutingAt("CHPUT")
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet();
            
            AssembleAndExecute(helloWorldProgram);

            Assert.AreEqual(helloWorld, Encoding.ASCII.GetString(printedChars.ToArray()));
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
                .BeforeExecutingAt("CHPUT")
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
                .BeforeExecutingAt("CHPUT")
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

            Assert.AreEqual(new byte[] {1,2,3,4}, Z80.Memory.GetContents(Sut.SymbolsDictionary["DATA"], 4));
        }

        [Test]
        public void Can_replace_value_written_to_memory()
        {
            Sut
                .BeforeWritingMemory(context =>
                    context.Address >= context.Symbols["DATA"])
                .ActuallyWrite(context => (byte)(context.Value + 1));

            AssembleAndExecute(writeMemoryProgram);

            Assert.AreEqual(new byte[] {11,21,31,41}, Z80.Memory.GetContents(Sut.SymbolsDictionary["DATA"], 4));
        }

        [Test]
        public void Can_act_after_writing_to_memory()
        {
            var writenValues = new List<byte>();

            Sut
                .AfterWritingMemory(context =>
                    context.Address >= context.Symbols["DATA"])
                .Do(context => writenValues.Add(context.Value));

            AssembleAndExecute(writeMemoryProgram);

            Assert.AreEqual(new byte[] {10,20,30,40}, writenValues);
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

        //WIP...



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
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Stopping_execution()
        {
            Sut
                .AfterExecutingAt(0x103)
                .StopExecution();

            Sut
                .BeforeExecuting()
                .Do(context => Debug.WriteLine($"Before: {context.Address:X}"));

            Sut
                .AfterExecuting()
                .Do(context => Debug.WriteLine($"After:  {context.Address:X}"));

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Changing_read_value_after()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Named("TalYCual")
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ExecuteRet();

            Sut
                .AfterReadingMemory(context => context.Address >= 0x010C)
                .ReplaceObtainedValueWith(context => (byte)(context.Value + 1));

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Changing_read_value_before()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ExecuteRet();

            Sut
                .BeforeReadingMemory(context => context.Address >= 0x010C)
                .SuppressMemoryAccessAndReturn(context => (byte)(context.Address & 0xFF));

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Monitoring_memory_reads()
        {
            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .ExecuteRet();

            Sut
                .AfterReadingMemory()
                .Do(context => Debug.WriteLine($"{context.Address:X} = {context.Value:X}"));

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Exception_in_matcher()
        {
            Sut
                .BeforeExecuting(context => {throw new Exception("Buh!!");})
                .Named("Buerh")
                .ExecuteRet();

            try
            {
                AssembleAndExecute(helloWorldProgram);
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
                .ExecuteRet();

            try
            {
                AssembleAndExecute(helloWorldProgram);
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
                .ExecuteRet()
                .NotExpected();

            AssembleAndExecute(helloWorldProgram);

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
                .ExecuteRet();

            handle.PrintAddress();

            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Extending_handles_2()
        {
            Sut
                .BeforeExecutingAt(0x00A2)
                .PrintAddress2()
                .ExecuteRet();

            AssembleAndExecute(helloWorldProgram);
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
                .ExecuteRet();
            
            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Changing_read_value_using_symbols()
        {
            Sut.SymbolsDictionary.Add("DATA", 0x010C);

            Sut
                .BeforeExecuting(context => context.Address == 0x00A2)
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .ExecuteRet();

            Sut
                .BeforeReadingMemory(context => context.Address >= context.Symbols["DATA"])
                .SuppressMemoryAccessAndReturn(context => (byte)(context.Address & 0xFF));

            AssembleAndExecute(helloWorldProgram);
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
                .ExecuteRet();
            
            AssembleAndExecute(helloWorldProgram);
        }

        [Test]
        public void Character_print_loop()
        {
            var message = "Esto mola mucho!";
            var printedChars = new List<byte>();

            var program =@"
CHPUT:  equ 00A2h

 ld hl,DATA
LOOP: ld a,(hl)
 or a
 ret z
 call CHPUT
 inc hl
 jr LOOP

DATA: db ""{0}"",0";
            program = string.Format(program, message);

            Sut
                .BeforeExecutingAt("CHPUT")
                .Do(context => Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A})))
                .Do(context => printedChars.Add(context.Z80.Registers.A))
                .ExecuteRet()
                .ExpectedExactly(message.Length);
            
            AssembleAndExecute(program);

            Sut.VerifyAllExpectations();

            Assert.AreEqual(message, Encoding.ASCII.GetString(printedChars.ToArray()));
        }

        private void AssembleAndExecute(string program, ushort address = 0x0100)
        {
            program = $" org 0{address:X}h\r\n{program}";

            File.WriteAllText("temp.asm", program);
            var psi = new ProcessStartInfo("sjasm.exe", "-s temp.asm")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var proc = new Process();
            proc.StartInfo = psi;
            proc.Start();
            proc.WaitForExit();

            var output = proc.StandardOutput.ReadToEnd();
            Debug.WriteLine(output);

            if(proc.ExitCode != 0)
                throw new InvalidOperationException($"Assembler exited with code {proc.ExitCode}");

            var symbolLines = File.ReadAllLines("temp.sym");
            foreach(var line in symbolLines)
            {
                var parts = line.Split(':');
                var symbol = parts[0];
                var hexValue = 
                    new string(parts[1].Replace("equ","").Where(c => char.IsDigit(c) || (c >= 'A' && c <= 'F')).ToArray());
                var value = Convert.ToUInt16(hexValue.TrimStart('0'), 16);
                Sut.SymbolsDictionary[symbol] = value;
            }

            var bytes = File.ReadAllBytes("temp.out");
            Z80.Memory.SetContents(address, bytes);
            Z80.Reset();
            Z80.Registers.PC = address;
            Z80.Continue();
        }
    }
}
