﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GorillaNetworking;
using Utilla.Models;
using HarmonyLib;
using TMPro;
using UnityEngine.Events;
using Utilla.Tools;
using GorillaGameModes;

namespace Utilla
{
    public class UtillaGamemodeSelector : MonoBehaviour
	{
		// Layout

        public GameModeSelectorButtonLayout Layout => GetComponent<GameModeSelectorButtonLayout>();
		private GameModeSelectorButtonLayoutData LayoutData => (GameModeSelectorButtonLayoutData)AccessTools.Field(typeof(GameModeSelectorButtonLayout), "data").GetValue(Layout);
        private ModeSelectButtonInfoData[] LayoutButtonInfo => LayoutData.Info;

        private ModeSelectButton[] modeSelectButtons = [];
        private static GameObject fallbackTemplateButton = null;

        // Pages

        private int PageCount => GamemodeManager.HasInstance ? Mathf.CeilToInt(GamemodeManager.Instance.Gamemodes.Count() / (float)Constants.PageSize) : Constants.PageSize;
        private static int currentPage;

        public void Awake()
		{
			modeSelectButtons = GetComponentsInChildren<ModeSelectButton>(true).Take(Constants.PageSize).ToArray();

            foreach (var mb in modeSelectButtons)
			{
				TMP_Text gamemodeTitle = (TMP_Text)AccessTools.Field(mb.GetType(), "gameModeTitle").GetValue(mb);

				gamemodeTitle.enableAutoSizing = true;
				gamemodeTitle.fontSizeMax = gamemodeTitle.fontSize;
				gamemodeTitle.fontSizeMin = 0f;
                gamemodeTitle.transform.localPosition = new Vector3(gamemodeTitle.transform.localPosition.x, 0f, gamemodeTitle.transform.localPosition.z + 0.08f);
			}

            CreatePageButtons(modeSelectButtons.First().gameObject);

			if (GamemodeManager.HasInstance && GamemodeManager.Instance.Gamemodes != null)
			{
				Logging.Log($"Current page of the GamemodeSelector is set to {currentPage}.");
                ShowPage(currentPage);
            }
		}

		public void GetSelectorGamemodes(out List<Gamemode> baseGamemodes, out List<Gamemode> moddedGamemodes)
		{
            baseGamemodes = [];
            moddedGamemodes = [];

            if (modeSelectButtons == null || modeSelectButtons.Length == 0 || !Layout) return;

            for (int i = 0; i < modeSelectButtons.Length; i++)
			{
                ModeSelectButtonInfoData info = LayoutButtonInfo[i];
                baseGamemodes.Add(info);
            }

			foreach(Gamemode bm in baseGamemodes)
			{
				string baseMode = bm.BaseGamemode;
                string moddedTitle = Enum.Parse<GameModeType>(baseMode) == GameModeType.Infection ? "MODDED" : $"MODDED {baseMode.ToUpper()}";
                moddedGamemodes.Add(new Gamemode($"MODDED_{baseMode.ToUpper()}", moddedTitle, baseMode));
            }
        }

		void CreatePageButtons(GameObject templateButton)
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.SetActive(false);
			MeshFilter meshFilter = cube.GetComponent<MeshFilter>();

			GameObject CreatePageButton(string text, Action onPressed)
			{
				// button creation
				GameObject button = Instantiate(templateButton.transform.childCount == 0 ? fallbackTemplateButton : templateButton);

				// button appearence
				button.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
				button.GetComponent<Renderer>().material = templateButton.GetComponent<GorillaPressableButton>().unpressedMaterial;

				// button location
				button.transform.parent = templateButton.transform.parent;
				button.transform.localRotation = templateButton.transform.localRotation;
				button.transform.localScale = Vector3.one * 0.1427168f; // shouldn't hurt anyone for now 
				button.transform.GetChild(0).gameObject.SetActive(true);

				// legacy text (to use)
				Text buttonText = button.GetComponentInChildren<Text>();
				if (buttonText)
				{
					buttonText.text = text;
					buttonText.transform.localScale = Vector3.Scale(buttonText.transform.localScale, new Vector3(2, 2, 1));
				}

				// text mesh pro (to exile)
				TMP_Text tmpText = button.transform.Find("Title")?.GetComponent<TMP_Text>() ?? button.GetComponentInChildren<TMP_Text>();
				if (tmpText) tmpText.gameObject.SetActive(false);

				// button behaviour
				Destroy(button.GetComponent<ModeSelectButton>());
                var unityEvent = new UnityEvent();
                unityEvent.AddListener(new UnityAction(onPressed));
                button.AddComponent<GorillaPressableButton>().onPressButton = unityEvent;

				return button;
			}

			GameObject nextPageButton = CreatePageButton("-->", NextPage);
			nextPageButton.transform.localPosition = new Vector3(-0.745f, nextPageButton.transform.position.y + 0.005f, nextPageButton.transform.position.z - 0.03f);

			GameObject previousPageButton = CreatePageButton("<--", PreviousPage);
			previousPageButton.transform.localPosition = new Vector3(-0.745f, -0.633f, previousPageButton.transform.position.z - 0.03f);

			Destroy(cube);

			if (templateButton.transform.childCount != 0)
			{
				fallbackTemplateButton = templateButton;
			}
		}

		public void NextPage()
		{
			if (currentPage < PageCount - 1)
			{
				ShowPage(currentPage + 1);
			}
		}

		public void PreviousPage()
		{
			if (currentPage > 0)
			{
				ShowPage(currentPage - 1);
			}
		}

		public void ShowPage(int page)
		{
            currentPage = page;

			List<Gamemode> currentGamemodes = GamemodeManager.Instance.Gamemodes.Skip(page * Constants.PageSize).Take(Constants.PageSize).ToList();

			for (int i = 0; i < modeSelectButtons.Length; i++)
			{
				if (i < currentGamemodes.Count)
				{
					Gamemode customMode = currentGamemodes[i];
		
                    modeSelectButtons[i].enabled = true;

					modeSelectButtons[i].SetInfo(LayoutButtonInfo.FirstOrDefault(i => i.Mode == customMode.ID) ?? customMode);
				}
				else
				{
					modeSelectButtons[i].enabled = false;

					modeSelectButtons[i].SetInfo(new Gamemode("", ""));
				}
			}

            GorillaComputer.instance.OnModeSelectButtonPress(GorillaComputer.instance.currentGameMode.Value, GorillaComputer.instance.leftHanded);
        }
	}
}