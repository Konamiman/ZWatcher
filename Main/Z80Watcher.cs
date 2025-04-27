using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Konamiman.Z80dotNet;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;
using Konamiman.ZWatcher.WatchHandles;

namespace Konamiman.ZWatcher
{
    public class Z80Watcher : IDisposable
    {
        private List<BeforeMemoryReadWatch> BeforeMemoryReadWatches { get; } = new List<BeforeMemoryReadWatch>();

        private List<AfterMemoryReadWatch> AfterMemoryReadWatches { get; } = new List<AfterMemoryReadWatch>();

        private List<BeforeMemoryWriteWatch> BeforeMemoryWriteWatches { get; } = new List<BeforeMemoryWriteWatch>();

        private List<AfterMemoryWriteWatch> AfterMemoryWriteWatches { get; } = new List<AfterMemoryWriteWatch>();

        private List<BeforeMemoryReadWatch> BeforePortReadWatches { get; } = new List<BeforeMemoryReadWatch>();

        private List<AfterMemoryReadWatch> AfterPortReadWatches { get; } = new List<AfterMemoryReadWatch>();

        private List<BeforeMemoryWriteWatch> BeforePortWriteWatches { get; } = new List<BeforeMemoryWriteWatch>();

        private List<AfterMemoryWriteWatch> AfterPortWriteWatches { get; } = new List<AfterMemoryWriteWatch>();

        private List<BeforeInstructionFetchWatch> BeforeInstructionFetchWatches { get; } = new List<BeforeInstructionFetchWatch>();

        private List<BeforeCodeExecutionWatch> BeforeCodeExecutionWatches { get; } = new List<BeforeCodeExecutionWatch>();

        private List<AfterCodeExecutionWatch> AfterCodeExecutionWatches { get; } = new List<AfterCodeExecutionWatch>();

        public IDictionary<string, ushort> Symbols { get; } = new Dictionary<string, ushort>();

        private readonly IZ80Processor z80;

        private bool hasMemoryAccessWatches = false;
        private bool hasBeforeInstructionFetchWatches = false;
        private bool hasBeforeInstructionExecutionWatches = false;
        private bool hasAfterInstructionExecutionWatches = false;

        BeforeInstructionFetchContext beforeInstructionFetchContext = null;
        BeforeCodeExecutionContext beforeCodeExecutionContext = null;
        AfterCodeExecutionContext afterCodeExecutionContext = null;

        BeforeMemoryReadContext beforeMemoryReadContext = null;
        AfterMemoryReadContext afterMemoryReadContext = null;
        BeforeMemoryWriteContext beforeMemoryWriteContext = null;
        AfterMemoryWriteContext afterMemoryWriteContext = null;

        public Z80Watcher(IZ80Processor z80)
        {
            this.z80 = z80;
        }

        #region Verifying expectations

        /// <summary>
        /// Verifies that all the declared expectations for watch reaching times
        /// have been fulfilled.
        /// </summary>
        /// <exception cref="ExpectationFailedException">At least one watch was not reached the number of times expected.</exception>
        public void VerifyAllExpectations()
        {
            CheckDisposed();

            var watches = GetAllRegisteredWatches();
            foreach(var watch in watches)
                watch.VerifyRequiredReaches();
        }

        private IEnumerable<ITimesreachedAware> GetAllRegisteredWatches()
        {
            var watchesListProperties =
                GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(p => IsListOfVerifiables(p.PropertyType))
                    .ToArray();

            return watchesListProperties.SelectMany(list => (IEnumerable<ITimesreachedAware>)list.GetValue(this, null));
        }
        
        private static bool IsListOfVerifiables(Type type)
        {
            return (
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>) &&
                (typeof(ITimesreachedAware).IsAssignableFrom(type.GetGenericArguments()[0])));
        }

        /// <summary>
        /// Sets all reach counters for all the registered watches to zero.
        /// </summary>
        public void ResetAllReachCounts()
        {
            CheckDisposed();

            var watches = GetAllRegisteredWatches();
            foreach(var watch in watches)
                watch.TimesReached = 0;
        }
       
        #endregion

        #region Creation of execution handles
        
        /// <summary>
        /// Registers a watch to be evaluated before fetching an instruction.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeInstructionFetchWatchHandle BeforeFetchingInstruction(Func<BeforeInstructionFetchContext, bool> isMatch)
        {
            CheckDisposed();

            var handle = new BeforeInstructionFetchWatchHandle(isMatch);
            BeforeInstructionFetchWatches.Add(handle.Watch);
            if(!hasBeforeInstructionFetchWatches) {
                beforeInstructionFetchContext = new BeforeInstructionFetchContext(z80, 0, Symbols);
                z80.BeforeInstructionFetch += Z80OnBeforeInstructionFetch;
                hasBeforeInstructionFetchWatches = true;
            }

            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before fetching an instruction at a given address.
        /// </summary>
        /// <param name="address">Instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeInstructionFetchWatchHandle BeforeFetchingInstructionAt(ushort address)
        {
            CheckDisposed();

            return BeforeFetchingInstruction(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before fetching an instruction at a given address.
        /// </summary>
        /// <param name="address">Name in the symbols dictionary of the instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeInstructionFetchWatchHandle BeforeFetchingInstructionAt(string address)
        {
            CheckDisposed();

            return BeforeFetchingInstruction(context => context.Address == context.Symbols[address]);
        }

        /// <summary>
        /// Registers a watch to be evaluated before fetching any instruction.
        /// </summary>
        /// <returns></returns>
        public BeforeInstructionFetchWatchHandle BeforeFetchingInstruction()
        {
            CheckDisposed();

            return BeforeFetchingInstruction(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing an instruction.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            CheckDisposed();

            var handle = new BeforeCodeExecutionWatchHandle(isMatch);
            BeforeCodeExecutionWatches.Add(handle.Watch);

            if(!hasBeforeInstructionExecutionWatches) {
                beforeCodeExecutionContext = new BeforeCodeExecutionContext(z80, 0, null, Symbols);
                z80.BeforeInstructionExecution += Z80OnBeforeInstructionExecution;
                hasBeforeInstructionExecutionWatches = true;
            }

            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecutingAt(ushort address)
        {
            CheckDisposed();

            return BeforeExecuting(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Name in the symbols dictionary of the instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecutingAt(string address)
        {
            CheckDisposed();

            return BeforeExecuting(context => context.Address == context.Symbols[address]);
        }

        /// <summary>
        /// Registers a watch to be evaluated before executing any instruction.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle BeforeExecuting()
        {
            CheckDisposed();

            return BeforeExecuting(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing an instruction.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecuting(Func<CodeExecutionContext, bool> isMatch)
        {
            CheckDisposed();

            var handle = new AfterCodeExecutionWatchHandle(isMatch);
            AfterCodeExecutionWatches.Add(handle.Watch);

            if(!hasAfterInstructionExecutionWatches) {
                afterCodeExecutionContext = new AfterCodeExecutionContext(z80, 0, null, Symbols);
                z80.AfterInstructionExecution += Z80OnAfterInstructionExecution;
                hasAfterInstructionExecutionWatches = true;
            }

            return handle;
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Name in the symbols dictionary of the instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecutingAt(string address)
        {
            CheckDisposed();

            return AfterExecuting(context => context.Address == context.Symbols[address]);
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing an instruction at a given address.
        /// </summary>
        /// <param name="address">Instruction address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecutingAt(ushort address)
        {
            CheckDisposed();

            return AfterExecuting(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after executing any instruction.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle AfterExecuting()
        {
            CheckDisposed();

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
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
            var handle = new BeforeMemoryReadWatchHandle(isMatch);
            BeforeMemoryReadWatches.Add(handle.Watch);
            return handle;
        }

        private void MaybeAttachMemoryAccessEvent()
        {
            if(!hasMemoryAccessWatches) {
                beforeMemoryReadContext = new BeforeMemoryReadContext(z80, 0, Symbols);
                afterMemoryReadContext = new AfterMemoryReadContext(z80, 0, 0, Symbols);
                beforeMemoryWriteContext = new BeforeMemoryWriteContext(z80, 0, 0, Symbols);
                afterMemoryWriteContext = new AfterMemoryWriteContext(z80, 0, 0, Symbols);

                z80.MemoryAccess += Z80OnMemoryAccess;
                hasMemoryAccessWatches = true;
            }
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading from a given memory address.
        /// </summary>
        /// <param name="address">Memory address for which the generated watch will be a match.</param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingMemory(ushort address)
        {
            CheckDisposed();

            return BeforeReadingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading any memory address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingMemory()
        {
            CheckDisposed();

            return BeforeReadingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingMemory(Func<AfterMemoryReadContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return AfterReadingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading any memory address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingMemory()
        {
            CheckDisposed();

            return AfterReadingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingMemory(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return BeforeWritingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to any memory address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingMemory()
        {
            CheckDisposed();

            return BeforeWritingMemory(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to memory.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingMemory(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return AfterWritingMemory(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to any memory address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingMemory()
        {
            CheckDisposed();

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
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return BeforeReadingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before reading any port address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle BeforeReadingPort()
        {
            CheckDisposed();

            return BeforeReadingPort(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading from a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingPort(Func<AfterMemoryReadContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return AfterReadingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after reading any port address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle AfterReadingPort()
        {
            CheckDisposed();

            return AfterReadingPort(context => true);
        }

         /// <summary>
        /// Registers a watch to be evaluated before writing to a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingPort(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return BeforeWritingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated before writing to any port address.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle BeforeWritingPort()
        {
            CheckDisposed();

            return BeforeWritingPort(context => true);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to a port.
        /// </summary>
        /// <param name="isMatch">Delegate that decides if the current context is a match for the generated watch.</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingPort(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            CheckDisposed();

            MaybeAttachMemoryAccessEvent();
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
            CheckDisposed();

            return AfterWritingPort(context => context.Address == address);
        }

        /// <summary>
        /// Registers a watch to be evaluated after writing to any port address.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle AfterWritingPort()
        {
            CheckDisposed();

            return AfterWritingPort(context => true);
        }

        #endregion
        
        #region Handling instruction execution
        
        private void Z80OnBeforeInstructionFetch(object sender, BeforeInstructionFetchEventArgs e)
        {
            beforeInstructionFetchContext.Address = ((IZ80Processor)sender).Registers.PC;
            beforeInstructionFetchContext.TimesReached = 0;
            InvokeAllCallbacksOnMatchingWatches(BeforeInstructionFetchWatches, beforeInstructionFetchContext);
        }

        private static void InvokeAllCallbacksOnMatchingWatches<T>(IEnumerable<IWatch<T>> watches, T context)
            where T:IContext
        {
            IWatch<T> currentWatch = null;
            var executingMatchers = true;
            try {
                var matchingWatches = watches
                    .Where(w => { currentWatch = w; return w.IsMatch(context); })
                    .ToArray();
                executingMatchers = false;
                foreach(var watch in matchingWatches) {
                    currentWatch = watch;
                    watch.TimesReached++;
                    context.TimesReached = watch.TimesReached;
                    foreach(var callback in watch.Callbacks) {
                        callback(context);

                        watch.TimesReached = context.TimesReached;
                        var afterExecutionContext = context as AfterCodeExecutionContext;
                        if(afterExecutionContext?.MustStop == true)
                            return;
                    }
                }
            }
            catch(Exception ex) {
                var what = executingMatchers ? "the matching delegate" : "one of the callbacks";
                var message =
                    $"Unhandled exception when invoking {what} for the watch \"{currentWatch.DisplayName}\": {ex.Message}";
                throw new WatchExecutionException(message, currentWatch.DisplayName, executingMatchers, context, ex);
            }
        }

        private void Z80OnAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs e)
        {
            if(e.LocalUserState is null) {
                throw new InvalidOperationException($"After instruction executuon handles must be paired with at least one before instruction execution handle. Invoking {nameof(BeforeExecuting)}() in the watcher instance is enough. ");
            }

            var z80 = (IZ80Processor)sender;
            var address = (ushort)e.LocalUserState;
            afterCodeExecutionContext.Address = address;
            afterCodeExecutionContext.Opcode = e.Opcode;
            afterCodeExecutionContext.TimesReached = 0;
            afterCodeExecutionContext.MustStop = false;
            InvokeAllCallbacksOnMatchingWatches(AfterCodeExecutionWatches, afterCodeExecutionContext);
            if(afterCodeExecutionContext.MustStop)
                e.ExecutionStopper.Stop(false);
        }

        private void Z80OnBeforeInstructionExecution(object sender, BeforeInstructionExecutionEventArgs e)
        {
            var z80 = (IZ80Processor)sender;
            var address = (ushort)(z80.Registers.PC - e.Opcode.Length);
            e.LocalUserState = address;
            beforeCodeExecutionContext.Address = address;
            beforeCodeExecutionContext.Opcode = e.Opcode;
            beforeCodeExecutionContext.TimesReached = 0;

            InvokeAllCallbacksOnMatchingWatches(BeforeCodeExecutionWatches, beforeCodeExecutionContext);
        }

        #endregion

        #region Handling memory access

        private void Z80OnMemoryAccess(object sender, MemoryAccessEventArgs e)
        {
            var z80 = (IZ80Processor)sender;

            if(e.EventType == MemoryAccessEventType.AfterMemoryRead) {
                afterMemoryReadContext.Address = e.Address;
                afterMemoryReadContext.Value = e.Value;
                afterMemoryReadContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(AfterMemoryReadWatches, afterMemoryReadContext);
                e.Value = afterMemoryReadContext.Value;
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryRead) {
                beforeMemoryReadContext.Address = e.Address;
                beforeMemoryReadContext.Value = null;
                beforeMemoryReadContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(BeforeMemoryReadWatches, beforeMemoryReadContext);
                if(beforeMemoryReadContext.Value != null) {
                    e.Value = (byte)beforeMemoryReadContext.Value;
                    e.CancelMemoryAccess = true;
                }
            }
            else if(e.EventType == MemoryAccessEventType.AfterMemoryWrite) {
                var value = e.CancelMemoryAccess ? (byte?)null : e.Value;
                afterMemoryWriteContext.Address = e.Address;
                afterMemoryWriteContext.Value = value;
                afterMemoryWriteContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(AfterMemoryWriteWatches, afterMemoryWriteContext);
            }
            else if(e.EventType == MemoryAccessEventType.BeforeMemoryWrite) {
                beforeMemoryWriteContext.Address = e.Address;
                beforeMemoryWriteContext.Value = e.Value;
                beforeMemoryWriteContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(BeforeMemoryWriteWatches, beforeMemoryWriteContext);
                if(beforeMemoryWriteContext.Value == null)
                    e.CancelMemoryAccess = true;
                else
                    e.Value = (byte)beforeMemoryWriteContext.Value;
            }

            else if(e.EventType == MemoryAccessEventType.AfterPortRead) {
                afterMemoryReadContext.Address = e.Address;
                afterMemoryReadContext.Value = e.Value;
                afterMemoryReadContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(AfterPortReadWatches, afterMemoryReadContext);
                e.Value = afterMemoryReadContext.Value;
            }
            else if(e.EventType == MemoryAccessEventType.BeforePortRead) {
                beforeMemoryReadContext.Address = e.Address;
                beforeMemoryReadContext.Value = null;
                beforeMemoryReadContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(BeforePortReadWatches, beforeMemoryReadContext);
                if(beforeMemoryReadContext.Value != null) {
                    e.Value = (byte)beforeMemoryReadContext.Value;
                    e.CancelMemoryAccess = true;
                }
            }
            else if(e.EventType == MemoryAccessEventType.AfterPortWrite) {
                var value = e.CancelMemoryAccess ? (byte?)null : e.Value;
                afterMemoryWriteContext.Address = e.Address;
                afterMemoryWriteContext.Value = value;
                afterMemoryWriteContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(AfterPortWriteWatches, afterMemoryWriteContext);
            }
            else if(e.EventType == MemoryAccessEventType.BeforePortWrite) {
                beforeMemoryWriteContext.Address = e.Address;
                beforeMemoryWriteContext.Value = e.Value;
                beforeMemoryWriteContext.TimesReached = 0;
                InvokeAllCallbacksOnMatchingWatches(BeforePortWriteWatches, beforeMemoryWriteContext);
                if(beforeMemoryWriteContext.Value == null)
                    e.CancelMemoryAccess = true;
                else
                    e.Value = (byte)beforeMemoryWriteContext.Value;
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Remove all the existing watches, basically resetting the watcher to its initial state.
        /// </summary>
        public void RemoveAllWatches()
        {
            if(hasMemoryAccessWatches) {
                z80.MemoryAccess -= Z80OnMemoryAccess;
                hasMemoryAccessWatches = false;
                BeforeMemoryReadWatches.Clear();
                AfterMemoryReadWatches.Clear();
                BeforeMemoryWriteWatches.Clear();
                AfterMemoryWriteWatches.Clear();
                BeforePortReadWatches.Clear();
                AfterPortReadWatches.Clear();
                BeforePortWriteWatches.Clear();
                AfterPortWriteWatches.Clear();
            }

            if(hasBeforeInstructionFetchWatches) {
                z80.BeforeInstructionFetch -= Z80OnBeforeInstructionFetch;
                hasBeforeInstructionFetchWatches = false;
                BeforeInstructionFetchWatches.Clear();
            }

            if(hasBeforeInstructionExecutionWatches) {
                z80.BeforeInstructionExecution -= Z80OnBeforeInstructionExecution;
                hasBeforeInstructionExecutionWatches = false;
                BeforeCodeExecutionWatches.Clear();
            }

            if(hasAfterInstructionExecutionWatches) {
                z80.AfterInstructionExecution -= Z80OnAfterInstructionExecution;
                hasAfterInstructionExecutionWatches = false;
                AfterCodeExecutionWatches.Clear();
            }

            beforeInstructionFetchContext = null;
            beforeCodeExecutionContext = null;
            afterCodeExecutionContext = null;
            beforeMemoryReadContext = null;
            afterMemoryReadContext = null;
            beforeMemoryWriteContext = null;
            afterMemoryWriteContext = null;
        }

        private bool disposed = false;

        private void CheckDisposed()
        {
            if(disposed) {
                throw new ObjectDisposedException("This object has been disposed. No further operations are allowed.");
            }
        }

        /// <summary>
        /// Disposes the watcher. Any further operations on it will throw an exception.
        /// </summary>
        public void Dispose()
        {
            if(disposed) {
                return;
            }
            disposed = true;

            RemoveAllWatches();
        }

        #endregion
    }
}
