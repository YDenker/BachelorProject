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

    private static Dictionary<string, List<Line>> lines = new Dictionary<string, List<Line>>();

    public static void AddLine(string key, Line line) => lines[key].Add(line);
    public static void AddListOfLines(string key, List<Line> lineList) => lines.Add(key, lineList);
    public static void ClearLines(string key) => lines[key].Clear();
    public static void RemoveLines(string key) => lines.Remove(key);

    private void OnPostRender()
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        foreach (var list in lines.Values)
        {
            for (int index = 0; index < list.Count; index++)
            {
                GL.Vertex(list[index].a);
                GL.Vertex(list[index].b);
            }
        }
        GL.End();
    }
}
