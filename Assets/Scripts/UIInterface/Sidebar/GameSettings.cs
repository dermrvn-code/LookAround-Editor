using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameSettings : MonoBehaviour
{

    public ToggleInput toggle;
    public SliderAndInput pieces;
    public SpriteSelector sprite;
    public ActionSettings actionSettings;


    SpriteManager spriteManager;
    PuzzleManager puzzleManager;
    void Awake()
    {
        spriteManager = FindFirstObjectByType<SpriteManager>();
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
    }

    public void Save()
    {
        puzzleManager.isEnabled = toggle.value;
        puzzleManager.ResetPuzzle();
        if (toggle.value)
        {
            puzzleManager.SetupPuzzle(int.Parse(sprite.value), pieces.value, actionSettings.GenerateAction());
            puzzleManager.RestartPuzzle();
        }
        InfoText.ShowInfo("Spiele wurden gespeichert.");
    }

    public void Initialize(bool isActive, int pieces, int selectedSprite, string actionString)
    {
        toggle.Initialize(isActive);

        if (pieces != 0) this.pieces.Initialize(pieces);

        Pairs.SpritePair[] spritePairs = new Pairs.SpritePair[spriteManager.sceneSprites.Length];
        for (int i = 0; i < spriteManager.sceneSprites.Length; i++)
        {
            SpriteData sprite = spriteManager.sceneSprites[i];

            if (sprite.texture == null) continue;

            spritePairs[i] = new Pairs.SpritePair();
            spritePairs[i].sprite = Sprite.Create(
                sprite.texture,
                new Rect(0, 0, sprite.texture.width, sprite.texture.height),
                new Vector2(0.5f, 0.5f)
            );
            spritePairs[i].value = sprite.index.ToString();

        }

        sprite.Initialize(spritePairs, selectedSprite.ToString());
        actionSettings.Initialize(actionString);
    }
}
