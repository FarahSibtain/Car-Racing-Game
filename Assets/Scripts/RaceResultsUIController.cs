using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceResultsUIController : MonoBehaviour
{
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Transform resultsContainer;
    [SerializeField] private GameObject resultEntryPrefab;
    [SerializeField] private TMP_Text positionDisplay;
    [SerializeField] private float displayDelay = 2.0f;

    // Reference to finish line
    private RaceFinishLine finishLine;

    private void Start()
    {
        // Find finish line
        finishLine = FindObjectOfType<RaceFinishLine>();

        if (finishLine != null)
        {
            // Subscribe to race results event
            finishLine.OnRaceResultsUpdated += DisplayResults;
        }

        // Hide results panel initially
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        if (finishLine != null)
        {
            finishLine.OnRaceResultsUpdated -= DisplayResults;
        }
    }

    public void DisplayResults(List<string> results)
    {
        if ( results.Count == 0)
        {
            return;
        }
        // Clear existing results
        ClearResults();

        // Show results panel
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        // Populate results (with slight delay between each for visual effect)
        StartCoroutine(PopulateResultsWithDelay(results));
    }

    private System.Collections.IEnumerator PopulateResultsWithDelay(List<string> results)
    {
        for (int i = 0; i < results.Count; i++)
        {
            // Create result entry
            GameObject entry = Instantiate(resultEntryPrefab, resultsContainer);

            // Set position text (1st, 2nd, 3rd, etc.)
            TextMeshProUGUI positionText = entry.transform.Find("PositionText")?.GetComponent<TextMeshProUGUI>();
            if (positionText != null)
            {
                positionText.text = $"{i + 1}{GetOrdinalSuffix(i + 1)}";
            }

            // Set player name text
            TextMeshProUGUI nameText = entry.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = results[i];
            }

            // Wait before showing next entry
            yield return new WaitForSeconds(displayDelay);
        }
    }

    private void ClearResults()
    {
        // Destroy all child objects in results container
        foreach (Transform child in resultsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private string GetOrdinalSuffix(int position)
    {
        int remainder = position % 100;
        if (remainder >= 11 && remainder <= 13)
        {
            return "th";
        }

        remainder = position % 10;
        switch (remainder)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    // Method to hide results panel
    public void HideResults()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }

    public void DisplayPosition(string positionStr)
    {
        positionDisplay.text = positionStr;
    }
}