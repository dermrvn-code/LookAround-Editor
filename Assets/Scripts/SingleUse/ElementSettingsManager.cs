using UnityEngine;

public class ElementsSettingManager : MonoBehaviour
{
    public Camera cam;
    public DomePosition selector;
    public GameObject selectedElement;

    PanelManager panelManager;
    void Start()
    {
        panelManager = FindObjectOfType<PanelManager>();
    }

    void Select(GameObject target)
    {
        DomePosition domePosition = target.GetComponent<DomePosition>();

        Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
        {
            selectedElement = target;
            Vector3 size = targetRenderer.bounds.size;
            Debug.Log("Size: " + size);

            Vector3 selectorScale = selector.transform.localScale;
            selectorScale.x = size.x;
            selectorScale.y = size.x;

            selector.transform.localScale = selectorScale;
            selector.position = domePosition.position;
            selector.gameObject.SetActive(true);
            panelManager.SidebarSetActive(true, target);
        }
    }

    void Deselect()
    {
        selectedElement = null;
        if (selector != null)
        {
            selector.gameObject.SetActive(false);
        }
        panelManager.SidebarSetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject target = hit.collider.gameObject;
                Select(target);
            }
            else
            {
                Deselect();
            }
        }
    }
}
