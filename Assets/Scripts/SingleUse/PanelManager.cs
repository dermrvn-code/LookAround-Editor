using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{

    public GameObject topBar;
    public GameObject bottomBar;
    public GameObject sideBar;

    public GameObject graphPanel;

    public GameObject dome;

    void Start()
    {
        dome.SetActive(true);
        topBar.SetActive(true);
        graphPanel.SetActive(false);
        bottomBar.SetActive(true);
        sideBar.SetActive(true);
    }

    public void SwitchToGraphPanel()
    {
        bottomBar.SetActive(false);
        sideBar.SetActive(false);
        dome.SetActive(false);
        graphPanel.SetActive(true);
    }

    public void SwitchToScene()
    {
        bottomBar.SetActive(true);
        sideBar.SetActive(true);
        dome.SetActive(true);
        graphPanel.SetActive(false);
    }


}
