module FileSyncHandlerMessageQueue

open System.Net
open System.Net.Sockets

open Types


type FileSyncMessageAndReplyChannel = FileSyncMessage*AsyncReplyChannel<FileSyncReply>

type FileSyncHandlerMessageQueue (client:TcpClient, remoteIP:IPAddress, port:int) =
    let errorEvent = new Event<string>()

    let fileSyncAgent = new MailboxProcessor<FileSyncMessageAndReplyChannel>(fun inbox ->
        let rec messageLoop (client:TcpClient) = async {
            //In case socket failed, attempt to reconnect
            try
                if not client.Connected
                then
                    client.Connect(remoteIP, port)
            with ex ->
                // Is it necessary to change the socket??
                let newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                newSocket.ReceiveTimeout <- 5000
                client.Client <- newSocket
                do! Async.Sleep(1000)
                return! messageLoop client
            
            let! message, replyChannel = inbox.Receive ()
            match message with
            | Stop ->
                client.Close ()
                client.Dispose ()
            | StartSync filesToSync ->
                try
                    try
                        Array.iter (fun filePath -> client.Client.SendFile(filePath)) filesToSync
                    with
                    | :? SocketException ->
                        raise (System.NotImplementedException("This exception handling is not yet implemented"))
                    | :? System.IO.FileNotFoundException ->
                        raise (System.NotImplementedException("This exception handling is not yet implemented"))
                finally
                    client.Close ()
                    client.Dispose ()
        }

        messageLoop client
    )

    let onError = errorEvent.Publish

    do onError.Add (fun errorMessage -> ()) //TODO: Log errors/exceptions for the messageLoop

    member x.Start () = 
        fileSyncAgent.Start ()
    
    member x.Stop () = 
        x.PostAndReply Stop
    
    member x.PostAndReply message = 
        fileSyncAgent.PostAndReply ((fun replyChannel -> message,replyChannel), timeout = 8000)
    
    member x.RemoteIP = 
        remoteIP

    member x.Port = 
        port