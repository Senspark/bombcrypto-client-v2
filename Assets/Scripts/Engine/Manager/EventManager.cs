using System;
using System.Collections.Generic;

using UnityEngine.Events;

namespace Engine.Manager {
    public enum InputEvent {
        Unknown,
        
        //Kiểm tra user đang nhấn phím hay dùng controller
        OnChangeInput,
        //Kiểm tra user đang dùng controller loại nào
        OnchangeController,
        
        RB,
        RT,
        LB,
        LT,
        A,
        B,
        X,
        Y,
        Share,
        Options,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        LStickButton,
        RStickButton,
        LeftStickX,
        LeftStickY,
        DPadX,
        DPadY,
        LStickLeft,
        LStickRight,
        LStickUp,
        LStickDown,
    }
    public enum PlayerEvent {
        OnDamage,
    }
    public static class EventManager<T> {
        private static readonly Dictionary<Enum, UnityEvent<T>> Event = new();
        private static readonly Dictionary<Enum, UnityEvent<T>> UniqueEvent = new();

        /// <summary>
        /// Thêm tác vụ cần gọi vào sự kiện.
        /// </summary>
        /// <param name="id">Loại sự kiện.</param>
        /// <param name="param">Tác vụ.</param>
        public static void Add(Enum id, UnityAction<T> param) {
            if (!Event.ContainsKey(id)) {
                UnityEvent<T> newEvent = new UnityEvent<T>();
                Event.Add(id, newEvent);
            }

            Event[id].AddListener(param);
        }
        
        //Chỉ có 1 listener cho event này
        public static void AddUnique(Enum id, UnityAction<T> param) {
            if (!UniqueEvent.TryGetValue(id, out var value)) {
                UnityEvent<T> newEvent = new UnityEvent<T>();
                UniqueEvent.Add(id, newEvent);
            }
            else {
                value.RemoveAllListeners();
            }

            UniqueEvent[id].AddListener(param);
        }
        /// <summary>
        /// Xóa tác vụ khỏi thư viện.
        /// </summary>
        /// <param name="id">Loại sự kiện.</param>
        /// <param name="param">Tác vụ.</param>
        public static void Remove(Enum id, UnityAction<T> param) {
            if (Event.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }
        
        public static void RemoveUnique(Enum id, UnityAction<T> param) {
            if (UniqueEvent.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }

        /// <summary>
        /// Triển khai sự kiện.
        /// </summary>
        /// <param name="id">ID sự kiện cần triển khai.</param>
        /// <param name="value"></param>
        public static void Dispatcher(Enum id, T value) {
            if (Event.TryGetValue(id, out var unityEvent)) {
                unityEvent.Invoke(value);
            }
            if (UniqueEvent.TryGetValue(id, out var uniqueEvent)) {
                uniqueEvent.Invoke(value);
            }
        }
        
    }
    public static class EventManager {
        private static readonly Dictionary<Enum, UnityEvent<object>> EventObject = new();
        private static readonly Dictionary<Enum, UnityEvent> Event = new();
        private static readonly Dictionary<Enum, UnityEvent<object>> UniqueEventObject = new();
        private static readonly Dictionary<Enum, UnityEvent> UniqueEvent = new();

        /// <summary>
        /// Thêm tác vụ cần gọi vào sự kiện.
        /// </summary>
        /// <param name="id">Loại sự kiện.</param>
        /// <param name="param">Tác vụ.</param>
        public static void Add(Enum id, UnityAction<object> param) {
            if (!EventObject.ContainsKey(id)) {
                UnityEvent<object> newEvent = new UnityEvent<object>();
                EventObject.Add(id, newEvent);
            }

            EventObject[id].AddListener(param);
        }
        public static void Add(Enum id, UnityAction param) {
            if (!Event.ContainsKey(id)) {
                UnityEvent newEvent = new UnityEvent();
                Event.Add(id, newEvent);
            }

            Event[id].AddListener(param);
        }
        //Chỉ có 1 listener cho event này
        public static void AddUnique(Enum id, UnityAction param) {
            if (!UniqueEvent.TryGetValue(id, out var value)) {
                UnityEvent newEvent = new UnityEvent();
                UniqueEvent.Add(id, newEvent);
            } else {
                value.RemoveAllListeners();
            }

            Event[id].AddListener(param);
        }
        public static void AddUnique(Enum id, UnityAction<object> param) {
            if (!UniqueEventObject.TryGetValue(id, out var value)) {
                UnityEvent<object> newEvent = new UnityEvent<object>();
                UniqueEventObject.Add(id, newEvent);
            } else {
                value.RemoveAllListeners();
            }

            UniqueEventObject[id].AddListener(param);
        }
        /// <summary>
        /// Xóa tác vụ khỏi thư viện.
        /// </summary>
        /// <param name="id">Loại sự kiện.</param>
        /// <param name="param">Tác vụ.</param>
        public static void Remove(Enum id, UnityAction param) {
            if (Event.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }
        public static void RemoveUnique(Enum id, UnityAction param) {
            if (UniqueEvent.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }
        
        public static void Remove(Enum id, UnityAction<object> param) {
            if (EventObject.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }
        public static void RemoveUnique(Enum id, UnityAction<object> param) {
            if (UniqueEventObject.TryGetValue(id, out var unityEvent)) {
                unityEvent.RemoveListener(param);
            }
        }

        /// <summary>
        /// Triển khai sự kiện.
        /// </summary>
        /// <param name="id">ID sự kiện cần triển khai.</param>
        public static void Dispatcher(Enum id) {
            if (Event.TryGetValue(id, out var unityEvent)) {
                unityEvent.Invoke();
            }
            if (UniqueEvent.TryGetValue(id, out var uniqueEvent)) {
                uniqueEvent.Invoke();
            }
        }
        public static void Dispatcher(Enum id, object value) {
            if (EventObject.TryGetValue(id, out var unityEvent)) {
                unityEvent.Invoke(value);
            }
            if (UniqueEventObject.TryGetValue(id, out var uniqueEvent)) {
                uniqueEvent.Invoke(value);
            }
        }
        
    }
}
