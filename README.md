# Inverse Kinematics & Linear Blend Skinning Demo

![IK GIF](https://github.com/OliviaLynn/IK-and-LBS-Demo/blob/main/IK_Demo.gif?raw=true)

The **inverse kinematics** demo consists of an arm with three bones and is bounded by a pole (the fixed sphere in the background). The arm tries to reach the target (the mobile sphere) and the location of its bones is calculated by 10 backwards/forward iterations per frame (or, almost - we run this function in OnLateUpdate!)

*A thank you to Ditzel Games' [IK Tutorial](https://www.youtube.com/watch?v=qqOAzn05fvk&ab_channel=DitzelGames) on YouTube, as well as the videos [Complete Procedural Animation in 25 Mintes](https://www.youtube.com/watch?v=abrJ3LXjLzA&t=485s&ab_channel=TimiTayo) and [Creating procedural walk movement](https://www.youtube.com/watch?v=acMK93A-FSY&ab_channel=Unity) which first explained the concept to me while I was poking around the internet back in Febrary. And ultimately, a huge shoutout to Karl Sims' 1994 [Evolved Virtual Creatures](https://youtu.be/JBgG_VSP7f8) that first got me interested in the possible uses of procedural animation.*

![LBS GIF](https://github.com/OliviaLynn/IK-and-LBS-Demo/blob/main/LBS_Demo.gif?raw=true)

The **linear blend skinning** demo bypasses Unity's automatic reskinning by simulating armature with two cylinders acting as "bones" for the larger, colorful cylinder mesh. The colors correspond to the weight of each bone on each vertex, and each vertex's position is manually calculated via a weighted interpolation between the transforms of the two bones. 

We can see LBS's characteristic volume loss there, which is not aesthetially ideal but lets us know our manual calculation is on the right track :-)

*A thank you to Pixel Fondue's [How It Works | Linear Blend Skinning Part 1](https://youtu.be/QDXG4wNzkOE), which walked me through the principles of LBS, and Rodolphe Vaillant's [Dual Quaternions skinning tutorial and C++ codes](http://rodolphe-vaillant.fr/entry/29/dual-quaternions-skinning-tutorial-and-c-codes), which further explains the concepts and lays out the mathematical equations behind the mesh deformations (while also getting into DQS and how it works!)*

*(In my LBS exploration I also encountered Ladislav Kavan's Siggraph Course [Skinning: Real-time Shape Deformation](https://skinning.org/direct-methods.pdf), which looks like it would be fun to read further and try out implementing - but maybe in openCV or something other than Unity.)*
