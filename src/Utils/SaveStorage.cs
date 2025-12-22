using System;
using System.Collections.Generic;
using System.Text.Json;
using MelonLoader;
using UnityEngine;
using Il2Cpp;

namespace MdrgAiDialog.Utils;

[MonoSingleton]
[RegisterTypeInIl2Cpp]
public class SaveStorage : MonoBehaviour {
  public static SaveStorage Instance => MonoSingletonManager.Get<SaveStorage>();
  private static readonly Logger logger = new("SaveStorage");
  private static readonly GameVariables gameVariables = GameScript.Instance.GameVariables;

  private const string prefix = "MdrgAiDialog_";
  private readonly Dictionary<string, object> cache = [];

  public static EventBus EventBus { get; private set; } = new();

  public void SetValue<T>(string key, T value) {
    cache[key] = value;

    try {
      string fullKey = prefix + key;
      string json = JsonSerializer.Serialize(value);

      gameVariables.customData.SetStringSpecialVariable(fullKey, json);
      EventBus.Fire("value-changed", key);
    } catch (Exception e) {
      logger.LogError($"Error saving key {key}: {e.Message}");
    }
  }

  public void RemoveValue(string key) {
    cache.Remove(key);

    try {
      string fullKey = prefix + key;
      // game-side API does not provide explicit delete; null/empty usually acts as "unset"
      gameVariables.customData.SetStringSpecialVariable(fullKey, null);
      EventBus.Fire("value-changed", key);
    } catch (Exception e) {
      logger.LogError($"Error removing key {key}: {e.Message}");
    }
  }

  public T GetValue<T>(string key, T defaultValue = default) {
    if (cache.TryGetValue(key, out var cachedValue)) {
      if (cachedValue == null) {
        return default;
      }
      if (cachedValue is T typedValue) {
        return typedValue;
      }
      logger.LogWarning($"Cache type mismatch for key '{key}'. Reloading from save.");
      cache.Remove(key);
    }

    try {
      string fullKey = prefix + key;
      string json = gameVariables.customData.GetStringSpecialVariableOrDefault(fullKey);

      if (!string.IsNullOrEmpty(json)) {
        T value = JsonSerializer.Deserialize<T>(json);
        cache[key] = value;
        return value;
      }
    } catch (Exception e) {
      logger.LogError($"Error loading key {key}: {e.Message}");
    }

    return defaultValue;
  }

  public void InvalidateCache() {
    cache.Clear();
    logger.Log("Cache invalidated");
    EventBus.Fire("cache-invalidated");
  }
}
