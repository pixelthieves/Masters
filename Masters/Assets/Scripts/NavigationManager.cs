﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class NavigationManager : MonoBehaviour
    {

        public GameObject startScreen;
        public GameObject howToPlayScreen;
        public GameObject chooseOpponentScene;

        private GameObject[] allScenes;

        private void Start()
        {
            allScenes = new[] {startScreen, howToPlayScreen, chooseOpponentScene};
            setScene(startScreen);

            Invoke("howToPlay", 1);
        }

        private void setScene(GameObject newScene)
        {
            foreach (var scene in allScenes)
            {
                scene.SetActive(scene == newScene);
            }
        }

        private void howToPlay()
        {
            setScene(howToPlayScreen);
        }

        public void chooseOponnetn()
        {
            setScene(chooseOpponentScene);
        }

        public void StartScene()
        {
            SceneManager.LoadScene("FightScene");
        }
    }
}