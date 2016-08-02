module Types

// Reply coming from the FileSyncMessageQueue
type FileSyncReply =
| Error of string
| FinishedSuccessfully

// Message going to the FileSyncMessageQueue
type FileSyncMessage =
| StartSync of string[]
| Stop