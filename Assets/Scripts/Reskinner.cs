using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reskinner : MonoBehaviour
{
    [SerializeField]
    private GameObject BoneOne;

    [SerializeField]
    private GameObject BoneTwo;
   
    public GameObject selfClone;

    Mesh mesh;
    Vector3 midpoint;

    float boneOneEnd;
    float boneTwoEnd;
    float[] boneWeights;

    Vector3[] originalVertices;
    Vector3[] rotatedVertices;

    void Awake() {
        Init();
    }

    void Init() {
        // Initialize basics
        mesh = GetComponent<MeshFilter>().mesh;
        midpoint = (BoneOne.transform.position + BoneTwo.transform.position) / 2.0f;
        originalVertices = mesh.vertices;
        rotatedVertices = mesh.vertices;

        // Calculate the ends of the bones
        Bounds boundsOne = BoneOne.GetComponent<Collider>().bounds;
        Bounds boundsTwo = BoneTwo.GetComponent<Collider>().bounds;
        boneOneEnd = boundsOne.center.z - 0.5f * boundsOne.size.z;        
        boneTwoEnd = boundsTwo.center.z + 0.5f * boundsTwo.size.z;

        // Calculate, record, and visualize the bone weights along the mesh
        boneWeights = new float[mesh.vertices.Length];
        Color[] colors = new Color[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++) {
            float weight = GetBoneOneWeight(transform.TransformPoint(mesh.vertices[i]));
            boneWeights[i] = weight;
            colors[i] = Color.Lerp(Color.cyan, Color.yellow, weight);
        }
        mesh.colors = colors;
    }

    float GetBoneOneWeight(Vector3 pos) {
        if (pos.z > boneOneEnd) 
            return 1.0f;
        if (pos.z < boneTwoEnd)
            return 0.0f;
        // Linear for now (TODO try out sigmoidal maybe? how are weights usually assigned?)
        return (pos.z - boneTwoEnd)/(boneOneEnd - boneTwoEnd);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Recalculate things, in case we shift the bones around in the editor while we're playing
            midpoint = (BoneOne.transform.position + BoneTwo.transform.position) / 2.0f;
            Bounds boundsOne = BoneOne.GetComponent<Collider>().bounds;
            Bounds boundsTwo = BoneTwo.GetComponent<Collider>().bounds;
            boneOneEnd = boundsOne.center.z - 0.5f * boundsOne.size.z;        
            boneTwoEnd = boundsTwo.center.z + 0.5f * boundsTwo.size.z;
            boneWeights = new float[mesh.vertices.Length];
            Color[] colors = new Color[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++) {
                float weight = GetBoneOneWeight(transform.TransformPoint(mesh.vertices[i]));
                boneWeights[i] = weight;
                colors[i] = Color.Lerp(Color.cyan, Color.yellow, weight);
            }
            mesh.colors = colors;
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            float angle = -3.0f;
            BoneOne.transform.RotateAround(midpoint, Vector3.up, angle);
            RotateAndRunLBS(angle);
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            float angle = 3.0f;
            BoneOne.transform.RotateAround(midpoint, Vector3.up, angle);
            RotateAndRunLBS(angle);
        }
    }

    void RotateAndRunLBS(float angle) {
        // record vertex positions as if they all moved with bone one
        selfClone.transform.Rotate(0.0f, angle, 0.0f); // in retrospect, we could have just childed selfClone to BoneOne - this would've automatically given the proper rotation very cleanly. TODO on refactor
        for (int i = 0; i < rotatedVertices.Length; i++) {
            rotatedVertices[i] = selfClone.transform.localToWorldMatrix.MultiplyPoint3x4(selfClone.GetComponent<MeshFilter>().mesh.vertices[i]);
        }
        //selfClone.transform.Rotate(0.0f, -angle, 0.0f);

        // calculate + assign interpolated values to each vertex
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < mesh.vertices.Length; i++) {
            var newPositionIfBoneOne = transform.worldToLocalMatrix.MultiplyPoint3x4(rotatedVertices[i]);
            var newPositionIfBoneTwo = originalVertices[i];
            vertices[i] = newPositionIfBoneOne * boneWeights[i] + newPositionIfBoneTwo * (1 - boneWeights[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
