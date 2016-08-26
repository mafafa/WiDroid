module ConnectionMessageQueueTests

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Sockets
open System.Text

open NetworkingCommon
open ConnectionMessageQueue
open ConnectionMessageQueue.Communication
open TestBase
open TestCommon
open TestCommon.TestCategories


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

    let createFileForTest path (content:string) =
        try
            let fileStream = File.Create path
            let contentBytes = Encoding.ASCII.GetBytes content
            fileStream.Write(contentBytes, 0, contentBytes.Length)
            fileStream.Dispose ()
            x.CleanupActions.Add ((fun () -> 
                match File.Exists path with
                | true ->
                    File.Delete path
                | false ->
                    raise (new IOException(sprintf "File %s does not exist, couldn't delete it!!!" path))), (sprintf "Deleting File: %s" path))
            contentBytes
        with ex ->
            Assert.Fail (sprintf "Was not able to create file: %s" path)
            [||]
    

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

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Start Sync Message, then Stop`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000

        let filePath = "testFile.txt"
        let destFile = "receivedTestFile.txt" 
        let contentBytes = createFileForTest filePath "This is a test file. Please do not mooh in it!"

        try
            let reply = messageQueue.PostAndReply (StartSync [|(filePath, destFile)|])
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "There was an error sending file: %s" filePath)
            | _ ->
                let _ = createFileForTest destFile ""   // Create file that shall be overriden, just for cleanup purposes
                readStreamToFile androidClient
                
                let contentReceived = File.ReadAllBytes destFile
                Assert.IsTrue((contentReceived.Length = contentBytes.Length), sprintf "Did not receive all bytes for file: %s" filePath)
                arrayItemsAreEqual contentBytes contentReceived
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
            Assert.Fail "Attempt to stop the server failed because of timeout"

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, Start Sync Message, Multiple Files`` () =
        let server = createServerAndStart 44000
        
        let androidClient = createClientAndConnect 44000
        let computerClient = server.AcceptTcpClient ()

        let messageQueue = createMessageQueueAndStart computerClient 44000

        let filePath1 = "testFile1.txt"
        let destFile1 = "receivedTestFile1.txt" 
        let contentBytes1 = createFileForTest filePath1 "This is a test file. Please do not mooh in it!"
        let filePath2 = "testFile2.txt"
        let destFile2 = "receivedTestFile2.txt" 
        let contentBytes2 = createFileForTest filePath2 "Omg, there is a second file!"
        let filePath3 = "testFile3.txt"
        let destFile3 = "receivedTestFile3.txt" 
        let contentBytes3 = createFileForTest filePath3 "MUCH FILEZZZZ!"

        try
            let reply = messageQueue.PostAndReply (StartSync [|(filePath1, destFile1); (filePath2, destFile2); (filePath3, destFile3)|])
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "There was an error sending files")
            | _ ->
                Array.iter (fun filePath -> createFileForTest filePath "" |> ignore) [|destFile1; destFile2; destFile3|]   // Create file that shall be overriden, just for cleanup purposes
                Array.iter (fun _ -> readStreamToFile androidClient) [|destFile1; destFile2; destFile3|]
                
                let assertTest (srcFile, destFile, contentOfFileSent:byte[]) =
                    let contentReceived = File.ReadAllBytes destFile
                    Assert.IsTrue((contentReceived.Length = contentOfFileSent.Length), sprintf "Did not receive all bytes for file: %s" srcFile)
                    arrayItemsAreEqual contentOfFileSent contentReceived

                Array.iter assertTest [|(filePath1, destFile1, contentBytes1); (filePath2, destFile2, contentBytes2); (filePath3, destFile3, contentBytes3)|]
        with
        | :? TimeoutException as ex ->
            Assert.Fail (sprintf "Attempt to send files: %A timed out." [|(filePath1, destFile1); (filePath2, destFile2); (filePath3, destFile3)|])

        try
            let reply = messageQueue.Stop ()
            match reply with
            | Error errorInfo ->
                Assert.Fail (sprintf "The message queue did not stop successfully: %s" errorInfo.Message)
            | _ ->
                x.CleanupActions.RemoveAt (x.CleanupActions.Count - 7)
        with
        | :? TimeoutException as ex ->
            Assert.Fail "Attempt to stop the server failed because of timeout"