using BiliLite.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tomlyn;
using Tomlyn.Model;

public class LocalObjectStorageHelper
{
    private readonly IObjectStorageHelper m_storage;

    public LocalObjectStorageHelper(string fileName = "settings.toml")
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        m_storage = new ApplicationDataObjectStorageHelper();
        //m_storage = new DbContextObjectStorageHelper();
    }

    public bool KeyExists(string key)
    {
        return m_storage.KeyExists(key);
    }

    public T Read<T>(string key)
    {
        return m_storage.Read<T>(key);
    }

    public void Save<T>(string key, T value)
    {
        m_storage.Save(key, value);
    }

    public void Remove(string key)
    {
        m_storage.Remove(key);
    }

    public void Clear()
    {
        m_storage.Clear();
    }
}

