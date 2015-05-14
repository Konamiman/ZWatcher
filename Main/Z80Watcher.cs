using System;
using System.Collections.Generic;
using System.Linq;
using Konamiman.Z80dotNet;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;
using Konamiman.ZTest.WatchHandles;

namespace Konamiman.ZTest
{
    public class Z80Watcher
    {
        private List<BeforeMemoryReadWatch> BeforeMemoryReadWatches { get; } = new List<BeforeMemoryReadWatch>();

        private List<AfterMemoryReadWatch> AfterMemoryReadWatches { get; } = new List<AfterMemoryReadWatch>();

        private List<BeforeMemoryWriteWatch> BeforeMemoryWriteWatches { get; } = new List<BeforeMemoryWriteWatch>();

        private List<AfterMemoryWriteWatch> AfterMemoryWriteWatches { get; } = new List<AfterMemoryWriteWatch>();

        private List<BeforeCodeExecutionWatch> BeforeCodeExecutionWatches { get; } = new List<BeforeCodeExecutionWatch>();

        private List<AfterCodeExecutionWatch> AfterCodeExecutionWatches { get; } = new List<AfterCodeExecutionWatch>();

        public Z80Watcher(IZ80Processor z80)
        {
            z80.BeforeInstructionExecution += Z80OnBeforeInstructionExecution;
            z80.AfterInstructionExecution += Z80OnAfterInstructionExecution;
            z80.MemoryAccess += Z80OnMemoryAccess;
        }

        #region Creation of execution handles

        public BeforeCodeExecutionWatchHandle BeforeExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new BeforeCodeExecutionWatchHandle(isMatch);
            BeforeCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        public BeforeCodeExecutionWatchHandle BeforeExecutingAt(ushort address)
        {
            return BeforeExecuting(context => context.Address == address);
        }

        public BeforeCodeExecutionWatchHandle BeforeExecuting()
        {
            return BeforeExecuting(context => true);
        }

        public AfterCodeExecutionWatchHandle AfterExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new AfterCodeExecutionWatchHandle(isMatch);
            AfterCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        public AfterCodeExecutionWatchHandle AfterExecutingAt(ushort address)
        {
            return AfterExecuting(context => context.Address == address);
        }

        public AfterCodeExecutionWatchHandle AfterExecuting()
        {
            return AfterExecuting(context => true);
        }

        #endregion

        #region Creation of memory access handles

        public BeforeMemoryReadWatchHandle BeforeReadingMemory(Func<BeforeMemoryReadContext, bool> isMatch)
        {
            var handle = new BeforeMemoryReadWatchHandle(isMatch);
            BeforeMemoryReadWatches.Add(handle.Watch);
            return handle;
        }

        public BeforeMemoryReadWatchHandle BeforeReadingMemory(ushort address)
        {
            return BeforeReadingMemory(context => context.Address == address);
        }

        public BeforeMemoryReadWatchHandle BeforeReadingMemory()
        {
            return BeforeReadingMemory(context => true);
        }

        public AfterMemoryReadWatchHandle AfterReadingMemory(Func<AfterMemoryReadContext, bool> isMatch)
        {
            var handle = new AfterMemoryReadWatchHandle(isMatch);
            AfterMemoryReadWatches.Add(handle.Watch);
            return handle;
        }

        public AfterMemoryReadWatchHandle AfterReadingMemory(ushort address)
        {
            return AfterReadingMemory(context => context.Address == address);
        }

        public AfterMemoryReadWatchHandle AfterReadingMemory()
        {
            return AfterReadingMemory(context => true);
        }

        public BeforeMemoryWriteWatchHandle BeforeWritingMemory(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            var handle = new BeforeMemoryWriteWatchHandle(isMatch);
            BeforeMemoryWriteWatches.Add(handle.Watch);
            return handle;
        }

        public BeforeMemoryWriteWatchHandle BeforeWritingMemory(ushort address)
        {
            return BeforeWritingMemory(context => context.Address == address);
        }

        public BeforeMemoryWriteWatchHandle BeforeWritingMemory()
        {
            return BeforeWritingMemory(context => true);
        }

        public AfterMemoryWriteWatchHandle AfterWritingMemory(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            var handle = new AfterMemoryWriteWatchHandle(isMatch);
            AfterMemoryWriteWatches.Add(handle.Watch);
            return handle;
        }

        public AfterMemoryWriteWatchHandle AfterWritingMemory(ushort address)
        {
            return AfterWritingMemory(context => context.Address == address);
        }

        public AfterMemoryWriteWatchHandle AfterWritingMemory()
        {
            return AfterWritingMemory(context => true);
        }

        #endregion
        
        #region Handling instruction execution

        private void Z80OnAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs e)
        {
            var z80 = (IZ80Processor)sender;
            var address = (ushort)(z80.Registers.PC - e.Opcode.Length);
            var context = new AfterCodeExecutionContext(z80, address, e.Opcode);
            
            var matched = AfterCodeExecutionWatches.Where(w => w.IsMatch(context));
            foreach(var match in matched) {
                foreach(var callback in match.Callbacks) {
                    callback(context);
                    if(context.MustStop) {
                        e.ExecutionStopper.Stop();
                        return;
                    }
                }
            }
        }

        private void Z80OnBeforeInstructionExecution(object sender, BeforeInstructionExecutionEventArgs e)
        {
            var z80 = (IZ80Processor)sender;
            var address = (ushort)(z80.Registers.PC - e.Opcode.Length);
            var context = new BeforeCodeExecutionContext(z80, address, e.Opcode);
            
            var matched = BeforeCodeExecutionWatches.Where(w => w.IsMatch(context));
            foreach(var match in matched) {
                foreach(var callback in match.Callbacks) {
                    callback(context);
                }
            }
        }

        #endregion

        #region Handling memory access

        private void Z80OnMemoryAccess(object sender, MemoryAccessEventArgs e)
        {
            var z80 = (IZ80Processor)sender;
            if(e.EventType == MemoryAccessEventType.AfterMemoryRead) {
                var context = new AfterMemoryReadContext(z80, e.Address, e.Value);
                ProcessMemoryAccess(context, AfterMemoryReadWatches);
                e.Value = context.Value;
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryRead) {
                var context = new BeforeMemoryReadContext(z80, e.Address);
                ProcessMemoryAccess(context, BeforeMemoryReadWatches);
                if(context.Value != null) {
                    e.Value = (byte)context.Value;
                    e.CancelMemoryAccess = true;
                }
            }
            else if(e.EventType == MemoryAccessEventType.AfterMemoryWrite) {
                var context = new AfterMemoryWriteContext(z80, e.Address, e.Value);
                ProcessMemoryAccess(context, AfterMemoryWriteWatches);
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryWrite) {
                var context = new BeforeMemoryWriteContext(z80, e.Address, e.Value);
                ProcessMemoryAccess(context, BeforeMemoryWriteWatches);
                if(context.Value == null)
                    e.CancelMemoryAccess = true;
                else 
                    e.Value = (byte)context.Value;
            }
        }

        private static void ProcessMemoryAccess<TWatch, TContext>(TContext context, IEnumerable<TWatch> watches)
            where TContext : MemoryAccessContext
            where TWatch : IWatch<TContext>
        {
            var matched = watches.Where(w => w.IsMatch(context));
            foreach(var match in matched) {
                foreach(var callback in match.Callbacks) {
                    callback(context);
                }
            }
        }

        #endregion
    }
}
