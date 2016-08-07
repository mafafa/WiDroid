module ConnectionMessageQueueTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Net
open System.Net.Sockets

open TestCategories
open ConnectionMessageQueue
open ConnectionMessageQueue.Communication


[<TestClass>]
type ConnectionMessageQueueTests () =

    let cleanupTest (messageQueuesUsed:ConnectionMessageQueue list) (server:TcpListener) =
        let replies = List.map (fun (messageQueue:ConnectionMessageQueue) -> messageQueue.Stop () |> ignore) messageQueuesUsed        
        server.Stop ()

    let createServerAndStart port =
        let server = new TcpListener (IPAddress.Loopback, port)
        server.Start ()
        server

    let createClientAndConnect port =
        let client = new TcpClient()
        client.Connect(IPAddress.Loopback, port)
        client
    
    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        messageQueue.Start ()

        Async.Sleep 5000 |> Async.RunSynchronously

        cleanupTest [messageQueue] server

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    [<ExpectedException(typeof<InvalidOperationException>, "Test was able to start message queue twice.")>]
    member x.``Start Message Queue, then Start Again`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        
        messageQueue.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        messageQueue.Start ()

        cleanupTest [messageQueue] server

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(ConnectionMessageQueueTest)>]
    member x.``Start Message Queue, then Stop`` () =
        let server = createServerAndStart 44000
        let client = createClientAndConnect 44000

        let messageQueue = new ConnectionMessageQueue(client, IPAddress.Loopback, 44000)
        messageQueue.Start ()

        Async.Sleep 5000 |> Async.RunSynchronously

        let reply = messageQueue.Stop ()

        match reply with
        | ConnectionQueueReply.FinishedSuccessfully ->
            server.Stop ()
        | ConnectionQueueReply.Error message ->
            server.Stop ()
            Assert.Fail(sprintf "Server did not stop successfully: %s" message)