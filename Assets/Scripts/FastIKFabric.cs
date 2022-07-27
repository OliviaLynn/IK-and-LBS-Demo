using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FastIKFabric : MonoBehaviour
{
    // Big shoutout to Ditzel Games' IK Tutorial on YouTube:
    // https://www.youtube.com/watch?v=qqOAzn05fvk&ab_channel=DitzelGames

    // The code here was written while following along with the video (not 
    // copy+pasted!) and additional comments have been included during coding 
    // both to encourage actual comprehension of concepts presented and to
    // leave markers for areas I'd like to return to and further explore.

    // Chain length of bones
    public int ChainLength = 2;

    // Target we're bending the chain to
    public Transform Target;
    public Transform Pole; // todo idk about the access modifiers here - would be better to just serialize field?
    
    // Solver iterations per update
    [Header("Solver Parameters")]
    public int Iterations = 10; // in this simple example there seems to be little functional difference, but for more complex armatures I'm sure it starts to show

    // Distance when the solver stops
    public float Delta = 0.001f;

    //Strength of going back to the start position
    [Range(0, 1)]
    public float SnapBackStrength = 1.0f;
    
    protected float[] BonesLength; // Ordered from the target to the origin
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    protected Vector3[] StartDirectionSucc; // The direction from one bone to its successor (28:04)
    protected Quaternion[] StartRotationBone;
    protected Quaternion StartRotationTarget;
    protected Quaternion StartRotationRoot;

    void Awake() {
        Init();
    }

    void Init() {
        // Initialize Arrays
        // "Bones are transforms"
        // Seems we can imagine Bones/Positions as vertices in a graph and BonesLength as edges
        Bones = new Transform[ChainLength+1]; 
        Positions = new Vector3[ChainLength+1];
        BonesLength = new float[ChainLength];
        StartDirectionSucc = new Vector3[ChainLength+1];
        StartRotationBone = new Quaternion[ChainLength+1];

        // Initialize Fields
        StartRotationTarget = Target.rotation;
        CompleteLength = 0;

        // Our Initial Data
        var current = transform;
        for (var i = Bones.Length-1; i >= 0; i--) {

            Bones[i] = current;
            StartRotationBone[i] = current.rotation;

            if (i == Bones.Length-1) { //differentiate btwn leaf bone and mid bone
                StartDirectionSucc[i] = Target.position - current.position;
            }
            else {
                StartDirectionSucc[i] = Bones[i+1].position - current.position;
                BonesLength[i] = StartDirectionSucc[i].magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }

        if (Bones[0] == null)
            throw new UnityException("The chain val is longer than the ancestor chain!");
    }

    void LateUpdate() {
        ResolveIK();
    }

    private void ResolveIK() {
        if (Target == null) 
            return;

        if (BonesLength.Length != ChainLength) 
            Init();
        
        // Get position
        for (int i = 0; i < Bones.Length; i++) 
            Positions[i] = Bones[i].position; // "We do not do any computations on the bones directly"
        
        var RootRot = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity; // current rotation of root bone (but why via parent? TODO revisit)
        var RootRotDiff = RootRot * Quaternion.Inverse(StartRotationRoot);

        // Calculation
        // 12:30 he explains why this is squared, sort of, but not really - if it's just an inequality I don't see the need (TODO look into this)
        if ((Target.position - Bones[0].position).sqrMagnitude >= CompleteLength * CompleteLength) { 
            // We wanna stretch the arm if the target is too far to actually reach
            var direction = (Target.position - Positions[0]).normalized;
            // set everything after root
            for (int i = 1; i < Positions.Length; i++) 
                Positions[i] = Positions[i-1] + direction * BonesLength[i-1];
        }
        else {
            for (int iteration = 0; iteration < Iterations; iteration++) {
                // back algorithm - start at the last bone (the one closest to target), work our way back to the root
                for (int i = Positions.Length-1; i > 0; i--) {
                    if (i == Positions.Length-1)
                        Positions[i] = Target.position; // set it to target
                    else
                        Positions[i] = Positions[i+1] + (Positions[i] - Positions[i+1]).normalized * BonesLength[i];
                }

                // forward algorithm - pull it all back to the root
                for (int i = 1; i < Positions.Length; i++) 
                    Positions[i] = Positions[i-1] + (Positions[i] - Positions[i-1]).normalized * BonesLength[i-1];

                // check if close enough
                if ((Positions[Positions.Length-1] - Target.position).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

        // Move toward pole (see 23:45 for explanation of the projections onto plane, and finding minimal distance to pole via the plane)
        if (Pole != null) {
            for (int i = 1; i < Positions.Length-1; i++) {
                var plane = new Plane(Positions[i+1] - Positions[i-1], Positions[i-1]);
                var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - Positions[i-1], projectedPole - Positions[i-1], plane.normal); // TODO revisit this bit (~28:00)
                Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i-1]) + Positions[i-1];
            }
        }

        // Set position and rotation
        for (int i = 0; i < Positions.Length; i++) {
            if (i == Positions.Length-1)
                Bones[i].rotation = Target.rotation * Quaternion.Inverse(StartRotationTarget) * StartRotationBone[i];
            else
                Bones[i].rotation = Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i+1] - Positions[i]) * StartRotationBone[i];
            Bones[i].position = Positions[i];
        }
    }

    void OnDrawGizmos() { // gizmos are why we're using UnityEditor
        var current = this.transform; // TODO why's he using var all the time? can we type this more strongly? is there a reason to not have done that?
        for (int i = 0; i < ChainLength && current != null && current.parent != null; i++) {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            // "Handles are just bars in the unity editor" he says in the video
            Handles.matrix = Matrix4x4.TRS(current.position, // TODO look through this to properly understand each argument
                                            Quaternion.FromToRotation(Vector3.up, current.parent.position-current.position), 
                                            new Vector3(scale, Vector3.Distance(current.parent.position, current.position), 
                                            scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
    }
}
