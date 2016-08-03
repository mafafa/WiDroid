module TCPListenerServer

open System.Collections.Generic
open System.Net
open System.Net.Sockets
open System.Threading.Tasks


type TCPListenerServer(discoveryPort:int) =
    let server = new TcpListener (IPAddress.Loopback, discoveryPort)

    let activeConnections = new List<TcpClient>()
    
    let serverLoop () =
        let rec loop (pendingConnection:Task<TcpClient>) = async {            
            let newPendingConnection, client =
                match pendingConnection.Status with
                | TaskStatus.Created | TaskStatus.WaitingForActivation | TaskStatus.WaitingToRun | TaskStatus.Running  ->
                    (None, None)
                | TaskStatus.Faulted | TaskStatus.Canceled ->
                    raise (new System.NotImplementedException())
                | TaskStatus.RanToCompletion ->
                    let connectionTask = server.AcceptTcpClientAsync ()
                    connectionTask.Start ()
                    (Some connectionTask, Some pendingConnection.Result)
                | _ -> 
                    raise (new System.NotImplementedException())
            
            // Add the new client to the list
            Option.iter (fun c -> activeConnections.Add c) client

            // Switch the new pending connection if there is one
            let connectionAttempt = defaultArg newPendingConnection pendingConnection

            return! loop connectionAttempt
        }

        try
            server.Start ()
            
            let connectionTask = server.AcceptTcpClientAsync ()
            connectionTask.Start ()
            
            loop connectionTask
        finally
            server.Stop ()

    member x.Start () =
        serverLoop ()

    member x.Stop () =
        server.Stop ()

    member x.ActiveConnections =
        activeConnections