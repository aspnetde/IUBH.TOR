namespace IUBH.TOR.Tests

open System
open Fabulous
open Fabulous.XamarinForms

[<AutoOpen>]
module TestHelpers =   
    /// Returns a random string that's unique each time
    let randomString() = Guid.NewGuid().ToString()

    /// Looks for an view element with the given Class ID in the view hierarchy.
    /// That should have been the Automation ID, but there's a bug right now using that.
    /// 
    /// This function is not optimized for efficiency and may execute slowly.
    /// Which shouldn't be noticable in pure test scenarios, though.
    let rec tryFindViewElement classId (element:ViewElement) =
        let elementClassId = element.TryGetAttributeKeyed(ViewAttributes.ClassIdAttribKey)
        if elementClassId.IsSome && elementClassId.Value = classId then
            Some(element)
        else
            let childElements = ResizeArray<ViewElement>()
                            
            let content = element.TryGetAttributeKeyed(ViewAttributes.ContentAttribKey)
            if content.IsSome then
                childElements.Add content.Value
            else                  
                let pages = element.TryGetAttributeKeyed(ViewAttributes.PagesAttribKey)
                if (pages.IsSome) then
                    childElements.AddRange pages.Value
                else                    
                    let children = element.TryGetAttributeKeyed(ViewAttributes.ChildrenAttribKey)
                    if (children.IsSome) then
                        childElements.AddRange children.Value
               
            let childElementResult =
                childElements
                |> Seq.map (fun e -> e |> tryFindViewElement classId)
                |> Seq.filter (fun e -> e.IsSome)
                |> Seq.tryHead
                
            match childElementResult with
            | Some result -> result
            | None -> None

    /// Checks if a Cmd.ofMsg will dispatch a certain message
    /// (including its payload) 
    let dispatchesMessage msg cmd =
        let messages = ResizeArray<_>()
        cmd |> Seq.iter (fun f -> f(fun x -> messages.Add x))
        messages |> Seq.exists (fun m -> m = msg)
