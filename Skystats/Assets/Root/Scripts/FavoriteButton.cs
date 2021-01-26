using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FavoriteButton : MonoBehaviour
{
    public Image image;
    public Sprite added, notAdded;
    
    private void Update()
    {
        image.sprite = Main.Instance.favoriteUsernames.Any(x => x.ToLower() == Main.Instance.username.ToLower()) ? added : notAdded;
    }

    public void ChangeIcon()
    {
        image.sprite = image.sprite == added ? notAdded : added;
    }

    public void ToggleFavorite()
    {
        if (Main.Instance.favoriteUsernames.Any(x => x.ToLower() == Main.Instance.username.ToLower()))
            Main.Instance.RemoveFavorite(Main.Instance.username);
        else
            Main.Instance.AddFavoriteCurrentUser();
    }
    
}
