using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A quick oval racing lab for Unity ML-Agents.
/// Attach this component to an empty GameObject, then use the component's
/// three-dot menu > Build Oval Track Now (works in Edit Mode).
/// The white/yellow centreline is a visual reference, not a trained controller.
/// </summary>
public class QuickOvalTrackBuilder : MonoBehaviour
{
    [Header("Optional: assign an existing ML-Agent car prefab")]
    public GameObject agentPrefab;

    [Header("Oval dimensions (Unity units)")]
    public float outerRadiusX = 20f;
    public float outerRadiusZ = 14f;
    public float innerRadiusX = 15f;
    public float innerRadiusZ = 9f;
    [Range(32, 256)] public int lineSegments = 120;

    [ContextMenu("Build Oval Track Now")]
    public void BuildOvalTrackNow()
    {
        const string generatedName = "GENERATED_OVAL_TRACK";
        Transform previous = transform.Find(generatedName);
        if (previous != null)
        {
            if (Application.isPlaying) Destroy(previous.gameObject);
            else DestroyImmediate(previous.gameObject);
        }

        GameObject generated = new GameObject(generatedName);
        generated.transform.SetParent(transform, false);

        // Flat, green ground beneath everything.
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(generated.transform, false);
        ground.transform.localPosition = new Vector3(0f, -0.21f, 0f);
        ground.transform.localScale = new Vector3(60f, 0.2f, 50f);
        Colour(ground, new Color(0.22f, 0.46f, 0.20f));

        // Outer asphalt oval: a flattened cylinder scaled into an ellipse.
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        road.name = "Grey Road Ring - outer ellipse";
        road.transform.SetParent(generated.transform, false);
        road.transform.localPosition = new Vector3(0f, -0.13f, 0f);
        road.transform.localScale = new Vector3(outerRadiusX * 2, 0.13f, outerRadiusZ * 2);
        Colour(road, new Color(0.17f, 0.18f, 0.20f));

        // Green centre masks the inner road, creating an oval road ring.
        GameObject infield = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        infield.name = "Green Infield";
        infield.transform.SetParent(generated.transform, false);
        infield.transform.localPosition = new Vector3(0f, -0.09f, 0f);
        infield.transform.localScale = new Vector3(innerRadiusX * 2, 0.13f, innerRadiusZ * 2);
        Colour(infield, new Color(0.30f, 0.55f, 0.23f));
        // This is only a colour overlay, not an obstacle.
        Collider infieldCollider = infield.GetComponent<Collider>();
        if (infieldCollider != null)
        {
            if (Application.isPlaying) Destroy(infieldCollider);
            else DestroyImmediate(infieldCollider);
        }

        // Elliptical yellow centreline, drawn without hand-placing checkpoints.
        GameObject line = new GameObject("Yellow Centreline - visual guide");
        line.transform.SetParent(generated.transform, false);
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = lineSegments;
        lr.startWidth = 0.18f;
        lr.endWidth = 0.18f;
        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null) lr.sharedMaterial = new Material(lineShader);
        float centerX = (outerRadiusX + innerRadiusX) * 0.5f;
        float centerZ = (outerRadiusZ + innerRadiusZ) * 0.5f;
        for (int i = 0; i < lineSegments; ++i)
        {
            float angle = (Mathf.PI * 2f * i) / lineSegments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * centerX, 0.065f,
                                           Mathf.Sin(angle) * centerZ));
        }

        // White stripe at the right-hand edge of the oval.
        GameObject finishStripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishStripe.name = "FINISH LINE - visible stripe";
        finishStripe.transform.SetParent(generated.transform, false);
        finishStripe.transform.localPosition = new Vector3(centerX, 0.071f, 0f);
        finishStripe.transform.localScale = new Vector3(outerRadiusX - innerRadiusX, 0.012f, 0.55f);
        Colour(finishStripe, Color.white);
        Collider stripeCollider = finishStripe.GetComponent<Collider>();
        if (stripeCollider != null)
        {
            if (Application.isPlaying) Destroy(stripeCollider);
            else DestroyImmediate(stripeCollider);
        }

        GameObject finishTrigger = new GameObject("FINISH LINE TRIGGER - connect to your agent code");
        finishTrigger.transform.SetParent(generated.transform, false);
        finishTrigger.transform.localPosition = new Vector3(centerX, 0.75f, 0f);
        BoxCollider trigger = finishTrigger.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(outerRadiusX - innerRadiusX, 1.5f, 0.55f);

        // Create four coloured placeholders, or four copies of your assigned agent prefab.
        Color[] colours = { Color.red, Color.cyan, Color.blue, Color.magenta };
        for (int i = 0; i < 4; i++)
        {
            float angle = 0.25f + i * 0.22f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * centerX,
                                           0.55f,
                                           Mathf.Sin(angle) * centerZ);
            Vector3 tangent = new Vector3(-Mathf.Sin(angle) * centerX, 0f,
                                           Mathf.Cos(angle) * centerZ).normalized;
            GameObject car;
            if (agentPrefab != null)
            {
                car = Instantiate(agentPrefab, generated.transform);
                car.name = "Racing Agent " + (i + 1);
                car.transform.localPosition = position;
                car.transform.localRotation = Quaternion.LookRotation(tangent, Vector3.up);
            }
            else
            {
                car = GameObject.CreatePrimitive(PrimitiveType.Cube);
                car.name = "Placeholder Car " + (i + 1) + " - replace with your agent";
                car.transform.SetParent(generated.transform, false);
                car.transform.localPosition = position;
                car.transform.localRotation = Quaternion.LookRotation(tangent, Vector3.up);
                car.transform.localScale = new Vector3(0.75f, 0.5f, 1.5f);
                Colour(car, colours[i]);
            }
        }

        Debug.Log("Oval, continuous centreline, finish stripe, trigger and 4 cars created. " +
                  "Reward and lap validation still require your ML-Agents C# agent script.", this);
    }

    private static void Colour(GameObject obj, Color color)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;
        bool srp = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = srp ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return;
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        rend.sharedMaterial = mat;
    }
}