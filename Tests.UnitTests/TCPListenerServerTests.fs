module TCPListenerServerTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Net
open System.Net.Sockets

open TestCategories
open TCPListenerServer


[<TestClass>]
type TCPListenerServerTests () =

    let cleanupTest (serverUsed:TCPListenerServer) (client:TcpClient list option) =
        serverUsed.Stop ()
        Option.iter (fun clientList -> List.iter (fun (c:TcpClient) -> c.GetStream().Close (); c.Close ()) clientList) client

    let createClientAndConnect (port:int) =
        let client = new TcpClient()
        client.Connect(IPAddress.Loopback, port)
        client
    
    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        cleanupTest server None

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, then Start Again`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        cleanupTest server None

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start, then Stop Server`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Stop ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Stop, then Stop Again`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Stop ()
        server.Stop ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Stop Server Without Starting`` () =
        let server = new TCPListenerServer(44000)
        server.Stop ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, then Stop, then Start Again`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Stop ()

        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

        cleanupTest server None
        
    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Client Connects`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        
        let client = createClientAndConnect 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        cleanupTest server (Some [client])

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Two Client Connects, Check Active Connections`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        
        let client1 = createClientAndConnect 44000
        let client2 = createClientAndConnect 44000

        Async.Sleep 5000 |> Async.RunSynchronously
        
        Assert.IsTrue(server.ActiveConnections.Count = 2, "There is not 2 connections in the server's active connections list.")

        cleanupTest server (Some [client1; client2])

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Client Connects, then Closes Connection`` () =
        let server = new TCPListenerServer(44000)
        server.Start ()
        
        let client = createClientAndConnect 44000

        Async.Sleep 5000 |> Async.RunSynchronously
        
        client.GetStream().Close ()
        client.Close ()

        Async.Sleep 5000 |> Async.RunSynchronously

        Assert.IsTrue(server.ActiveConnections.Count = 0, "There are still connections in the server's active connections list.")

        // Client resources have already been disposed
        cleanupTest server (None)

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Client Connects, Stays Connected for Two Minutes`` () =
        let rec testLoop (server:TCPListenerServer) timeout =
            match DateTime.UtcNow > timeout with
            | true ->
                ()
            | false ->
                Assert.IsTrue(server.ActiveConnections.Count = 1, sprintf "Connection was lost after %O" (DateTime.UtcNow - timeout))
                testLoop server timeout

        let server = new TCPListenerServer(44000)
        server.Start ()
        
        let client = createClientAndConnect 44000
        
        Async.Sleep 5000 |> Async.RunSynchronously

        let timeout = DateTime.UtcNow.AddMinutes 2.0
        testLoop server timeout

        cleanupTest server (Some [client])