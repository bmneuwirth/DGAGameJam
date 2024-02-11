using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Image = UnityEngine.UI.Image;

public class PhotoUI : MonoBehaviour
{
    public PhotoScript photoScript;
    public Canvas canvas;
    public Image blackBackground;

    public FirstPersonController controller;
    public GameObject trashBin;

    private GameObject[] images;

    private bool menuMode;

    public int gridWidth = 4;
    public int gridHeight = 3;
    public float cellWidth;
    public float cellHeight;
    public float spacing = 10f;
    public RectOffset padding;

    // Start is called before the first frame update
    void Start()
    {
        // If screen resized, this will not update right now
        cellHeight = (Screen.height / (float)gridHeight) - (spacing * 2);
        cellWidth = cellHeight;
        padding = new RectOffset(10, 10, 10, 10);
        Assert.AreEqual(gridWidth * gridHeight, PhotoScript.MAX_PHOTOS);
        GenerateGrid();
        blackBackground.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        List<Photo> activePhotos = photoScript.activePhotos;
        controller.menu = menuMode; 

        if (Input.GetKeyDown(KeyCode.E))
        {
            menuMode = !menuMode;
            blackBackground.enabled = menuMode;

            if (!menuMode)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].SetActive(false);
                }

            }
        }

        if (menuMode)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (activePhotos.Count > i)
                {
                    images[i].SetActive(true);
                    images[i].GetComponent<Image>().material = activePhotos[i].material;
                }
                else
                {
                    images[i].SetActive(false);
                }
            }

        }

    }

    public bool IsMenuMode()
    {
        return menuMode;
    }

    void GenerateGrid()
    {
        images = new GameObject[gridWidth * gridHeight];

        GameObject gridPanel = new GameObject("GridPanel");
        gridPanel.transform.SetParent(canvas.transform, false);

        // Add RectTransform and configure it
        RectTransform rectTransform = gridPanel.AddComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Add GridLayoutGroup and configure it
        GridLayoutGroup gridLayout = gridPanel.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        gridLayout.spacing = new Vector2(spacing, spacing);
        gridLayout.padding = padding;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;

        // Optional: Add ContentSizeFitter to adjust the grid panel size based on its content
        ContentSizeFitter contentSizeFitter = gridPanel.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform panelRect = gridPanel.GetComponent<RectTransform>();
        float totalWidth = (cellWidth + spacing) * gridWidth - spacing;
        float totalHeight = (cellHeight + spacing) * gridHeight - spacing;

        // Optional: Adjust the size of the gridPanel to fit the grid exactly
        panelRect.sizeDelta = new Vector2(totalWidth, totalHeight);

        int i = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                images[i] = new GameObject("Photo UI Image");
                images[i].transform.SetParent(gridPanel.transform, false);
                images[i].SetActive(false);

                Image image = images[i].AddComponent<Image>();
                image.color = Color.white;

                RectTransform rect = images[i].GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(cellWidth, cellHeight);
                float posX = (cellWidth + spacing) * x;
                float posY = (cellHeight + spacing) * y;
                rect.anchoredPosition = new Vector2(posX, -posY);

                GameObject trashBinButton = Instantiate(trashBin, rect.transform, false);
                trashBinButton.name = i.ToString();
                trashBinButton.GetComponent<Button>().onClick.AddListener(() => photoScript.DeletePhoto(Convert.ToInt32(trashBinButton.name)));

                LayoutElement layoutElement = trashBinButton.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                RectTransform btnRect = trashBinButton.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(cellWidth / 2, cellHeight / 2);
                btnRect.anchorMin = new Vector2(1, 1);
                btnRect.anchorMax = new Vector2(1, 1);
                btnRect.pivot = new Vector2(1, 1);
                btnRect.anchoredPosition = new Vector2(-5, -5); // Adjust for padding from corner

                i++;
            }
        }
    }
}
