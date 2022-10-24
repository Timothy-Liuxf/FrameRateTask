# FrameRateTask

---

## Languages

[English](https://github.com/Timothy-LiuXuefeng/FrameRateTask/blob/master/README.md)  
[中文（简体）](https://github.com/Timothy-LiuXuefeng/FrameRateTask/blob/master/README.zh-CN.md)

## Introduction

`FrameRateTask`, a frame rate stabilizer, a task executor which executes tasks at a stable frame rate.  

This project intends to build an engine to support executing tasks which need to be executed repeatedly and frequently in a single thread, and the time interval between two execution needs to be accurately stable. And this engine can also provide the real-time frame rate. Up to now, this time interval should be no more than 1 second. It can be used to control the frame rate of a game, or communication through network, etc. 

The source code is in the project `FrameRateTask`, and the example of usage is in the project `Test`.

> This project is initially written for [THUAI4](https://github.com/eesast/THUAI4) to support some functions.

## Get This Package

To get this package, please enter the nuget package page: [https://www.nuget.org/packages/FrameRateTask/](https://www.nuget.org/packages/FrameRateTask/)

or use .NET CLI:  

```shell
$ dotnet add package FrameRateTask
```

## Author

Autor: Timothy-LiuXuefeng

Copyright (C) 2022 Timothy-LiuXuefeng

## LICENSE

[MIT license](./LICENSE.txt)

## Contributing to this repository

Please read [CONTRIBUTING](./CONTRIBUTING.md) carefully before contributing to this repository.

## Interfaces

### CSharp  

#### `class FrameRateTaskExecutor<TResult>`

`TResult`: The return value of the task.

+ Constructor:

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Func<bool> loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`  

    If the object is constructed by this constructor, when calling its the `Start` or `TryStart` method, the program behaves as below:  

    ```c#
    while (loopCondition && time <= maxTotalDuration)
    {
     if (!loopToDo()) break;
     /* Do something to delay until next frame comes. */
    }
    return finallyReturn;
    ```

    + `loopCondition`: The condition to judge if the loop will continue.
    + `loopToDo`: The loop body. If it returns false, jump out of the loop.
    + `timeInterval`: Time interval between two executing in milliseconds.
    + `finallyReturn`: Specify the last thing to do and the return value.
    + `maxTotalDuration`：The maximum time in total for this task in milliseconds, `long.MaxValue` for default.

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Action loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    The only thing different from the last constructor is that `loopToDo` has no return value so that you cannot jump out of the loop through `loopToDo`.

+ `public void Start()`

  Start executing the task. If the task has started, it will throw `Timothy.FrameRateTask.TaskStartedMoreThanOnceException`. Otherwise, it will execute the task and return after the task finishes. 

+ `public bool TryStart()`

  Try to start executing the task. If the task has started, it will return `false`. Otherwise, it will execute the task and return `true` after the task finishes.  

+ `public uint FrameRate { get; }`

  The real-time frame rate, initialized with the expected frame rate, and will be changed when the execution of the task.

+ `public bool Finished { get; }`

  Whether the task has finished.

+ `public bool HasExecuted { get; }`

  Whether the task has begun to execute.

+ `public TResult Result { get; }`

  The return value of the task. If the task has not finished, it will throw `Timothy.FrameRateTask.TaskNotFinishedException`.

+ `public bool AllowTimeExceed { get; init; }`

  Whether the engine allows time exceeding, `true` for default. See more details under `MaxTolerantTimeExceedCount`.

+ `public Action<bool> TimeExceedAction { init; }`

  It will be called when time exceeds. See more details under `MaxTolerantTimeExceedCount`.

+ `public ulong MaxTolerantTimeExceedCount { get; init; }`

  The maximum number of time exceeding in a series, `5` for default. Once time exceeds, if the number of time exceeding in a series is no more than `MaxTolerantTimeExceedCount`, `TimeExceedAction` will be called with argument `false`。Otherwise, if `AllowTimeExceed` is set `true`, `TimeExceedAction` will be called with argument `true`, otherwise, that is, `AllowTimeExceed` is set `false`, it will throw `Timothy.FrameRateTask.TimeExceedException`.

  Once more than `MaxTolerantTimeExceedCount`, it will automatically abandon unfinished loops, and reset the loop counter. The example `TemporaryCongestion` is a sample code of this.

