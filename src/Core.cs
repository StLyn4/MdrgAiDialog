using System;
using MelonLoader;
using UnityEngine;
using MdrgAiDialog.Chat;
using MdrgAiDialog.Utils;

[assembly: MelonInfo(typeof(MdrgAiDialog.Core), "MdrgAiDialog", "0.4.0", "Delta", null)]
[assembly: MelonGame("IncontinentCell", "My Dystopian Robot Girlfriend")]

namespace MdrgAiDialog;

public class Core : MelonMod {
  /// <summary>
  /// Gets the singleton instance of the plugin
  /// </summary>
  public static Core Instance { get; private set; }

  /// <summary>
  /// Gets the root GameObject of the mod
  /// </summary>
  public GameObject RootObject { get; private set; }

  public override void OnInitializeMelon() {
    // Make this instance available to other classes
    Instance = this;

    // Load config
    ModConfig.Load();

    // Create Root GameObject
    RootObject = new GameObject(Guid.NewGuid().ToString());
    UnityEngine.Object.DontDestroyOnLoad(RootObject);

    // Add singletons
    MonoSingletonManager.Add<MainThreadRunner>();
    MonoSingletonManager.Add<SaveStorage>();
    MonoSingletonManager.Add<ChatManager>();
    MonoSingletonManager.Add<ChatWriter>();
  }
}
