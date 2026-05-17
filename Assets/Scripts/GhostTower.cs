using UnityEngine;

public class GhostTower : MonoBehaviour
{
    SpriteRenderer[] srs;
    LineRenderer lr;

    private void Awake()
    {
        srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            Color c = sr.color;
            c.a = 0.4f;
            sr.color = c;
        }

        lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            lr = gameObject.AddComponent<LineRenderer>();
        }

        lr.positionCount = 51;
        lr.useWorldSpace = false;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 5;
    }

    public void SetValid(bool valid)
    {
        if (srs != null)
        {
            Color color = valid ? new Color(0, 1, 0, 0.4f) : new Color(1, 0, 0, 0.4f);
            foreach (var sr in srs)
            {
                sr.color = color;
            }
        }
        
        Color lrColor = valid ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        lr.startColor = lrColor;
        lr.endColor = lrColor;
    }

    public void SetRadius(float radius)
    {
        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }
    }
}
