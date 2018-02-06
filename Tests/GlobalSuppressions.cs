﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

#if DEBUG

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "MonoTests.System.Threading.Tasks.TaskContinuationChainLeakTester")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope = "member", Target = "MonoTests.System.Linq.Expressions.MemberClass.#OnTest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Collections.ObjectModel.ObservableCollectionTest+ObservableCollectionTestHelper.#DoubleEnterReentrant()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Threading.CancellationTokenSourceTest.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Threading.CancellationTokenSourceTest.#DisposeEx()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Threading.CountdownEventTests.#Dispose_Double()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Threading.ManualResetEventSlimTests.#Dispose_Double()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:No usar Dispose varias veces en objetos", Scope = "member", Target = "MonoTests.System.Threading.Tasks.TaskTests.#FromResult()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.ManualResetEventSlimTests.Wait_SetConcurrent")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.ContinueTaskTests.ContinueWithInvalidArguments")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.ContinueTaskTests.ContinueWithWithStart")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.ContinueTaskTests.LazyCancelationTest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.ContinueTaskTests.RunSynchronouslyOnContinuation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.ContinueWithInvalidArguments")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.ContinueWithWithStart")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.DoubleTimeoutedWaitTest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.LazyCancelationTest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.RunSynchronouslyArgumentChecks")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.RunSynchronouslyOnContinuation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.Start_NullArgument")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WaitAll_Zero")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WaitAny_Cancelled")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WaitAny_CancelledWithoutExecution")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WaitAny_Zero")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAll_Start")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAny_Cancelled")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAny_Faulted")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAny_Start")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAnyResult_Cancelled")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAnyResult_Faulted")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0022:Should dispose object", Justification = "<Pendiente>", Scope = "member", Target = "~M:MonoTests.System.Threading.Tasks.TaskTests.WhenAnyResult_Start")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CC0033:Dispose Fields Properly", Justification = "<Pendiente>", Scope = "member", Target = "~F:MonoTests.System.Threading.Tasks.TaskContinuationChainLeakTester._mre")]
#endif