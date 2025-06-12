using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{

    public GameObject topBar;
    public GameObject bottomBar;
    public GameObject sideBar;

    public GameObject graphPanel;

    public GameObject dome;

    public ElementsSettingManager esm;

    GraphManager graphManager;

    void Start()
    {
        esm = GetComponent<ElementsSettingManager>();
        sideBar.SetActive(false);
        DomeSetActive(true);
        TopBarSetActive(true);
        BottomBarSetActive(true);
        GraphSetActive(false);
        SidebarSetActive(false);
        graphManager = graphPanel.GetComponent<GraphManager>();
    }

    public void TopBarSetActive(bool isActive)
    {
        if (topBar != null)
        {
            topBar.SetActive(isActive);
        }
    }
    public void BottomBarSetActive(bool isActive)
    {
        if (bottomBar != null)
        {
            bottomBar.SetActive(isActive);
        }
    }
    public void DomeSetActive(bool isActive)
    {
        if (dome != null)
        {
            dome.SetActive(isActive);
        }
    }

    public void GraphSetActive(bool isActive)
    {
        if (graphPanel != null)
        {
            if (isActive)
            {
                graphManager.LoadGraph();
            }
            foreach (Transform child in graphPanel.transform)
            {
                child.gameObject.SetActive(isActive);
            }
            Image image = graphPanel.GetComponent<Image>();
            if (image != null)
            {
                image.enabled = isActive;
            }
        }
    }

    public void SidebarSetActive(bool isActive, GameObject element = null)
    {
        if (sideBar != null)
        {
            sideBar.GetComponent<Animator>().SetBool("visible", isActive);
            if (!sideBar.activeSelf)
            {
                sideBar.SetActive(isActive);
            }
        }
        if (!isActive)
        {
            if (esm != null)
            {
                esm.Deselect();
            }
        }
    }

    public void CloseSidebar()
    {
        SidebarSetActive(false);
    }

    public void SwitchToGraphPanel()
    {
        BottomBarSetActive(false);
        SidebarSetActive(false);
        DomeSetActive(false);
        GraphSetActive(true);
    }

    public void SwitchToScene()
    {
        BottomBarSetActive(true);
        SidebarSetActive(false);
        DomeSetActive(true);
        GraphSetActive(false);
    }

    public void ToggleGraphPanel()
    {
        Image image = graphPanel.GetComponent<Image>();
        if (image != null)
        {
            if (image.IsActive())
            {
                SwitchToScene();
            }
            else
            {
                SwitchToGraphPanel();
            }
        }
    }


}
