using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawer : MonoBehaviour
{
    public struct Line
    {
        public Vector3 a;
        public Vector3 b;

        public Line(Vector3 start, Vector3 end)
        {
            a = start;
            b = end;
        }
    }

    public Material material;

    private static List<Line> lines = new List<Line>();

    public static void AddLine(Line line) => lines.Add(line);
    public static void ClearLines() => lines.Clear();

    private void OnPostRender()
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        foreach (var line in lines)
        {
            GL.Vertex(line.a);
            GL.Vertex(line.b);
        }
        GL.End();
    }
}
