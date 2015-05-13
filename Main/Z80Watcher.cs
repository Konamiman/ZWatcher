using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest
{
    public class Z80Watcher
    {
        private List<MemoryAccessWatch> MemoryReadWatches { get; } = new List<MemoryAccessWatch>();

        private List<MemoryAccessWatch> MemoryWriteWatches { get; } = new List<MemoryAccessWatch>();

        private List<CodeExecutionWatch> BeforeCodeExecutionWatches { get; } = new List<CodeExecutionWatch>();

        private List<CodeExecutionWatch> AfterCodeExecutionWatches { get; } = new List<CodeExecutionWatch>();

        public Z80Watcher(IZ80Processor z80)
        {
            z80.BeforeInstructionExecution += Z80OnBeforeInstructionExecution;
            z80.AfterInstructionExecution += Z80OnAfterInstructionExecution;
            z80.MemoryAccess += Z80OnMemoryAccess;
        }

        public CodeExecutionWatchHandle BeforeExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new CodeExecutionWatchHandle(isMatch, isBeforeExecution: true);
            BeforeCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        public CodeExecutionWatchHandle BeforeExecutingAt(ushort address)
        {
            return BeforeExecuting(context => context.Address == address);
        }

        public CodeExecutionWatchHandle BeforeExecuting()
        {
            return BeforeExecuting(context => true);
        }

        public CodeExecutionWatchHandle AfterExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new CodeExecutionWatchHandle(isMatch, isBeforeExecution: false);
            AfterCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        public CodeExecutionWatchHandle AfterExecutingAt(ushort address)
        {
            return AfterExecuting(context => context.Address == address);
        }

        public CodeExecutionWatchHandle AfterExecuting()
        {
            return AfterExecuting(context => true);
        }

        private void Z80OnMemoryAccess(object sender, MemoryAccessEventArgs e)
        {
            var z80 = (IZ80Processor)sender;
            if(e.EventType == MemoryAccessEventType.AfterMemoryRead) {
                var context = new MemoryAccessContext(z80, e.Address, true) { Value = e.Value };
                ProcessMemoryAccess(context, MemoryReadWatches);
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryRead) {
                var context = new MemoryAccessContext(z80, e.Address, false) { Value = null };
                ProcessMemoryAccess(context, MemoryReadWatches);
            }
            else if(e.EventType == MemoryAccessEventType.AfterMemoryWrite) {
                var context = new MemoryAccessContext(z80, e.Address, true) { Value = e.Value };
                ProcessMemoryAccess(context, MemoryWriteWatches);
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryWrite) {
                var context = new MemoryAccessContext(z80, e.Address, false) { Value = e.Value };
                ProcessMemoryAccess(context, MemoryWriteWatches);
                if(context.Value == null)
                    e.CancelMemoryAccess = true;
                else 
                    e.Value = (byte)context.Value;
            }
        }

        private void ProcessMemoryAccess(MemoryAccessContext context, List<MemoryAccessWatch> watches)
        {
            var matched = watches.Where(w => w.IsMatch(context));
            foreach(var match in matched) {
                foreach(var callback in match.Callbacks) {
                    callback(context);
                }
            }
        }
        
        private void Z80OnAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs e)
        {
            Z80OnInstructionExecution(sender, e.Opcode, AfterCodeExecutionWatches, e.ExecutionStopper);
        }

        private void Z80OnInstructionExecution(object sender, byte[] opcode, IEnumerable<Watch<CodeExecutionContext>> watches, IExecutionStopper executionStopper)
        {
            var z80 = (IZ80Processor)sender;
            var address = (ushort)(z80.Registers.PC - opcode.Length);
            var context = new CodeExecutionContext(z80, address, opcode, instructionHasBeenExecuted: executionStopper != null);
            
            var matched = watches.Where(w => w.IsMatch(context));
            foreach(var match in matched) {
                foreach(var callback in match.Callbacks) {
                    callback(context);
                    if(context.MustStop && executionStopper != null) {
                        executionStopper.Stop();
                        return;
                    }
                }
            }
        }

        private void Z80OnBeforeInstructionExecution(object sender, BeforeInstructionExecutionEventArgs e)
        {
            Z80OnInstructionExecution(sender, e.Opcode, BeforeCodeExecutionWatches, null);
        }
    }
}
