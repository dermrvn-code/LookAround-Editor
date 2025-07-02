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
        Scene,
        Bundler
    }

    public ViewType currentViewType = ViewType.Scene;

    public GameObject topBar;
    public GameObject bottomBar;
    public GameObject sideBar;

    public GameObject graphPanel;

    public GameObject bundlerPanel;

    public GameObject dome;

    public GameObject sceneTilePrefab;
    public Transform SceneOverviewList;

    GraphManager graphManager;
    SceneManager sceneManager;
    ProjectManager projectManager;

    SidebarSettingsManager sidebarSettingsManager;

    Vector2 sideBarAnchorMin;

    RectTransform sideBarRectTransform;
    Animator sideBarAnimator;
    void Start()
    {
        graphManager = graphPanel.GetComponent<GraphManager>();
        sidebarSettingsManager = FindObjectOfType<SidebarSettingsManager>();
        sceneManager = FindObjectOfType<SceneManager>();
        projectManager = FindObjectOfType<ProjectManager>();

        sideBarRectTransform = sideBar.GetComponent<RectTransform>();
        sideBarAnchorMin = sideBar.GetComponent<RectTransform>().anchorMin;
        sideBarAnimator = sideBar.GetComponent<Animator>();

        DomeSetActive(true);
        TopBarSetActive(true);
        BottomBarSetActive(true);
        GraphSetActive(false);
        SidebarSetActive(false);
        BundlerSetActive(false);
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

    void BundlerSetActive(bool isActive)
    {
        if (bundlerPanel != null)
        {
            bundlerPanel.SetActive(isActive);
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

    public void AddWorld()
    {
        sidebarSettingsManager.OpenWorldSettings(newWorld: true);
    }

    public void OpenWorldSettings()
    {
        if (projectManager.IsInProject())
        {
            sidebarSettingsManager.OpenWorldSettings(newWorld: false);
        }
        else
        {
            InfoText.ShowInfo("Lade ein Projekt, um die Welteinstellungen zu bearbeiten.");
        }
    }

    public void OpenAppSetings()
    {
        sidebarSettingsManager.OpenAppSettings();
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
            if (sideBarAnimator == null)
            {
                sideBarAnimator = sideBar.GetComponent<Animator>();
            }
            sideBarAnimator.SetBool("visible", isActive);
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
        BundlerSetActive(false);
    }

    public void SwitchToScene()
    {
        currentViewType = ViewType.Scene;
        BottomBarSetActive(true);
        SidebarSetActive(false);
        DomeSetActive(true);
        GraphSetActive(false);
        BundlerSetActive(false);
    }

    public void SwitchToBundler()
    {
        currentViewType = ViewType.Bundler;
        BottomBarSetActive(false);
        SidebarSetActive(false);
        DomeSetActive(false);
        GraphSetActive(false);
        BundlerSetActive(true);
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
