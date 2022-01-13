﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Internal;
using Baracuda.Threading.Utils;
using UnityEngine;

namespace Baracuda.Threading
{
    public sealed partial class Dispatcher : IDisableCallback
    {
        #region --- [DISPATCH: COROUTINE] ---
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) 
                    throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                Current.StartCoroutine(enumerator);
            });
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, ExecutionCycle cycle)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                Current.StartCoroutine(enumerator);
            }, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, MonoBehaviour target)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                target.StartCoroutine(enumerator);
            });
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                target.StartCoroutine(enumerator);
            },cycle);
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [DISPATCH: COROUTINE ASYNC : AWAIT START] ---
        
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                try
                {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                    if (!Application.isPlaying)
                    {
                        Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                        return;
                    }
#endif
                    var result = Current.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, MonoBehaviour target)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                    if (!Application.isPlaying)
                    {
                        Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                        return;
                    }
#endif
                    var result = target.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                });
            }
            _queuedDefault = true;
            return tcs.Task;
        }
        

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                        if (!Application.isPlaying) 
                            throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                        var result = Current.StartCoroutine(enumerator);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception exception)
                    {
                        tcs.TrySetException(exception);
                    }
                });
            }
            _queuedDefault = true;

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, MonoBehaviour target, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();

            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
                    ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                    if (!Application.isPlaying) 
                        throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                    var result = target.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                });
            }
            
            _queuedDefault = true;
            return tcs.Task;
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
           Invoke(() =>
           {
               try
               {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                   if (!Application.isPlaying)
                   {
                       Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                       return;
                   }
#endif
                   var result = Current.StartCoroutine(enumerator);
                   tcs.TrySetResult(result);
               }
               catch (Exception exception)
               {
                   tcs.TrySetException(exception);
               }
           }, cycle);

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                var result = target.StartCoroutine(enumerator);
                tcs.TrySetResult(result);
                
            }, cycle);
            
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                    if (!Application.isPlaying) 
                        throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                    var result = Current.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);
            
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsyncAwaitStart(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                if (!Application.isPlaying) 
                    throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                var result = target.StartCoroutine(enumerator);
                tcs.TrySetResult(result);
            }, cycle);
            
            return tcs.Task;
        }
        

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH: COROUTINE ASYNC : AWAIT COMPLETION] ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that is executed as a <see cref="Coroutine"/>
        /// on the main thread and return a <see cref="Task"/>, that can be awaited and returns
        /// after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, CancellationToken.None, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }
        

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, MonoBehaviour target, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, CancellationToken.None, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }



        //--------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, ExecutionCycle cycle, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, CancellationToken.None, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, CancellationToken.None, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeAsyncAwaitCompletion(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INTERNAL COROUTINE] ---
        
        /// <summary>
        /// Start an internal coroutine with cancellation support and exception handling.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource tcs, CancellationToken ct, bool throwExceptions, MonoBehaviour target)
        {
            if (throwExceptions)
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetException, tcs.TrySetCompleted, ct);
            }
            else
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetResult, tcs.TrySetCompleted, ct);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource tcs, CancellationToken ct, bool throwExceptions)
        {
            if (throwExceptions)
            {
                Current.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetException, tcs.TrySetCompleted, ct, _current);
            }
            else
            {
                Current.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetResult, tcs.TrySetCompleted, ct, _current);
            }
        }
        
        
        #endregion
        
        #region --- [INTERFACE: DISABLE CALLBACK] ---
        
        public event Action onDisable;

        private void OnDisable()
        {
            onDisable?.Invoke();
            onDisable = null;
        }

        #endregion
        
        #region --- [OBSOLETE] ---
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator)
        {
            return InvokeAsyncAwaitStart(enumerator);
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, MonoBehaviour target)
        {
            return InvokeAsyncAwaitStart(enumerator, target);
        }
        

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, CancellationToken ct)
        {
            return InvokeAsyncAwaitStart(enumerator, ct);
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, MonoBehaviour target, CancellationToken ct)
        {
            return InvokeAsyncAwaitStart(enumerator, target, ct);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle)
        {
            return InvokeAsyncAwaitStart(enumerator, cycle);
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            return InvokeAsyncAwaitStart(enumerator, cycle, target);
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            return InvokeAsyncAwaitStart(enumerator, cycle, ct);
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        [Obsolete("Use InvokeAsyncAwaitStart or InvokeAsyncAwaitCompletion instead!")]
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct)
        {
            return InvokeAsyncAwaitStart(enumerator, cycle, target, ct);
        }
        
        #endregion
    }
}