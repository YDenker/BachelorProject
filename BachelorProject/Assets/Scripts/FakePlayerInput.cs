using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FakePlayerInput", menuName = "CreateTrainingInputFile", order = 1)]
public class FakePlayerInput : ScriptableObject
{
    [Tooltip("Check this to efficiently train the brain")]
    public bool Randomize;
    [Tooltip("Radius of the sphere")]
    [Range(3, 20)]
    [SerializeField]
    private float radius;
    [Tooltip("The brain will attempt to make all the faces have x corners")]
    [SerializeField]
    [Range(3,5)]
    private int attemptedVerticesPerFace;
    [Tooltip("The brain will attempt to create the sphere with approximatly x corners")]
    [SerializeField]
    [Range(12, 1280)]
    private int approximateVerticesTotal;
    [Tooltip("This is the accuracy you want to train your brain with. 0 = only flawless shapes. 1 = shapes with many flaws \nWARNING: Creating flawless shapes is sometimes impossible to achieve!")]
    [Range(0f,1f)]
    public float MarginOfError;

    public float Radius { get => Randomize ? Random.value * 17 + 3 : radius; }
    public int AttemptedVerticesPerFace { get => Randomize ? (int)(Random.value * 2 + 3) : attemptedVerticesPerFace; }
    public int ApproximateVerticdesTotal { get => Randomize ? (int)(Random.value * 1268 + 12) : approximateVerticesTotal; }
}
