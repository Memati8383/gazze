using NUnit.Framework;
using UnityEngine;
using Settings;

namespace Settings.Tests
{
    public class SettingsTests
    {
        [SetUp]
        public void Setup()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Model_InitializesWithDefaultValues()
        {
            var model = new SettingsModel();
            Assert.AreEqual(1f, model.MusicVolume);
            Assert.AreEqual(1f, model.SFXVolume);
            Assert.IsTrue(model.MusicEnabled);
            Assert.IsTrue(model.SFXEnabled);
        }

        [Test]
        public void Model_SavesAndLoadsMusicVolume()
        {
            var model = new SettingsModel();
            model.SetMusicVolume(0.5f);
            
            var newModel = new SettingsModel();
            Assert.AreEqual(0.5f, newModel.MusicVolume);
        }

        [Test]
        public void Model_ClampsVolumeValues()
        {
            var model = new SettingsModel();
            
            model.SetMusicVolume(1.5f);
            Assert.AreEqual(1f, model.MusicVolume);
            
            model.SetMusicVolume(-0.5f);
            Assert.AreEqual(0f, model.MusicVolume);
        }

        [Test]
        public void Model_TogglesAudioCorrectly()
        {
            var model = new SettingsModel();
            model.SetMusicEnabled(false);
            Assert.IsFalse(model.MusicEnabled);
            
            var newModel = new SettingsModel();
            Assert.IsFalse(newModel.MusicEnabled);
        }
    }
}