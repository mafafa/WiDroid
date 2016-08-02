module TCPListenerServer

open System.Net
open System.Net.Sockets

open FileSyncHandlerMessageQueue


type TCPListenerServer(discoveryPort:int) =
    let server = new TcpListener (IPAddress.Loopback, discoveryPort)

    let doSyncEvent = new Event<(IPAddress*int)[]>()
    
    let serverLoop () =
        let rec loop () = async {
            let client = server.AcceptTcpClient ()

            // Create queue to manage file sync with that client
            let ipEndPoint = client.Client.RemoteEndPoint :?> IPEndPoint
            let queue = new FileSyncHandlerMessageQueue(client, ipEndPoint.Address, ipEndPoint.Port)
            queue.Start ()

            return! loop ()
        }

        try
            server.Start ()
            loop ()
        finally
            server.Stop ()

    let onSyncTrigger = doSyncEvent.Publish

    do onSyncTrigger.Add (fun errorMessage -> ()) //TODO: Log errors/exceptions for the messageLoop

    member x.Start () =
        serverLoop ()

    member x.Stop () =
        server.Stop ()

    member x.TriggerSync (remoteClient:(IPAddress*int)[]) =
        doSyncEvent.Trigger remoteClient