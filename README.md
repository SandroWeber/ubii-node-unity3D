# Unity3D client for Ubi-Interact

## Unity Package

Import "ubii.unitypackage" to get all scripts and plugins. Create a gameobject and attach Scripts/ubii/client/UbiiClient.cs as your client connection to the UBII backend.

# How to use

- Add an object with the [UbiiNode.cs](https://github.com/SandroWeber/ubii-node-unity3D/blob/develop/Ubi-Interact-Client/Assets/ubii/scripts/client/UbiiNode.cs) script attached
- in the script's configuration, change Service and TopicData URLs according to master node configuration. IMPORTANT: service communication over HTTP(S) only works with the ".../binary" endpoint for now


## Example Unity project

Open one of the scenes under /Ubi-Interact-Client/Assets/ubii/scenes/


## Update dependencies (only necessary if message definitions are outdated)

- Get the newest protobuf definitions from https://github.com/SandroWeber/ubii-msg-formats/tree/develop/dist/cs
- copy them to /Ubi-Interact-Client/Assets/ubii/protobuf/
