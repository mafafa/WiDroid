module NetworkingCommon

open System
open System.IO
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets
open System.Runtime.Serialization.Formatters.Binary
open System.Text

open NetworkProtocol


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

let readStreamToFile (client:TcpClient) =
    let formatter = new BinaryFormatter()

    try
        let message = (formatter.Deserialize (client.GetStream ())) :?> MessageType
        match message with
        | FileTransfer fileSyncMessage ->
            match fileSyncMessage.FileContent with
            | content when content.Length > 0 ->
                File.WriteAllBytes (fileSyncMessage.DestPath, content)
            | _ ->
                failwith "There was no content in the FileSync message!!!"
        | _ ->
            ()
    with
    | :? InvalidCastException as ex ->
        failwith "Message format unknown!!!"