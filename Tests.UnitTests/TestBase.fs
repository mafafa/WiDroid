module TestBase

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic


[<Obsolete("Override the Initialize method instead of using [<TestIniatialize>]", true)>]
type TestInitializeAttribute () = 
    inherit Attribute ()

[<Obsolete("Add actions to CleanupActions property instead of using [<TestCleanup>]", true)>]
type TestCleanupAttribute () = 
    inherit Attribute ()

[<TestClass>]
type TestBase () =
    let cleanupActionList = new List<((unit -> unit) * string)>()

    abstract member CleanupActions : List<((unit -> unit) * string)>
    abstract member RemoveLastCleanupAction : unit -> unit
    abstract member Initialize : unit -> unit
    abstract member Cleanup : unit -> unit

    default x.CleanupActions = cleanupActionList

    default x.RemoveLastCleanupAction () =
        x.CleanupActions.RemoveAt (x.CleanupActions.Count - 1)
    
    [<Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute>]
    default x.Initialize () =
        printfn "============================== Test Initialize =============================="
        x.CleanupActions.Clear ()
        printfn ""
        printfn "============================== Test Method =============================="

    [<Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute>]
    default x.Cleanup () =
        printfn ""
        printfn "============================== Test Cleanup =============================="
        let rec tryCleanup (exceptions:Exception list) acc =
            if acc < x.CleanupActions.Count
            then
                let newExceptions = 
                    let action, description = x.CleanupActions.Item acc
                    try
                        do action ()
                        printfn "Successfully did Cleanup Action: %s" description
                        exceptions
                    with ex ->
                        printfn "Cleanup Action failed: %s" description
                        printfn "%s" (ex.ToString ())
                        List.append exceptions [ex]
                
                tryCleanup newExceptions (acc + 1)
            else
                exceptions

        x.CleanupActions.Reverse ()
        let exceptionList = tryCleanup list.Empty 0
        
        match List.length exceptionList with
        | 0 ->
            ()
        | 1 ->
            raise (List.head exceptionList)
        | _ ->
            raise (new AggregateException("Multiple failures in test cleanup", exceptionList))