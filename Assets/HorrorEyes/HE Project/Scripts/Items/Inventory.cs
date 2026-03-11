using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;


public class Inventory : MonoBehaviour {

    [Header("Inventory Settings")]
    private GameControll m_gameControll;
    [HideInInspector]
    public ItemsDatabase m_itemsDatabase;
    public Transform m_slotsContent;
    public GameObject m_slotPrefab;
    public List<Slot> m_slots = new List<Slot>();

    [Header("UI Settings")]
    public Sprite m_emptySprite;

    private int m_currentSlot = 0;

    private int selectedItemID = -1;

    private void Awake()
    {
        m_gameControll = GetComponent<GameControll>();
        m_itemsDatabase = GetComponent<ItemsDatabase>();
    }

    private void Update()
    {
        if (!m_gameControll.m_mobileTouchInput)
        {
            PCSelection();
        }
    }

    public int GetSelectedItemID()
    {
        if (selectedItemID != -1)
        {
            return m_slots[selectedItemID].m_itemID;
        }
        else
        {
            return -1;
        }
    }

    private void PCSelection()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0) ChangeSelectedSlot(-1);
        if (scroll < 0) ChangeSelectedSlot(1);
    }

    private void ChangeSelectedSlot(int direction)
    {
        m_currentSlot += direction;
        if (m_currentSlot < 0)
        {
            m_currentSlot = m_slots.Count - 1;
        }
        else if (m_currentSlot >= m_slots.Count)
        {
            m_currentSlot = 0;
        }
        selectedItemID = m_currentSlot;
        ValidateSelection();
    }

    public void AddItem (int id, int cnt)
    {
        if (id != 0)
        {

            int same = GetSlotWithSameItem(id);

            if (same != -1)
            {
                m_slots[same].m_itemCount += cnt;
                PrepareSlot(m_slots[same]);
            }
            else
            {
                GameObject slt = Instantiate(m_slotPrefab, m_slotsContent);
                Slot newSlot = slt.GetComponent<Slot>();
                newSlot.m_itemID = id;
                newSlot.m_itemCount = cnt;
                m_slots.Add(newSlot);
                PrepareSlot(newSlot);

                if (id == 0) /// if item id == 0 (eyePills id)
                {
                    m_gameControll.AddEyePills(1);
                }

            }
        }else
        {
            if (id == 0) /// if item id == 0 (eyePills id)
            {
                m_gameControll.AddEyePills(1);
            }
        }
        ValidateSelection();
    }

private void ValidateSelection()
{
    if (m_slots.Count == 0) selectedItemID = -1;
    else if (selectedItemID >= m_slots.Count) selectedItemID = m_slots.Count - 1;
    else if (selectedItemID == -1) selectedItemID = 0;
    RefreshSelectedVisual();
}

private void RefreshSelectedVisual()
    {
        for (int i = 0; i < m_slots.Count; i++)
        {
            if (i == selectedItemID)
            {
                m_slots[i].m_icon.color = Color.green;
            }
            else
            {
                m_slots[i].m_icon.color = Color.white;
            }
        }
    }


    public void RemoveItem(int itemID, int removeCount)
    {
        int same = GetSlotWithSameItem(itemID);

        if(same != -1)
        {
            m_slots[same].m_itemCount -= removeCount;
            m_gameControll.ShowTip(itemID,4);

            if(m_slots[same].m_itemCount <= 0)
            {
                Destroy(m_slots[same].gameObject);
                m_slots.RemoveAt(same);
            }else
            {
                PrepareSlot(m_slots[same]);
            }
        }
        ValidateSelection();
    }



    private void PrepareSlot(Slot slot)
    {

        if (slot.m_itemID != -1)
        {
            int dbID = m_itemsDatabase.GetItemInDatabaseByID(slot.m_itemID);
            if (dbID != -1)
            {
                slot.m_icon.sprite = m_itemsDatabase.Items[dbID].m_itemIcon;
                slot.m_countText.text = slot.m_itemCount.ToString();

            }
        }else
        {
            slot.m_itemCount = 0;
            slot.m_countText.text = "";
            slot.m_icon.sprite = m_emptySprite;
        }
    }


    int GetFreeSlot()
    {
        for (int i = 0; i < m_slots.Count; i++)
        {
            if (m_slots[i].m_itemID == -1)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetSlotWithSameItem(int id)
    {
        for (int i = 0; i < m_slots.Count; i++)
        {
            if(m_slots[i].m_itemID == id)
            {
                return i;
            }
        }

        return -1;
    }


}
