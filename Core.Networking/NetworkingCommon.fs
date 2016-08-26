module NetworkingCommon

open System
open System.IO
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets
open System.Runtime.Serialization.Formatters.Binary
open System.Text

    
    module ClientServerCommunication =       
        let [<Literal>] EOT = 04uy
        
        (*type FileTransferMessage = {
            DestPath : string
            FileContent: byte[]
        }
        type ACKMessage = {
            TimeACK : DateTime
        }
        type ResendMessage = {
            RetriesLeft : int
        }*)

        type MessageType =
        | FileTransfer// of FileTransferMessage
        | ACK// of ACKMessage
        | Resend //of ResendMessage
        type Message = {
            Type : MessageType
            Content : obj option
        }

        let sendFile (client:TcpClient) (srcFilePath:string) = 
            let formatter = new BinaryFormatter()

            // Deserialize message content to obj
            let fileBytes = File.ReadAllBytes srcFilePath
            let deserializedContent = fileBytes :> obj

            // Create and serialize message into network stream
            let message = { Type = FileTransfer; Content = Some deserializedContent }
            formatter.Serialize (client.GetStream (), message)

        let sendAck (client:TcpClient) =
            let formatter = new BinaryFormatter()

            // Create and serialize message into network stream
            let message = { Type = ACK; Content = None }
            formatter.Serialize (client.GetStream(), message)

        let sendResend (client:TcpClient) =
            let formatter = new BinaryFormatter()

            // Create and serialize message into network stream
            let message = { Type = Resend; Content = None }
            formatter.Serialize (client.GetStream(), message)

open ClientServerCommunication

let connectionIsStillActive (client:TcpClient) =
    let ipProperties = IPGlobalProperties.GetIPGlobalProperties ()
    let allTcpConnections = ipProperties.GetActiveTcpConnections ()
    let relevantTcpConnections = Array.filter (fun (connectionInfo:TcpConnectionInformation) -> 
        (connectionInfo.LocalEndPoint = (client.Client.LocalEndPoint :?> IPEndPoint)) && (connectionInfo.RemoteEndPoint = (client.Client.RemoteEndPoint :?> IPEndPoint))) allTcpConnections
        
    try
        let stateOfConnection = (Array.get relevantTcpConnections 0).State
        match stateOfConnection with
        | TcpState.Established ->
            true
        | _ ->
            false
    with
    | :? System.IndexOutOfRangeException as ex ->
        false

let readStreamToFile (client:TcpClient) outputPath =
    let formatter = new BinaryFormatter()

    try
        let message = (formatter.Deserialize (client.GetStream ())) :?> Message
        match message.Type with
        | FileTransfer ->
            match message.Content with
            | Some content ->
                let bytesContent = 
                    use mStream = new MemoryStream()
                    formatter.Serialize (mStream, content)
                    mStream.ToArray ()

                File.WriteAllBytes (outputPath, bytesContent)
            | None ->
                failwith "There was no content in the FileSync message!!!"
        | _ ->
            ()
    with
    | :? InvalidCastException as ex ->
        failwith "Message format unknown!!!"