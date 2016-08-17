module NetworkingCommon

open System
open System.IO
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets

    
    module ClientServerCommunication =
        let [<Literal>] EOT = 04uy

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

let readStreamToFile (stream:NetworkStream) outputPath =
    let rec readStreamToFileLoop (fileStream:FileStream) buffer =
        let bytesRead = stream.Read(buffer, 0, buffer.Length)
        match Array.contains EOT buffer with
        | true ->
            // Remove EOT byte and "extra space" of the array so that it doesn't write null bytes in the file
            let bufferWithoutNullOrEOT = Array.filter ((<>) EOT) buffer |> Array.filter ((<>) 0uy)
            fileStream.Write(bufferWithoutNullOrEOT, 0, bufferWithoutNullOrEOT.Length)
            ()
        | false ->
            fileStream.Write(buffer, 0, bytesRead)
            readStreamToFileLoop fileStream buffer  
        
    let buffer = Array.zeroCreate 1024
    let fileStream = File.Open(outputPath, FileMode.Create)
    try
        readStreamToFileLoop fileStream buffer
    finally
        fileStream.Close ()