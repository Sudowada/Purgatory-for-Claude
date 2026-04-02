using UnityEngine;
using UnityEngine.UI;

public class SimpleCardTest : MonoBehaviour
{
    void Start()
    {
        // Add a button component to every card
        var cards = FindObjectsOfType<CardDisplay>();
        Debug.Log("Found " + cards.Length + " cards");
        
        foreach (var card in cards)
        {
            var button = card.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => {
                Debug.Log("Card clicked: " + card.Data);
            });
        }
    }
}