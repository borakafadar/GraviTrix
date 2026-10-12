using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalGradientText : BaseMeshEffect
{
    public Color leftColor = Color.cyan;
    public Color rightColor = Color.magenta;

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
        {
            return;
        }

        List<UIVertex> vertices = new List<UIVertex>();
        vertexHelper.GetUIVertexStream(vertices);

        float minX = vertices[0].position.x;
        float maxX = vertices[0].position.x;

        for (int i = 1; i < vertices.Count; i++)
        {
            float x = vertices[i].position.x;

            if (x < minX)
            {
                minX = x;
            }

            if (x > maxX)
            {
                maxX = x;
            }
        }

        float width = maxX - minX;

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];

            float t = width <= 0.01f ? 0f : Mathf.InverseLerp(minX, maxX, vertex.position.x);
            vertex.color = Color.Lerp(leftColor, rightColor, t);

            vertices[i] = vertex;
        }

        vertexHelper.Clear();
        vertexHelper.AddUIVertexTriangleStream(vertices);
    }
}
