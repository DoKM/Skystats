using fNbt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Helper;

public enum IMAGETYPE
{
    Flat,
    Boots,
    Leggings,
    Chestplate,
    Helmet
}

public class Slot : MonoBehaviour
{
    public GameObject imageParent;
    public TMP_Text stackText;
    public Image rarityTierImage;
    public Item currentHoldingItem;
    public List<Item> backpackContents;

    [HideInInspector] public GameObject flatItemImagePrefab, textureImagePrefab, leatherHelmetPrefab, leatherChestplatePrefab, leatherLeggingsPrefab, leatherBootsPrefab, tooltip, tooltipHolder, backpackHolder, petXPBar;
    public Texture2D itemTexture;

    private void Awake()
    {
        flatItemImagePrefab = Resources.Load<GameObject>("Prefabs/Slot/2D");
        textureImagePrefab = Resources.Load<GameObject>("Prefabs/Slot/3D");
        tooltip = GameObject.FindGameObjectWithTag("Tooltip");
        tooltipHolder = tooltip.transform.GetChild(0).gameObject;
        backpackHolder = tooltipHolder.transform.Find("Backpack Preview IGNORE").GetChild(0).gameObject;

        leatherHelmetPrefab = Resources.Load<GameObject>("Prefabs/Slot/Armor/Helmet");
        leatherChestplatePrefab = Resources.Load<GameObject>("Prefabs/Slot/Armor/Chestplate");
        leatherLeggingsPrefab = Resources.Load<GameObject>("Prefabs/Slot/Armor/Leggings");
        leatherBootsPrefab = Resources.Load<GameObject>("Prefabs/Slot/Armor/Boots");
        petXPBar = Resources.Load<GameObject>("Prefabs/Pet XP Bar");
    }

    private void Update()
    {
        if (imageParent.transform.childCount <= 0 && gameObject.activeInHierarchy)
            FillItem(currentHoldingItem);
        else if (imageParent.transform.childCount > 1)
            Destroy(imageParent.transform.GetChild(1).gameObject);
    }

    public virtual void FillItem(Item newItem)
    {
        ClearChildren(imageParent.transform);
        currentHoldingItem = newItem;

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (flatItemImagePrefab == null)
            flatItemImagePrefab = Resources.Load<GameObject>("Prefabs/Slot/2D");

        InstantiateModel(Main.Instance.transparentCutout);
    }

    public void InstantiateModel(Shader shader)
    {
        // Item has a 3d model with texture
        if (currentHoldingItem != null && currentHoldingItem != new Item())
        {
            UpdateStackAmountText(currentHoldingItem.StackAmount);
            UpdateRarity(currentHoldingItem.RarityTier);

            if (currentHoldingItem.IsLeather)
                InstantiateLeatherArmor();
            else if (currentHoldingItem.TextureLink != "" && currentHoldingItem.TextureLink != null)
            {
                if (gameObject.activeInHierarchy)
                    if (shader == Main.Instance.transparentCutoutMask)
                        StartCoroutine(ApplyWardrobeTexture(shader));
                    else
                        StartCoroutine(ApplyTexture(shader));
            }
            else if (currentHoldingItem.ItemIDString != null)
                InstantiateItem();
        }
        else
            InstantiateEmpty();
    }

    private void InstantiateEmpty()
    {
        var emptySprite = Resources.Load<Sprite>("Block Images/Textures/Empty");
        InstantiateImage(emptySprite, flatItemImagePrefab);

        UpdateStackAmountText(1);
        UpdateRarity(RARITY.UNKNOWN);
    }

    private void InstantiateItem()
    {
        var itemSprite = Resources.Load<Sprite>($"Block Images/Textures/{currentHoldingItem.ItemIDString}");
        var idNoDamage = Regex.Match(currentHoldingItem.ItemIDString, "[0-9]+(?=-)");

        if (itemSprite == null)
            itemSprite = Resources.Load<Sprite>($"Block Images/Textures/{idNoDamage}-0");

        InstantiateImage(itemSprite, flatItemImagePrefab);
    }

    private void InstantiateLeatherArmor()
    {
        var armorPieceMatch = Regex.Match(currentHoldingItem.Name.ToUpper(), "(BOOTS|LEGGINGS|CHESTPLATE|HELMET|OXFORDS|JACKET|PANTS|FEDORA|POLO|TROUSERS|GALOSHES)");
        var armorPieceName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(armorPieceMatch.Value);
        var armorPiece = Resources.Load<GameObject>("Prefabs/Slot/Armor/" + armorPieceName);

        InstantiateImage(armorPiece, currentHoldingItem.LeatherArmorColor);
    }

    private void UpdateStackAmountText(byte count)
    {
        if (count > 1)
            stackText.text = count.ToString();
        else stackText.text = "";
    }

    private void ColorArmorPiece(Transform armorPieceParent, Color newColor)
    {
        var newMat = new Material(Shader.Find("UI/Default"));
        newMat.color = newColor;

        for (int i = 0; i < armorPieceParent.childCount - 1; i++)
        {
            var img = armorPieceParent.GetChild(i).GetComponent<Image>();
            img.maskable = true;
            img.material = newMat;
        }
    }

    private void UpdateRarity(RARITY rarityTier)
    {
        string hexColor = "#" + GetRarityHexColor(rarityTier);
        ColorUtility.TryParseHtmlString(hexColor, out var finalColor);

        if (rarityTier == RARITY.UNKNOWN)
            finalColor.a = 0.3f;

        rarityTierImage.color = finalColor;
    }

    private string GetRarityHexColor(RARITY tier)
    {
        switch (tier)
        {
            case RARITY.COMMON:
                return "AAAAAA";
            case RARITY.UNCOMMON:
                return "55FF55";
            case RARITY.RARE:
                return "5555FF";
            case RARITY.EPIC:
                return "AA00AA";
            case RARITY.LEGENDARY:
                return "FFAA00";
            case RARITY.MYTHIC:
                return "FF55FF";
            case RARITY.SUPREME:
                return "AA0000";
            case RARITY.SPECIAl:
                return "FF5555";
            case RARITY.VERY_SPECIAL:
                return "AA0000";
            case RARITY.UNKNOWN:
                return "555555";
        }

        return "555555";
    }

    private IEnumerator ApplyTexture(Shader shader)
    {
        yield return StartCoroutine(GetTextureFromLink(currentHoldingItem.TextureLink));

        var tex = itemTexture;
        FillImage(tex, shader);
    }

    private IEnumerator ApplyWardrobeTexture(Shader shader)
    {
        var tempMat = GetSetMaterial(shader);
        yield return StartCoroutine(GetTextureFromLink(currentHoldingItem.TextureLink));

        var tex = itemTexture;
        tempMat.SetTexture("_MainTex", tex);
    }

    public IEnumerator GetTextureFromLink(string textureLink)
    {
        // Get the texutre ID and check if it exists
        // in the saved textures folder for better performance
        string textureID = textureLink.Substring(textureLink.LastIndexOf("/") + 1);

        string path = Path.Combine(Application.persistentDataPath, "saved textures");
        path = Path.Combine(path, textureID + ".png");

        Texture2D texture = new Texture2D(1, 1);

        if (!File.Exists(path))
        {
            // Get the texture from the texture url via one of two proxies.
            // Backup proxies: https://api.codetabs.com/v1/proxy?quest=
            //				   https://api.allorigins.win/raw?url=
            var www = UnityWebRequestTexture.GetTexture($"{textureLink}");
            yield return www.SendWebRequest();

            if (www.isHttpError && www.isNetworkError)
                ErrorHandler.Instance.Push(new Error { ErrorCode = 510, ErrorHeader = "Unable to get item texture", ErrorMessage = $"Item \"{currentHoldingItem.Name}\" has no texture, or no texture could be found. Please inform a developer." });
            else
            {
                while (!www.isDone)
                    yield return www;
                texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
        }

        // Create a new texture and load the saved image onto it
        texture.LoadImage(File.ReadAllBytes(path));

        itemTexture = ScalePointTexture(texture);
    }

    public Texture2D ScalePointTexture(Texture2D tex)
    {
        // Scale the texture to correct size
        if (tex.height != 64)
            tex = ScaleTexture(tex, 64, 64);

        // Cache the texture 
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    public Texture2D ScaleTexture(Texture2D tex, int x, int y)
    {
        Color[] pix = tex.GetPixels(0, 0, tex.width, tex.height);
        Texture2D destTex = new Texture2D(x, y);

        destTex.SetPixels(0, 32, tex.width, tex.height, pix);
        destTex.Apply();

        return destTex;
    }

    public void FillImage(Texture2D texture, Shader shader)
    {
        var tempMat = new Material(shader);
        tempMat.SetTexture("_MainTex", texture);

        var itemGameobject = Instantiate(textureImagePrefab, imageParent.transform);
        var meshRenderers = itemGameobject.GetComponentsInChildren<Renderer>(true);
        foreach (var mesh in meshRenderers)
            mesh.material = tempMat;
    }

    public Material GetSetMaterial(Shader shader)
    {
        var tempMat = new Material(shader);
        tempMat.mainTexture = new Texture2D(1, 1);

        var itemGameobject = Instantiate(textureImagePrefab, imageParent.transform);
        var meshRenderers = itemGameobject.GetComponentsInChildren<Renderer>(true);
        foreach (var mesh in meshRenderers)
            mesh.material = tempMat;

        return tempMat;
    }

    public void InstantiateImage(Sprite sprite, GameObject prefab)
    {
        ClearChildren(imageParent.transform);
        var itemGameobject = Instantiate(prefab, imageParent.transform);

        if (sprite != null)
            itemGameobject.GetComponent<Image>().sprite = sprite;
    }

    public void InstantiateImage(GameObject prefab, Color armorColor)
    {
        ClearChildren(imageParent.transform);

        if (prefab != null)
        {
            var itemGameobject = Instantiate(prefab, imageParent.transform);
            ColorArmorPiece(itemGameobject.transform, armorColor);
        }
    }

    public void ClearChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
            Destroy(parent.GetChild(i).gameObject);
    }

    public void OnMouseEnter()
    {
        if (currentHoldingItem.Name != "" && currentHoldingItem.Name != null && currentHoldingItem != null)
        {
            ClearTooltip();
            ActivateTooltip(true);
        }
    }

    public void OnMouseExit()
    {
        ActivateTooltip(false);
    }

    public virtual void ActivateTooltip(bool active)
    {
        if (active)
        {
            var newObj = Instantiate(Resources.Load<GameObject>("Description line"), tooltipHolder.transform);
            var finalDescription = currentHoldingItem.Name + StringListToString(currentHoldingItem.ItemDescription);
            newObj.transform.SetAsFirstSibling();

            newObj.GetComponent<Text>().text = finalDescription;
            tooltip.GetComponent<Tooltip>().update = true;
            if (currentHoldingItem.Backpack == new Backpack() || currentHoldingItem.Backpack == null || currentHoldingItem.Backpack.BackpackSize <= 0)
                ClearBackpack();
            tooltipHolder.SetActive(true);
        }
        else
            tooltipHolder.SetActive(false);

    }

    public void ClearBackpack()
    {
        if (backpackHolder != null)
        {
            ClearChildren(backpackHolder.transform);
            tooltip.GetComponent<Tooltip>().UpdateTooltipRect();
            backpackHolder.transform.parent.gameObject.SetActive(false);
        }
    }

    public void ClearTooltip()
    {
        for (int i = 0; i < tooltipHolder.transform.childCount; i++)
        {
            if (!tooltipHolder.transform.GetChild(i).name.Contains("IGNORE"))
            {
                Destroy(tooltipHolder.transform.GetChild(i).gameObject);
                tooltip.GetComponent<Tooltip>().UpdateTooltipRect();
            }
        }
    }

    public string StringListToString(List<string> list)
    {
        string newString = "\n";

        if (list != null && list != new List<string>())
            for (int i = 0; i < list.Count; i++)
                if (i == list.Count - 1)
                    newString += list[i];
                else
                    newString += $"{list[i]}\n";


        return newString;
    }

    public void ToggleActive()
    {
        Main.Instance.currentProfile.ActiveWeapon = currentHoldingItem;
        PlayerStats.ProcessStats(Main.Instance.currentProfile);
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            var childAnim = transform.parent.GetChild(i).GetComponent<Animator>();

            if (childAnim.gameObject == gameObject)
            {
                childAnim.SetBool("Enabled", !childAnim.GetBool("Enabled"));
                childAnim.SetTrigger("Pressed");
            }
            else
            {
                if (childAnim.GetBool("Enabled") == true)
                {
                    childAnim.SetBool("Enabled", false);
                    childAnim.SetTrigger("Pressed");
                }
            }
        }

        DamagePrediction.Instance.StartDamageCalculation(true);
    }

}
