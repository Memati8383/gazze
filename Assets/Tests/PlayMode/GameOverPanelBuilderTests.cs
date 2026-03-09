using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Gazze.UI;

public class GameOverPanelBuilderTests
{
    [Test]
    public void Build_CreatesPanelWithButtonsAndTexts()
    {
        var data = new GameOverPanelBuilder.Data
        {
            score = 1234,
            highScore = 2345,
            level = 3,
            playTimeSeconds = 95f,
            achievements = new List<string> { "Test1", "Test2" }
        };
        GameObject root = GameOverPanelBuilder.Build(data);
        Assert.NotNull(root);
        var panel = root.transform.Find("GameOverPanel");
        Assert.NotNull(panel);
        var buttons = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        Assert.GreaterOrEqual(buttons.Length, 4);
        var tmps = panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        Assert.Greater(tmps.Length, 4);
        Object.DestroyImmediate(root);
    }
}
