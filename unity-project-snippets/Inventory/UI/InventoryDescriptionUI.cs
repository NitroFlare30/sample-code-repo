using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDescriptionUI : MonoBehaviour
{
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;

    [SerializeField]
    private Sprite emptySprite;

    private void Awake()
    {
        ResetDescription();
    }

    public void ResetDescription()
    {
        gameObject.SetActive(false);
        title.text = "";
        description.text = "";
    }

    public virtual void SetItemDescription(Sprite sprite, string itemName, string itemDescription)
    {
        gameObject.SetActive(true);
        itemImage.sprite = sprite;
        title.text = itemName;
        description.text = itemDescription;
    }

    public void SetQuestDescription(Quest quest)
    {
        gameObject.SetActive(true);
        itemImage.sprite = emptySprite;
        title.text = quest.info.name;
        description.text = quest.CurrentCheckpoint.GenerateDescription();
    }

    public void SetRelationDescription(Relation relation)
    {
        gameObject.SetActive(true);
        itemImage.sprite = relation.UI_Sprite;
        title.text = relation.characterName;
        description.text = "";
        description.text += "Points Until Relationship Increase: " + (Relation.MAX_RELATIONSHIP_POINTS - relation.CurrentRelationshipPoints).ToString() + "\n";
        // TODO: Account for other relation types
        description.text += "Relationship Level: " + relation.CurrentRelationshipLevel + "/" + (Relation.MAX_STANDARD_HEARTS).ToString() + "\n";
    }
}
