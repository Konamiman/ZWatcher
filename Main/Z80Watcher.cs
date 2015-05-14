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

        private List<BeforeMemoryReadWatch> BeforePortReadWatches { get; } = new List<BeforeMemoryReadWatch>();

        private List<AfterMemoryReadWatch> AfterPortReadWatches { get; } = new List<AfterMemoryReadWatch>();

        private List<BeforeMemoryWriteWatch> BeforePortWriteWatches { get; } = new List<BeforeMemoryWriteWatch>();

        private List<AfterMemoryWriteWatch> AfterPortWriteWatches { get; } = new List<AfterMemoryWriteWatch>();

        private List<BeforeCodeExecutionWatch> BeforeCodeExecutionWatches { get; } = new List<BeforeCodeExecutionWatch>();

        private List<AfterCodeExecutionWatch> AfterCodeExecutionWatches { get; } = new List<AfterCodeExecutionWatch>();

        public Z80Watcher(IZ80Processor z80)
        {
            z80.BeforeInstructionExecution += Z80OnBeforeInstructionExecution;
            z80.AfterInstructionExecution += Z80OnAfterInstructionExecution;
            z80.MemoryAccess += Z80OnMemoryAccess;
        }

        #region Creation of execution handles

        /// <summary>
        /// Registers a watch to be evaluated before executing an instruction.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new BeforeCodeExecutionWatchHandle(isMatch);
            BeforeCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecutingAt(ushort address)
        {
            return BeforeExecuting(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing any instruction.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecuting()
        {
            return BeforeExecuting(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing an instruction.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            var handle = new AfterCodeExecutionWatchHandle(isMatch);
            AfterCodeExecutionWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecutingAt(ushort address)
        {
            return AfterExecuting(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing any instruction.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecuting()
        {
            return AfterExecuting(context => true);
        }

        #endregion

        #region Creation of memory access handles

        /// <summary>
        /// Registers a watch to be evaluated before reading from memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingMemory(Func<BeforeMemoryReadContext, bool> isMatch)
        {
            var handle = new BeforeMemoryReadWatchHandle(isMatch);
            BeforeMemoryReadWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading from a given memory address.
        /// </summary>
        /// <param name="address">Memory address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingMemory(ushort address)
        {
            return BeforeReadingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading any memory address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingMemory()
        {
            return BeforeReadingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingMemory(Func<AfterMemoryReadContext, bool> isMatch)
        {
            var handle = new AfterMemoryReadWatchHandle(isMatch);
            AfterMemoryReadWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from a given memory address.
        /// </summary>
        /// <param name="address">Memory address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingMemory(ushort address)
        {
            return AfterReadingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading any memory address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingMemory()
        {
            return AfterReadingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingMemory(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            var handle = new BeforeMemoryWriteWatchHandle(isMatch);
            BeforeMemoryWriteWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to a given memory address.
        /// </summary>
        /// <param name="address">Memory address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingMemory(ushort address)
        {
            return BeforeWritingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to any memory address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingMemory()
        {
            return BeforeWritingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingMemory(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            var handle = new AfterMemoryWriteWatchHandle(isMatch);
            AfterMemoryWriteWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to a given memory address.
        /// </summary>
        /// <param name="address">Memory address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingMemory(ushort address)
        {
            return AfterWritingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to any memory address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingMemory()
        {
            return AfterWritingMemory(context => true);
        }

        #endregion
        
        #region Creation of port access handles

        /// <summary>
        /// Registers a watch to be evaluated before reading from a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingPort(Func<BeforeMemoryReadContext, bool> isMatch)
        {
            var handle = new BeforeMemoryReadWatchHandle(isMatch);
            BeforePortReadWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading from a given port address.
        /// </summary>
        /// <param name="address">Port address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingPort(ushort address)
        {
            return BeforeReadingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading any port address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingPort()
        {
            return BeforeReadingPort(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingPort(Func<AfterMemoryReadContext, bool> isMatch)
        {
            var handle = new AfterMemoryReadWatchHandle(isMatch);
            AfterPortReadWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from a given port address.
        /// </summary>
        /// <param name="address">Port address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingPort(ushort address)
        {
            return AfterReadingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading any port address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingPort()
        {
            return AfterReadingPort(context => true);
        }

         /// <summary>
        /// Registers a watch to be evaluated before writing to a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingPort(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            var handle = new BeforeMemoryWriteWatchHandle(isMatch);
            BeforePortWriteWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to a given port address.
        /// </summary>
        /// <param name="address">Port address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingPort(byte address)
        {
            return BeforeWritingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to any port address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingPort()
        {
            return BeforeWritingPort(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingPort(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            var handle = new AfterMemoryWriteWatchHandle(isMatch);
            AfterPortWriteWatches.Add(handle.Watch);
            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to a given port address.
        /// </summary>
        /// <param name="address">Port address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingPort(byte address)
        {
            return AfterWritingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to any port address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingPort()
        {
            return AfterWritingPort(context => true);
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

            else if(e.EventType == MemoryAccessEventType.AfterMemoryRead) {
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
