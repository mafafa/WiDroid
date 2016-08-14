module TestCommon

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Collections.Generic


    module TestCategories =
        let [<Literal>] Networking = "Networking"
        let [<Literal>] TCPListenerServerTest = "TCPListenerServerTest"
        let [<Literal>] ConnectionMessageQueueTest = "ConnectionMessageQueueTest"

    
    let arrayItemsAreEqual (array1:'a[]) (array2:'a[]) =
        let list1 = new List<'a>(array1)
        let list2 = new List<'a>(array2)
        CollectionAssert.AreEqual(list1, list2)