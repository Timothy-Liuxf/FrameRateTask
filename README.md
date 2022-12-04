# FrameRateTask

---

## Languages

[English](https://github.com/Timothy-LiuXuefeng/FrameRateTask/blob/master/README.md)  
[中文（简体）](https://github.com/Timothy-LiuXuefeng/FrameRateTask/blob/master/README.zh-CN.md)

## Introduction

`FrameRateTask`, a frame rate stabilizer, a tool to perform tasks at a stable frame rate.

This project aims to build an engine that can execute a task repeatedly and at high frequency in a single thread with a precise and stable time interval between every two executions as required. Also, this engine can provide real-time frame rate. This engine can be used to control game frame rate, network communication frame rate, etc.

## Get This Package

To get this package, please enter the nuget package page: [https://www.nuget.org/packages/FrameRateTask/](https://www.nuget.org/packages/FrameRateTask/)

or use .NET CLI:

```shell
dotnet add package FrameRateTask
```

## Author

Autor: Timothy-LiuXuefeng

Copyright (C) 2022 Timothy-LiuXuefeng

## LICENSE

[MIT license](https://github.com/Timothy-Liuxf/FrameRateTask/blob/master/LICENSE.txt)

## Contributing to this repository

Please read [CONTRIBUTING](https://github.com/Timothy-Liuxf/FrameRateTask/blob/master/CONTRIBUTING.md) carefully before contributing to this repository.

## Interfaces

### CSharp

#### `class FrameRateTaskExecutor<TResult>`

`TResult`: The return value of the task.

+ Constructor:

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Func<bool> loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    If the object is constructed with this constructor, when the `Start` or `TryStart` method of the object is called, the procedure is equivalent to the following code:

    ```c#
    while (loopCondition && time <= maxTotalDuration)
    {
     if (!loopToDo()) break;
     /* Delay until the next frame arrives */
    }
    return finallyReturn;
    ```

    + `loopCondition`: The judgment condition of whether to continue the loop.
    + `loopToDo`: The loop body. If it returns false, jump out of the loop.
    + `timeInterval`: The time interval between two executions of the loop body in milliseconds.
    + `finallyReturn`: Specify the last code to be executed and the return value.
    + `maxTotalDuration`：The maximum time in milliseconds for the whole task, `long.MaxValue` by default.

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Action loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    The only difference with the previous constructor is that `loopToDo` has no return value so that you cannot jump out of the loop through `loopToDo`.

+ `public void Start()`

  Start the task. If the task has already started, a `Timothy.FrameRateTask.TaskStartedMoreThanOnceException` exception will be thrown. Otherwise, it will start the task and return when the task finishes.

+ `public bool TryStart()`

  Try to start the task. Returns `false` if the task has already started, otherwise it will start the task and return `true` when the task finishes.

+ `public uint FrameRate { get; }`

  The real-time frame rate of the loop body execution, initialized to the expected frame rate, and will be changed during the execution of the task.

+ `public bool Finished { get; }`

  Whether the task has finished.

+ `public bool HasExecuted { get; }`

  Whether the task has been started.

+ `public TResult Result { get; }`

  The return value of the task. A `Timothy.FrameRateTask.TaskNotFinishedException` exception will be thrown if the task has not finished.

+ `public bool AllowTimeExceed { get; init; }`

  Whether to allow the execution timeouts, `true` by default. See the description of `MaxTolerantTimeExceedCount` for more details.

+ `public Action<bool> TimeExceedAction { init; }`

  It will be called after the execution timeouts. See the description of `MaxTolerantTimeExceedCount` for more details.

+ `public ulong MaxTolerantTimeExceedCount { get; init; }`

  The maximum number of consecutive timeouts allowed, `5` by default. Once a loop has timed out, if the number of consecutive timeouts does not exceed `MaxTolerantTimeExceedCount`, `TimeExceedAction` will be called with argument `false`. Otherwise, if `AllowTimeExceed` is set to `false`, a `Timothy.FrameRateTask.TimeExceedException` exception will be thrown; if `AllowTimeExceed` is set to `true`, `TimeExceedAction` will be called with argument `true`, and the incomplete loop will be discarded and the loop count will be reset (see the example `TemporaryCongestion`)
