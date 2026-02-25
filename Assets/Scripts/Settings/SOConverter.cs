using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SOConverter<T> : CustomCreationConverter<T> where T : ScriptableObject
{
    public override T Create(Type aObjectType)
    {
        if (typeof(T).IsAssignableFrom(aObjectType))
        {
            // Create the ScriptableObject instance
            T instance = (T)ScriptableObject.CreateInstance(aObjectType);

            // Destroy the instance immediately to prevent OnEnable from being called
            if(Application.isPlaying)
                UnityEngine.Object.Destroy(instance);

            return instance;
        }
        return null;
    }
}

public class GameSettingsConverter : JsonConverter<GameSettings>
{
    private readonly GameSettings defaults;

    public GameSettingsConverter(GameSettings defaults)
    {
        this.defaults = defaults;
    }

    public override GameSettings ReadJson(
        JsonReader reader,
        Type objectType,
        GameSettings existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        // Start from defaults
        GameSettings instance = ScriptableObject.CreateInstance<GameSettings>();
        GameSettings.CopySettings(instance, defaults);

        // Overwrite only what exists in the save
        serializer.Populate(obj.CreateReader(), instance);

        if (Application.isPlaying)
            UnityEngine.Object.Destroy(instance);

        return instance;
    }

    public override void WriteJson(JsonWriter writer, GameSettings value, JsonSerializer serializer)
    {
        JObject.FromObject(value, serializer).WriteTo(writer);
    }

}
