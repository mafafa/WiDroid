module ConnectionMessageQueueTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Sockets
open System.Text

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
        printfn "Created server"
        server

    let createClientAndConnect port =
        let client = new TcpClient()
        client.Connect(IPAddress.Loopback, port)
        x.CleanupActions.Add ((fun () -> client.GetStream().Close (); client.Close ()), "Disposing of client resources")
        printfn "Created a client"
        client

    let createMessageQueueAndStart client port =
        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        messageQueue.Start ()
        x.CleanupActions.Add ((fun () -> 
            match messageQueue.Stop () with
            | FinishedSuccessfully ->
                ()
            | Error errorInfo ->
                raise errorInfo.Exception), "Disposing of message queue client resources")
        printfn "Created a message queue"
        messageQueue

    (*let createFileForTest path (content:string) =
        try
            let fileStream = File.Create path
            let contentBytes = Encoding.ASCII.GetBytes content
            fileStream.Write(contentBytes, 0, contentBytes.Length)
            fileStream.Dispose ()
            x.CleanupActions.Add ((fun () -> File.Delete path), (sprintf "Deleting File: %s" path))
            contentBytes
        with ex ->
            Assert.Fail (sprintf "Was not able to create file: %s" path)
            [||]

    let readStreamToFile (stream:NetworkStream) outputPath =
        let rec readStreamToFileLoop (fileStream:FileStream) buffer =
            let bytesRead = stream.Read(buffer, 0, buffer.Length)
            match bytesRead > 0 with
            | true ->
                fileStream.Write(buffer, 0, bytesRead)
                readStreamToFileLoop fileStream buffer
            | false ->
                ()
        
        let buffer = Array.zeroCreate 1024
        let fileStream = File.Open(outputPath, FileMode.Create)
        readStreamToFileLoop fileStream buffer
        fileStream.Close ()*)
    

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000

        Async.Sleep 5000 |> Async.RunSynchronously

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<InvalidOperationException>, "Test was able to start message queue twice.")>]
    member x.``Start Message Queue, then Start Again`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        messageQueue.Start ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, then Stop`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000
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
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000
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
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = new ConnectionMessageQueue(computerClient, IPAddress.Loopback, 44000)
        x.CleanupActions.Add ((fun () -> computerClient.GetStream().Close (); computerClient.Close ()), "Disposing of computer client resources")
        
        messageQueue.Stop () |> ignore

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<InvalidOperationException>, "Test was able to start message queue twice without an exception.")>]
    member x.``Start Message Queue, Stop, then Start Again`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        try
            let reply = messageQueue.Stop ()
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "The message queue did not stop successfully: %s" errorInfo.Message)
            | _ ->
                x.RemoveLastCleanupAction ()
        with
        | :? TimeoutException as ex ->
            Assert.Fail "Attempt to stop the server failed because of timeout" 

        messageQueue.Start ()

    (*[<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Dispose Client, then Stop`` () =
        ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Close Client Socket Connection, then Stop`` () =
        ()*)

    (*[<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Start Sync Message, then Stop`` () =
        let server = createServerAndStart 44000
        let queueClient = createClientAndConnect 44000
        let client = createClientAndConnect 44000

        let messageQueue = createMessageQueueAndStart client 44000

        let receivingClient = server.AcceptTcpClient ()
        x.CleanupActions.Add 
        
        let filePath = @"testFile.txt"
        let contentBytes = createFileForTest filePath "This is a test file. Please do not mooh in it!"

        try
            let reply = messageQueue.PostAndReply (StartSync [|filePath|])
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "There was an error sending file: %s" filePath)
            | _ ->
                let _ = createFileForTest @"receivedTestFile.txt" ""
                readStreamToFile (receivingClient.GetStream()) @"receivedTestFile.txt"
                
                let contentReceived = File.ReadAllBytes @"receivedTestFile.txt"
                Assert.IsTrue((contentReceived.Length = contentBytes.Length), sprintf "Did not receive all bytes for file: %s" filePath)
                Assert.AreEqual(contentBytes, contentReceived, sprintf "Did not receive same message. Message sent: %A ----- Message received: %A" contentBytes contentReceived)
        with
        | :? TimeoutException as ex ->
            Assert.Fail (sprintf "Attempt to send file: %s timed out." filePath)

        try
            let reply = messageQueue.Stop ()
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "The message queue did not stop successfully: %s" errorInfo.Message)
            | _ ->
                x.CleanupActions.RemoveAt (x.CleanupActions.Count - 3)
        with
        | :? TimeoutException as ex ->
            Assert.Fail "Attempt to stop the server failed because of timeout" *)

    (*[<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Start Sync Message, Files don't Exist`` () =
        ()*)