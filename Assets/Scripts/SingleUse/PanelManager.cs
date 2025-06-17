using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{

    public enum ViewType
    {
        Graph,
        Scene
    }

    public ViewType currentViewType = ViewType.Scene;

    public GameObject topBar;
    public GameObject bottomBar;
    public GameObject sideBar;

    public GameObject graphPanel;

    public GameObject dome;

    public GameObject sceneTilePrefab;
    public Transform SceneOverviewList;

    GraphManager graphManager;
    SceneManager sceneManager;

    SidebarSettingsManager sidebarSettingsManager;

    Vector2 sideBarAnchorMin;

    RectTransform sideBarRectTransform;

    void Start()
    {
        graphManager = graphPanel.GetComponent<GraphManager>();
        sidebarSettingsManager = FindObjectOfType<SidebarSettingsManager>();
        sceneManager = FindObjectOfType<SceneManager>();

        sideBarRectTransform = sideBar.GetComponent<RectTransform>();
        sideBarAnchorMin = sideBar.GetComponent<RectTransform>().anchorMin;

        DomeSetActive(true);
        TopBarSetActive(true);
        BottomBarSetActive(true);
        GraphSetActive(false);
        SidebarSetActive(false);

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

    public void UpdateSceneList()
    {
        for (int i = SceneOverviewList.childCount - 1; i > 0; i--)
        {
            Destroy(SceneOverviewList.GetChild(i).gameObject);
        }
        foreach (var scene in sceneManager.sceneList.Values)
        {
            SceneTile sceneTile = Instantiate(sceneTilePrefab, SceneOverviewList).GetComponent<SceneTile>();
            sceneTile.Setup(scene, scene.IsStartScene);
            sceneTile.editButton.onClick.AddListener(() =>
            {
                sidebarSettingsManager.OpenSceneSettings(scene.Name);
            });
        }
    }

    public void SidebarSetActive(bool isActive)
    {
        if (isActive)
        {
            if (currentViewType == ViewType.Scene)
            {
                sideBarRectTransform.anchorMin = sideBarAnchorMin;
            }
            else
            {
                sideBarRectTransform.anchorMin = new Vector2(sideBarAnchorMin.x, 0);
            }
        }

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
            if (sidebarSettingsManager != null)
            {
                sidebarSettingsManager.Deselect();
            }
        }
    }

    public void CloseSidebar()
    {
        SidebarSetActive(false);
    }

    public void SwitchToGraphPanel()
    {
        currentViewType = ViewType.Graph;
        BottomBarSetActive(false);
        SidebarSetActive(false);
        DomeSetActive(false);
        GraphSetActive(true);
    }

    public void SwitchToScene()
    {
        currentViewType = ViewType.Scene;
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
