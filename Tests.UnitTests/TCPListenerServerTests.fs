module TCPListenerServerTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Net
open System.Net.Sockets

open TestCategories
open TestBase
open TCPListenerServer


[<TestClass>]
type TCPListenerServerTests () as x =
    inherit TestBase ()

    let createClientAndConnect port =
        let client = new TcpClient()
        client.Connect(IPAddress.Loopback, port)
        x.CleanupActions.Add ((fun () -> client.GetStream().Close (); client.Close ()), "Disposing of client resources")
        client

    let createServerAndStart port =
        let server = new TCPListenerServer(44000)
        server.Start ()
        x.CleanupActions.Add ((fun () -> server.Stop ()), "Stopping Server Used")
        server


    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server`` () =
        let server = createServerAndStart 44000
        Async.Sleep 5000 |> Async.RunSynchronously

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, then Start Again`` () =
        let server = createServerAndStart 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start, then Stop Server`` () =
        let server = createServerAndStart 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Stop ()

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Stop, then Stop Again`` () =
        let server = createServerAndStart 44000
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
        let server = createServerAndStart 44000
        Async.Sleep 5000 |> Async.RunSynchronously

        server.Stop ()

        server.Start ()
        Async.Sleep 5000 |> Async.RunSynchronously
        
    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Client Connects`` () =
        let server = createServerAndStart 44000
        
        let client = createClientAndConnect 44000
        Async.Sleep 5000 |> Async.RunSynchronously

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Two Client Connects, Check Active Connections`` () =
        let server = createServerAndStart 44000
        
        let client1 = createClientAndConnect 44000
        let client2 = createClientAndConnect 44000

        Async.Sleep 5000 |> Async.RunSynchronously
        
        Assert.IsTrue(server.ActiveConnections.Count = 2, "There is not 2 connections in the server's active connections list.")

    [<TestMethod>]
    [<TestCategory(Networking)>]
    [<TestCategory(TCPListenerServerTest)>]
    member x.``Start Server, Client Connects, then Closes Connection`` () =
        let server = createServerAndStart 44000
        
        let client = createClientAndConnect 44000
        x.RemoveLastCleanupAction ()

        Async.Sleep 5000 |> Async.RunSynchronously
        
        client.GetStream().Close ()
        client.Close ()

        Async.Sleep 5000 |> Async.RunSynchronously

        Assert.IsTrue(server.ActiveConnections.Count = 0, "There are still connections in the server's active connections list.")

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

        let server = createServerAndStart 44000
        
        let client = createClientAndConnect 44000
        
        Async.Sleep 5000 |> Async.RunSynchronously

        let timeout = DateTime.UtcNow.AddMinutes 2.0
        testLoop server timeout