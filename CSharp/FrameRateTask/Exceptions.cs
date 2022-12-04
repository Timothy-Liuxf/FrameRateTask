/*

 * FrameRateTask

 * Copyright © 2022 Timothy Liu

 * MIT LICENSE

 * https://timothy-liuxf.github.io/FrameRateTask/

 */

using System;

namespace Timothy.FrameRateTask
{
    /// <summary>
    /// The exception that is thrown when the user gets the return value while the task has not yet finished.
    /// </summary>
    public class TaskNotFinishedException : InvalidOperationException
    {
        ///
        public override string Message => "The task has not yet finished!";
    }

    /// <summary>
    /// The exception that is thrown when the specified time interval is invalid.
    /// </summary>
    public class IllegalTimeIntervalException : ArgumentOutOfRangeException
    {
        ///
        public override string Message => "The time interval should be a positive number!";
    }

    /// <summary>
    /// The exception that is thrown when the task times out.
    /// </summary>
    public class TimeExceedException : TimeoutException
    {
        ///
        public override string Message => "The loop runs so slowly that it cannot complete the task in the given time!";
    }

    /// <summary>
    /// The exception that is thrown when the user tries to start a task that has already been started.
    /// </summary>
    public class TaskStartedMoreThanOnceException : InvalidOperationException
    {
        ///
        public override string Message => "The task has been started more than once!";
    }

}
