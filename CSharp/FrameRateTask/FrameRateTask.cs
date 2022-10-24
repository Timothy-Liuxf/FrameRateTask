/*

 * FrameRateTask

 * Copyright © 2022 Timothy Liu

 * MIT LICENSE

 * https://timothy-liuxf.github.io/FrameRateTask/

 */

using System;
using System.Threading;

namespace Timothy.FrameRateTask
{

    /// <summary>
    /// This class intends to perform a task that needs to be executed repeatedly at exact intervals.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value of the task.</typeparam>
    public class FrameRateTaskExecutor<TResult>
    {
        /// <summary>
        /// Gets the current actual frame rate.
        /// </summary>
        public uint FrameRate
        {
            get => (uint)Interlocked.CompareExchange(ref frameRate, 0, 0);
            private set => Interlocked.Exchange(ref frameRate, value);
        }
        private long frameRate;

        /// <summary>
        /// Gets whether the task has finished.
        /// </summary>
        /// <returns>
        /// true if the task has finished; otherwise, false.
        /// </returns>
        public bool Finished
        {
            get => Interlocked.CompareExchange(ref finished, 0, 0) != 0;
            set => Interlocked.Exchange(ref finished, value ? 1 : 0);
        }
        private int finished = 0;

        /// <summary>
        /// Gets whether the task has started
        /// </summary>
        /// <returns>
        /// true if the task has started; otherwise, false.
        /// </returns>
        public bool HasExecuted { get => Interlocked.CompareExchange(ref hasExecuted, 0, 0) != 0; }
        private int hasExecuted = 0;
        private bool TrySetExecute()
        {
            if (Interlocked.Exchange(ref hasExecuted, 1) != 0)
            {
                return false;
            }
            return true;
        }

        private TResult result;
        /// <summary>
        /// Gets the return value of the task.
        /// </summary>
        /// <exception cref="TaskNotFinishedException">
        /// This task has not yet finished.
        /// </exception>
        public TResult Result
        {
            get
            {
                if (!Finished) throw new TaskNotFinishedException();
                return result;
            }
            private set => result = value;
        }

        /// <summary>
        /// Gets or sets whether to allow timeout.
        /// </summary>
        /// <remarks>
        /// If it is set to false and the task fails to complete in the given time, the task will throw Timothy.FrameRateTask.TimeExceedException.
        /// The default value is true.
        /// </remarks>
        public bool AllowTimeExceed
        {
            get;
#if NET5_0_OR_GREATER
            init;
#else
            set;
#endif
        } = true;

        /// <summary>
        /// Sets the method to be called when the task execution times out.
        /// </summary>
        /// <remarks>
        /// Parameter bool: true if the timeout count is greater than MaxTolerantTimeExceedCount when called; otherwise, false.
        /// </remarks>
        public Action<bool> TimeExceedAction
        {
            private get;
#if NET5_0_OR_GREATER
            init;
#else
            set;
#endif
        } = callByExceed => { };

        /// <summary>
        /// Gets or sets the maximum number of consecutive timeouts.
        /// </summary>
        /// <remarks>
        /// The default value is 5.
        /// </remarks>
        public ulong MaxTolerantTimeExceedCount
        {
            get;
#if NET5_0_OR_GREATER
            init;
#else
            set;
#endif
        } = 5;

        /// <summary>
        /// Starts this task synchronously.
        /// </summary>
        /// <exception cref="TaskStartedMoreThanOnceException">
        /// The task has already started.
        /// </exception>
        public void Start()
        {
            if (!TryStart()) throw new TaskStartedMoreThanOnceException();
        }
        /// <summary>
        /// Tries to start this task synchronously.
        /// </summary>
        /// <returns>
        /// true if the task is started successfully; false if it has been started.
        /// </returns>
        public bool TryStart()
        {
            if (!TrySetExecute()) return false;
            loopFunc();
            return true;
        }

        private Action loopFunc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loopCondition">The judgment condition of the loop. Returns true if you want to continue the loop; otherwise, returns false.</param>
        /// <param name="loopToDo">The loop body. Returns false if you want to jump out of the loop; otherwise, returns true.</param>
        /// <param name="timeInterval">The time interval between two executions.</param>
        /// <param name="finallyReturn">The method used to set the return value, which will be called after the loop.</param>
        /// <param name="maxTotalDuration">The maximum time for which the loop will run.</param>
        public FrameRateTaskExecutor
            (
                Func<bool> loopCondition,
                Func<bool> loopToDo,
                long timeInterval,
                Func<TResult> finallyReturn,
                long maxTotalDuration = long.MaxValue
            )
        {

            if (timeInterval <= 0L && timeInterval > 1000L)
            {
                throw new IllegalTimeIntervalException();
            }
            FrameRate = (uint)(1000L / timeInterval);

            loopFunc = () =>
            {
                ulong timeExceedCount = 0UL;
                long lastLoopEndingTickCount, beginTickCount;

                var nextTime = (lastLoopEndingTickCount = beginTickCount = Environment.TickCount64) + timeInterval;
                var endTime = beginTickCount < long.MaxValue - maxTotalDuration ? beginTickCount + maxTotalDuration : long.MaxValue;

                uint loopCnt = 0;
                var nextCntTime = beginTickCount + 1000L;

                while (loopCondition() && nextTime <= endTime)
                {
                    if (!loopToDo()) break;

                    var nowTime = Environment.TickCount64;
                    if (nextTime >= nowTime)
                    {
                        timeExceedCount = 0UL;
                        Thread.Sleep((int)(nextTime - nowTime));
                    }
                    else
                    {
                        ++timeExceedCount;
                        if (timeExceedCount > MaxTolerantTimeExceedCount)
                        {
                            if (AllowTimeExceed)
                            {
                                TimeExceedAction(true);
                                timeExceedCount = 0UL;
                                nextTime = Environment.TickCount64;
                            }
                            else
                            {
                                throw new TimeExceedException();
                            }
                        }
                        else if (AllowTimeExceed) TimeExceedAction(false);
                    }

                    lastLoopEndingTickCount = nextTime;
                    nextTime += timeInterval;
                    ++loopCnt;
                    if (Environment.TickCount64 >= nextCntTime)
                    {
                        nextCntTime = Environment.TickCount64 + 1000L;
                        FrameRate = loopCnt;
                        loopCnt = 0;
                    }
                }

                result = finallyReturn();
                Interlocked.MemoryBarrierProcessWide();
                Finished = true;
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loopCondition">The judgment condition of the loop. Returns true if you want to continue the loop; otherwise, returns false.</param>
        /// <param name="loopToDo">The loop body.</param>
        /// <param name="timeInterval">The time interval between two executions.</param>
        /// <param name="finallyReturn">The method used to set the return value, which will be called after the loop.</param>
        /// <param name="maxTotalDuration">The maximum time for which the loop will run.</param>
        public FrameRateTaskExecutor
            (
                Func<bool> loopCondition,
                Action loopToDo,
                long timeInterval,
                Func<TResult> finallyReturn,
                long maxTotalDuration = long.MaxValue
            ) : this(loopCondition, () => { loopToDo(); return true; }, timeInterval, finallyReturn, maxTotalDuration) { }
    }
}
