# Prerequisities:
- Malbers Animal Controller
- Unity Netcode for GameObjects
- ParrelSync (for testing 2 players - otherwise you can use your own method of connecting the clients)
- If you haven't heard of ParrelSync I highly recommend it - no more building to test your multiplayer game!
- https://github.com/VeriorPies/ParrelSync

IF you already have ClientNetworkAnimator and/or ClientNetworkTransform, you can remove these from the NGOAC folder (they're from the Unity Co-op Client Auth Example)

## Tested on:
2022.3.21f1

## Limitations:
- This has only been tested with ClientNetworkTransform i.e. clients have authority of their positions and animations
- Late Join is not supported 
	- clients joining after another player has picked up a weapon will not see the weapon as equipped
- Projectiles (from bow/rifle) are not in sync, these should probably be spawned and owned by the server
- Clients are checking their own damage against them - this can result in pvp hits not registering, this should be flipped to an I Shot You First approach for better gameplay feel if players can damage each other

# Setup Instructions:
1. Download the latest unitypackage from [latest releases](https://github.com/Serinoxxx/NGOAC/releases/)
2. Import "NGOAC.unitypackage" into your project
3. Open the "PlayGround Human (1 - Holsters)" scene (This is from the Malbers Animal Controller demo scenes)
4. Rename "Steve" in the hierarchy to "SteveNGO"
5. Create a NetworkPrefabs folder
6. Drag "SteveNGO" into the folder and save as original 
	- Delete SteveNGO from the scene
7. Disable the following components on the SteveNGO prefab (make sure it's applied to the prefab)
	Animal
	Interactor
	MalbersInput
	States
	EventListener
	MDamageable
	Collider(Interactor)
	Stamina Sprint Data (disable the whole gameObject)
	CM Main Target -> Unity Event Raiser
	CM Main Target -> Transform Hook
	
8. Add an empty GameObject to SteveNGO called "AimTransform" and add a "ClientNetworkTransform" component
	- We only need to sync position for this, so you can disable rotation and scale
	
9. Add the following components to the root of SteveNGO
NetworkObject
ClientNetworkTransform
NetworkRigidbody
ClientNetworkAnimator - assign the animator property
PlayerConnectedEvents - assign all of the references
					  - Camera Event Raiser - CM Main Target
					  - Camera Transform Hook - CM Main Target
					  - Pick Up Drop - Interactions
					  - Stamina Sprint Data - Stamina (Sprint) Data
					  - Aim Target - AimTransform
					  
10. On the Main Canvas, add the NetworkPlayerUIController component. For UIHolsters - assign holsters and the corresponding image component e.g.
	- Element 0 - Back Holster 1 - Inventory/Button Back Holster/Image
	- Element 1 - Left Holster - Inventory/Button Left Holster/Image
	- Element 2 - Right Holster - Inventory/Button Right Holster/Image
	
	For Weapon Sprite Map, assign the DemoWeaponSpriteMapping (if you have custom weapons/sprites you'll need to add them to this SO)
	
	Note*- I had to handle the weapon sprites this way because by default the AC uses events to set the sprite and the sprite is embedded in the event. We don't listen to the event (as it happens for all players) so I needed a custom solution here.
	
11. Add an empty gameobject to the scene called "NetworkManager" 
    - add a NetworkManager component. 
		- Set the Player Prefab to SteveNGO
		- Select transport type "Unity Transport"
	- Add the PlayerRegister component
	
12. This system relies on each weapon having a unique Index. I noticed in the demo scene the "Sword Collectable" and "Sword Collectable Fire" have the same Index.
    - click on the "Sword Collectable Fire", under the Melee Weapon -> General -> General -> Click the "Generate" button on the index field to generate a unique ID.
	- apply the change as an override for sword collectable fire
	- repeat this for Rifle
	- If you don't do this then the player could pick up the wrong weapon
	
13. Delete the -----------Characters-------------- object from the scene
	- this can cause issues with the camera

14. For the shootable weapons (bow, rifle etc), set Shootable->Aiming-Aim Action to "Manual". This seems to resolve several issues

15. (optional) on the UI/Settings Menu canvas, remove the pause input on the MInput component
    - I find that pausing the host/client can cause some issues with multiplayer
	
## SETUP COMPLETED!

# Testing Instructions (Required ParrelSync):
1. Use ParrelSync to make a clone project
2. Use ParrelSync to open the clone in a new editor
3. Enter Play Mode, it will ask if you want to add the scene to the build list - do this
4. While running, click on the NetworkManager (under DontDestroyOnLoad) and click StartHost
5. Enter Play Mode in your clone editor
6. click on the NetworkManger and click StartClient

Hopefully it all works!
