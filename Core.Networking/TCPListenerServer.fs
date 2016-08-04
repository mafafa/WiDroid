module TCPListenerServer

open System.Collections.Generic
open System.Net
open System.Net.Sockets
open System.Threading
open System.Threading.Tasks


type TCPListenerServer(discoveryPort:int) =
    let server = new TcpListener (IPAddress.Loopback, discoveryPort)

    let activeConnections = new List<TcpClient>()
    let cancellationToken = new CancellationTokenSource()
    
    let rec loop (pendingConnection:Task<TcpClient>) = async {            
        let newPendingConnection, client =
            match pendingConnection.Status with
            | TaskStatus.Created | TaskStatus.WaitingForActivation | TaskStatus.WaitingToRun 
            | TaskStatus.WaitingForChildrenToComplete | TaskStatus.Running  ->
                (None, None)
            | TaskStatus.Faulted ->
                let result = pendingConnection.Exception
                raise (new System.NotImplementedException())
            | TaskStatus.Canceled ->
                raise (new System.NotImplementedException())
            | TaskStatus.RanToCompletion ->
                let connectionTask = server.AcceptTcpClientAsync ()
                (Some connectionTask, Some pendingConnection.Result)
            | _ -> 
                raise (new System.NotImplementedException())
            
        // Add the new client to the list
        Option.iter (fun c -> activeConnections.Add c) client

        // Switch the new pending connection if there is one
        let connectionAttempt = defaultArg newPendingConnection pendingConnection

        // Check that the connections are still alive
        Seq.iter (fun (connection:TcpClient) -> if not connection.Connected then activeConnections.Remove connection |> ignore) activeConnections

        Async.Sleep 1000 |> Async.RunSynchronously

        //match cancellationToken.IsCancellationRequested with
        //| true ->
        //    ()
        //| false ->
        return! loop connectionAttempt
    }

    member x.Start () =
        if not cancellationToken.Token.CanBeCanceled
        then
            raise (new System.Exception("Cancellation token cannot be used to cancel server loop task."))

        try
            server.Start ()
            let connectionTask = server.AcceptTcpClientAsync ()
            Async.Start (loop connectionTask, cancellationToken.Token)
        with 
        | :? SocketException as ex ->
            server.Stop ()
            raise ex

    member x.Stop () =
        cancellationToken.Cancel ()
        Async.Sleep 2000 |> Async.RunSynchronously
        server.Stop ()
        activeConnections.Clear ()

    member x.ActiveConnections =
        activeConnections