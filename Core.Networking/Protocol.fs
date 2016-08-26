module NetworkProtocol

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Runtime.Serialization.Formatters.Binary


let [<Literal>] EOT = 04uy
        
type FileTransferMessage = {
    DestPath : string
    FileContent: byte[]
}
type ACKMessage = {
    TimeACK : DateTime
}
type ResendMessage = {
    RetriesLeft : int
}

type MessageType =
| FileTransfer of FileTransferMessage
| ACK of ACKMessage
| Resend of ResendMessage

let sendFile (client:TcpClient) (srcFilePath:string) (destFilePath:string) = 
    let formatter = new BinaryFormatter()

    let fileBytes = File.ReadAllBytes srcFilePath

    // Create and serialize message into network stream
    let message = FileTransfer { DestPath = destFilePath; FileContent = fileBytes }
    formatter.Serialize (client.GetStream (), message)

let sendAck (client:TcpClient) =
    let formatter = new BinaryFormatter()

    // Create and serialize message into network stream
    let message = ACK { TimeACK = DateTime.UtcNow }
    formatter.Serialize (client.GetStream(), message)

let sendResend (client:TcpClient) (numRetries) =
    let formatter = new BinaryFormatter()

    // Create and serialize message into network stream
    let message = Resend { RetriesLeft = numRetries }
    formatter.Serialize (client.GetStream(), message)