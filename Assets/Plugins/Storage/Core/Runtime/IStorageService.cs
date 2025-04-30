using System;
using System.Collections.Generic;

namespace Plugins.Storage.Core.Runtime
{
    public interface IStorageService
    {
        public StorageType DefaultType { get; }
        
        public bool IsSupported(StorageType storageType);
        public bool IsAvailable(StorageType storageType);
        
        void Get(string key, Action<bool, string> onComplete, StorageType? storageType = null);
        void Get(List<string> keys, Action<bool, List<string>> onComplete, StorageType? storageType = null);
        
        void Set(string key, string value, Action<bool> onComplete = null, StorageType? storageType = null);
        void Set(string key, int value, Action<bool> onComplete = null, StorageType? storageType = null);
        void Set(string key, bool value, Action<bool> onComplete = null, StorageType? storageType = null);
        void Set(List<string> keys, List<object> values, Action<bool> onComplete = null, StorageType? storageType = null);
        
        void Remove(string key, Action<bool> onComplete = null, StorageType? storageType = null);
        void Remove(List<string> keys, Action<bool> onComplete = null, StorageType? storageType = null);
    }
}