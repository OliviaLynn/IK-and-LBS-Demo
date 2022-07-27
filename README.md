# Inverse Kinematics & Linear Blend Skinning Demo

![IK GIF](https://github.com/OliviaLynn/IK-and-LBS-Demo/blob/main/IK_Demo.gif?raw=true)

The **inverse kinematics** demo consists of an arm with three bones and is bounded by a pole (the fixed sphere in the background). The arm tries to reach the target (the mobile sphere) and the location of its bones is calculated by 10 backwards/forward iterations per frame (or, almost - we run this function in OnLateUpdate!)

![LBS GIF](https://github.com/OliviaLynn/IK-and-LBS-Demo/blob/main/LBS_Demo.gif?raw=true)

The **linear blend skinning** demo bypasses Unity's automatic reskinning by simulating armature with two cylinders acting as "bones" for the larger, colorful cylinder mesh. The colors correspond to the weight of each bone on each vertex, and each vertex's position is manually calculated via a weighted interpolation between the transforms of the two bones. 

We can see LBS's characteristic volume loss there, which lets us know our manual calculation is on the right track :-)
