# Unity3D client for Ubi-Interact

## Unity Package

Import "ubii.unitypackage" to get all scripts and plugins. Create a gameobject and attach Scripts/ubii/client/UbiiClient.cs as your client connection to the UBII backend.

## Example Unity project

Open the scene "Test" in the unity project under /Ubi-Interact-Client/Assets/Scenes


## Update dependencies

- Get the newest protobuf definitions from https://github.com/SandroWeber/ubii-msg-formats/tree/develop/dist/cs
- copy them to /Ubi-Interact-Client/Assets/ubii/protobuf/

# How to use

- Add an object with the [UbiiNode.cs](https://github.com/SandroWeber/ubii-node-unity3D/blob/develop/Ubi-Interact-Client/Assets/ubii/scripts/client/UbiiNode.cs) script attached
- change Service and TopicData URLs according to master node config. IMPORTANT: service communication over HTTP(S) only works with the ".../binary" endpoint for now
