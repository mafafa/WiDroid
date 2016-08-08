module ConnectionMessageQueueTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Net
open System.Net.Sockets

open TestCategories
open TestBase
open ConnectionMessageQueue
open ConnectionMessageQueue.Communication


[<TestClass>]
type ConnectionMessageQueueTests () as x =
    inherit TestBase ()

    let createServerAndStart port =
        let server = new TcpListener (IPAddress.Loopback, port)
        server.Start ()
        x.CleanupActions.Add ((fun () -> server.Stop ()), "Stopping Server Used")
        server

    let createClientAndConnect port =
        let client = new TcpClient()
        client.Connect(IPAddress.Loopback, port)
        client

    let createMessageQueueAndStart client port =
        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        messageQueue.Start ()
        x.CleanupActions.Add ((fun () -> 
            match messageQueue.Stop () with
            | FinishedSuccessfully ->
                ()
            | Error errorInfo ->
                raise errorInfo.Exception), "Disposing of client resources")
        messageQueue
    

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000

        Async.Sleep 5000 |> Async.RunSynchronously

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<InvalidOperationException>, "Test was able to start message queue twice.")>]
    member x.``Start Message Queue, then Start Again`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        messageQueue.Start ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, then Stop`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        try
            let reply = messageQueue.Stop ()
            match reply with
            | FinishedSuccessfully ->
                x.RemoveLastCleanupAction ()
            | Error queueError ->
                Assert.Fail (sprintf "The message queue did not stop successfully: %s" queueError.Message)
        with
        | :? TimeoutException as ex ->
            Assert.Fail "Attempt to stop the server failed because of timeout" 

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<TimeoutException>, "Test was able to stop message queue twice.")>]
    member x.``Start Message Queue, Stop, then Stop Again`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        try
            let reply1 = messageQueue.Stop ()
            match reply1 with
            | Error errorInfo ->
                Assert.Fail "Got reply1, but first attempt to stop the server failed" 
            | _ ->
                x.RemoveLastCleanupAction ()
        with
        | :? TimeoutException as ex ->
            Assert.Fail "First attempt to stop the server failed because of timeout" 
        
        let reply2 = messageQueue.Stop ()
        match reply2 with
        | FinishedSuccessfully ->
            Assert.Fail "Got reply2, but test was able to stop message queue twice"
        | _ ->
            ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<TimeoutException>, "Test was able to stop message queue without it being started.")>]
    member x.``Stop Message Queue Without Starting`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        
        messageQueue.Stop () |> ignore

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<InvalidOperationException>, "Test was able to start message queue twice without an exception.")>]
    member x.``Start Message Queue, Stop, then Start Again`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000
        x.RemoveLastCleanupAction ()
        Async.Sleep 5000 |> Async.RunSynchronously

        try
            let reply = messageQueue.Stop ()
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "The message queue did not stop successfully: %s" errorInfo.Message)
            | _ ->
                ()
        with
        | :? TimeoutException as ex ->
            Assert.Fail "Attempt to stop the server failed because of timeout" 

        messageQueue.Start ()