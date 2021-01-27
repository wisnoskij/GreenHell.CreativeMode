using HarmonyLib;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreativeMode : Mod
{
	private const string ModName = "creativemode";
	private const string HarmonyId = "com.wisnoski.greenhell." + ModName;
	Harmony instance;
	public void Start()
	{
		instance = new Harmony(HarmonyId);
		instance.PatchAll(Assembly.GetExecutingAssembly());
		Debug.Log(string.Format("Mod {0} has been loaded!", ModName));
	}
	
	[ConsoleCommand("/buildall", "Build All Ghosts")]
	public void OnModUnload()
	{
		GameObject InGameMenu = GameObject.Find("InGameMenu");
        if (InGameMenu != null && InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("Build") != null)
        {
            Destroy(InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("Build").gameObject);
        }
        instance.UnpatchAll();
		Debug.Log(string.Format("Mod {0} has been unloaded!", ModName));
	}
	
	public static void build()
	{
		(Traverse.Create(ConstructionGhostManager.Get()).Field("m_AllGhosts").GetValue() as List<ConstructionGhost>).ForEach((b) => Traverse.Create(b).Field("m_CurrentStep").SetValue(999));
	}
}

[HarmonyPatch(typeof(ConstructionGhost))]
[HarmonyPatch("UpdateState")]
internal class Patch_ConstructionGhost_UpdateState
{
	static void Prefix(ConstructionGhost __instance)
	{
		if((int) Traverse.Create(__instance).Field("m_State").GetValue() == 1){
			Traverse.Create(__instance).Field("m_CurrentStep").SetValue(999);
		}
	}
}

[HarmonyPatch(typeof(MenuInGame))]
[HarmonyPatch("OnShow")]
internal class Patch_MenuInGame_AddButton
{
	public static void Prefix(MenuInGame __instance)
	{
		GameObject InGameMenu = GameObject.Find("InGameMenu");
		if (InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("Build") == null)
        {
			GameObject btn = GameObject.Instantiate(InGameMenu.transform.Find("MenuInGame").Find("Buttons").Find("Resume").gameObject, InGameMenu.transform.Find("MenuInGame").Find("Buttons"));
			btn.name = "Build";
			btn.GetComponent<UIButtonEx>().onClick.AddListener(CreativeMode.build);
			InGameMenu.transform.Find("MenuInGame").GetComponent<MenuInGame>().AddMenuButton(btn.GetComponent<UIButtonEx>(), "Build All");
			btn.GetComponentInChildren<Text>().text = "Build All";
		}
	}
}

