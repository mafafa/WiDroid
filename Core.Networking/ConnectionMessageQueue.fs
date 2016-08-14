module ConnectionMessageQueue

open System
open System.Net
open System.Net.Sockets


    module Communication =
        type ConnectionMessageQueueError = {
            Exception : Exception
            Message : string
        }
        
        // Reply coming from the FileSyncMessageQueue
        type ConnectionQueueReply =
        | Error of ConnectionMessageQueueError
        | FinishedSuccessfully

        // Message going to the FileSyncMessageQueue
        type ConnectionQueueMessage =
        | StartSync of string[]
        | Stop

open Communication

type FileSyncMessageAndReplyChannel = ConnectionQueueMessage * AsyncReplyChannel<ConnectionQueueReply>

type ConnectionMessageQueue (client:TcpClient, remoteIP:IPAddress, port:int) =
    let errorEvent = new Event<ConnectionMessageQueueError>()

    let fileSyncAgent = new MailboxProcessor<FileSyncMessageAndReplyChannel>(fun inbox ->
        let rec messageLoop (client:TcpClient) = async {
            //In case socket failed, attempt to reconnect
            try
                if not client.Connected
                then
                    client.Connect(remoteIP, port)
            with
            | :? SocketException as ex ->
                Async.Sleep 5000 |> Async.RunSynchronously
                return! messageLoop client
            | :? ObjectDisposedException as ex ->
                let newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                newSocket.ReceiveTimeout <- 5000
                client.Client <- newSocket
                Async.Sleep 2000 |> Async.RunSynchronously
                return! messageLoop client
            
            let! message, replyChannel = inbox.Receive ()
            match message with
            | Stop ->
                try
                    client.GetStream().Close ()
                    client.Close ()
                    replyChannel.Reply FinishedSuccessfully
                with
                | :? ObjectDisposedException as ex ->
                    replyChannel.Reply FinishedSuccessfully
                | ex ->
                    replyChannel.Reply (Error { Exception = ex; Message = sprintf "Exception in stopping message queue with remote IP %A: %s" remoteIP ex.Message })
                
                return ()
            | StartSync filesToSync ->
                try
                    Array.iter (fun filePath -> client.Client.SendFile(filePath)) filesToSync
                    replyChannel.Reply FinishedSuccessfully
                with ex ->
                    replyChannel.Reply (Error { Exception = ex; Message = sprintf "Exception in sending file for sync to remote IP %A: %s" remoteIP ex.Message })

                return! messageLoop client
        }

        messageLoop client
    )

    let onError = errorEvent.Publish

    do onError.Add (fun message -> ()) //TODO: Log errors/exceptions for the messageLoop

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