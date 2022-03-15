using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;


public class PlayerEquipmentController : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform inventoryUIParent;

    [Header("Anchors")]
    [SerializeField] private Transform helmetAnchor;

	

	[SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private Transform armorAnchor;

    private GameObject currentHelmetObj;
    private GameObject currentLeftHandObj;
    private GameObject currentRighHandObj;
    private GameObject currentArmorObj;
    private int playerHealth = 0;

    private void Start()
    {
        StartCoroutine(fetchNFT());
        UnityEngine.Debug.Log("fetch complete");

        UnityEngine.Debug.Log("INit Inventory");
        //inventory.InitInventory(this);

        UnityEngine.Debug.Log("OpenInventoryUI");
        inventory.OpenInventoryUI();
    }

    async Task UseDelay()
    {
        UnityEngine.Debug.Log("In delay");
        await Task.Delay(3000); // wait for 1 second
        UnityEngine.Debug.Log("End delay");
    }

    IEnumerator fetchNFT()
    {
        var url = "https://api.ci.xooa.io/api/v2/nft/my-tokens?page-size=10&bookmark=&async=false&timeout=5000";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        var apiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJBcGlLZXkiOiI2N0hBNDc4LVlSUjRSQTgtR0FFWk5WOS1ZWjUxWlo5IiwiQXBpU2VjcmV0IjoicTN1ME8yTmNxMnR0RlBTIiwiUGFzc3BocmFzZSI6IjI0OTNjYzgxY2VlN2IzZjZiZDc3ZWFiNzQzY2Q2ODZhIiwiaWF0IjoxNjQ2ODAyMDMxfQ.xG1IbKKMEfHkhpGPNVCMiTDQncK3yzaGu3NJnSz7oyE";
        request.Headers.Add("Authorization", "Bearer "+ apiToken);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        JObject info = JObject.Parse(json);
        string pathToDelete = @$"/Users/neelshah/Downloads/Xooa/InventorySystem/Assets/Data/Items/";

        UnityEngine.Debug.Log("Fetched Object from Xooa");
        UnityEngine.Debug.Log(info);
        System.IO.DirectoryInfo di = new DirectoryInfo(pathToDelete);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (var asset in info["data"])
        {
            var assetName = asset["properties"]["asset"][0]["originalName"].ToString();
            var assetURL = asset["properties"]["asset"][0]["url"]["preview"].ToString();
            UnityEngine.Debug.Log(assetURL);
            UnityEngine.Debug.Log(assetName);
            addAssetInItems(assetName, assetURL);
        }
        UnityEngine.Debug.Log(" Yeild return ");
        yield return 1;
    }

    private void addAssetInItems(string name, string url)
    {
        UnityEngine.Debug.Log("Adding Asset===");
        string savePath = @$"/Users/neelshah/Downloads/Xooa/InventorySystem/Assets/Data/Items/{name}";
        WebClient client = new WebClient();
        client.DownloadFile(url, savePath);
    }

    public void AssignHelmetItem(HelmetInventoryItem item)
	{
        DestroyIfNotNull(currentHelmetObj);
        currentHelmetObj = CreateNewItemInstance(item, helmetAnchor);
	}

    public void AssignHandItem(HandInventoryItem item)
    {
        switch (item.hand)
        {
            case Hand.LEFT:
                DestroyIfNotNull(currentLeftHandObj);
                currentLeftHandObj = CreateNewItemInstance(item, leftHandAnchor);
                break;
            case Hand.RIGHT:
                DestroyIfNotNull(currentRighHandObj);
                currentRighHandObj = CreateNewItemInstance(item, rightHandAnchor);
                break;
            default:
                break;
        }
    }

    public void AssingArmorItem(ArmorInventoryItem item)
    {
        DestroyIfNotNull(currentArmorObj);
        currentArmorObj = CreateNewItemInstance(item, armorAnchor);
    }

    public void AssingHealthPotionItem(HealthPotionInventoryItem item)
    {
        inventory.RemoveItem(item, 1);
        playerHealth += item.GetHealthPoints();
        Debug.Log(string.Format("Player has now {0} health points", playerHealth));
    }

    private GameObject CreateNewItemInstance(InventoryItem item, Transform anchor)
    {
        var itemInstance = Instantiate(item.GetPrefab(), anchor);
        itemInstance.transform.localPosition = item.GetLocalPosition();
        itemInstance.transform.localRotation = item.GetLocalRotation();
        return itemInstance;
    }

    private void DestroyIfNotNull(GameObject obj)
    {
        if (obj)
        {
            Destroy(obj);
        }
    }

    public Transform GetUIParent()
    {
        return inventoryUIParent;
    }
}
