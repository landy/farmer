[<AutoOpen>]
module Farmer.Builders.SignalR

open Farmer
open Farmer.Arm.SignalRService
open Farmer.Helpers
open Farmer.SignalR

type SignalRConfig =
    { Name : ResourceName
      Sku : Sku
      Capacity : int option
      AllowedOrigins : string list
      Tags: Map<string,string> }
    member this.ResourceId = signalR.resourceId this.Name
    interface IBuilder with
        member this.ResourceId = this.ResourceId
        member this.BuildResources location = [
            { Name = this.Name
              Location = location
              Sku = this.Sku
              Capacity = this.Capacity
              AllowedOrigins = this.AllowedOrigins
              Tags = this.Tags  }
        ]
    member this.Key =
        let expr = sprintf "listKeys(resourceId('Microsoft.SignalRService/SignalR', '%s'), providers('Microsoft.SignalRService', 'SignalR').apiVersions[0]).primaryConnectionString" this.Name.Value
        ArmExpression.create(expr, this.ResourceId)

type SignalRBuilder() =
    member _.Yield _ =
        { Name = ResourceName.Empty
          Sku = Free
          Capacity = None
          AllowedOrigins = []
          Tags = Map.empty  }
    member _.Run(state:SignalRConfig) =
        { state with Name = state.Name |> sanitiseSignalR |> ResourceName }

    /// Sets the name of the Azure SignalR instance.
    [<CustomOperation("name")>]
    member _.Name(state:SignalRConfig, name) = { state with Name = name }
    member this.Name(state:SignalRConfig, name) = this.Name(state, ResourceName name)
    /// Sets the SKU of the Azure SignalR instance.
    [<CustomOperation("sku")>]
    member _.Sku(state:SignalRConfig, sku) = { state with Sku = sku }
    /// Sets the capacity of the Azure SignalR instance.
    [<CustomOperation("capacity")>]
    member _.Capacity(state:SignalRConfig, capacity) = { state with Capacity = Some capacity }
    /// Sets the allowed origins of the Azure SignalR instance.
    [<CustomOperation("allowed_origins")>]
    member _.AllowedOrigins(state:SignalRConfig, allowedOrigins) = { state with AllowedOrigins = allowedOrigins}
    [<CustomOperation "add_tags">]
    member _.Tags(state:SignalRConfig, pairs) =
        { state with
            Tags = pairs |> List.fold (fun map (key,value) -> Map.add key value map) state.Tags }
    [<CustomOperation "add_tag">]
    member this.Tag(state:SignalRConfig, key, value) = this.Tags(state, [ (key,value) ])

let signalR = SignalRBuilder()

