#undef ENABLE_INPUT_SYSTEM // TODO: the new input system support is not ready
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Input state from camera navigation.
/// </summary>
public class CameraInput
{
    public readonly Mouse mouse = new Mouse();
    public readonly Touches oneTouch = new Touches(1);
    public readonly Touches twoTouches = new Touches(2);

    public IPointer GetPointer(PointerType pointerType)
    {
        return pointerType == PointerType.Mouse ? mouse : oneTouch;
    }

    public enum PointerType
    {
        Mouse = 0,
        Touch = 1
    }

    public interface IPointer
    {
        public bool IsStarted { get; }
        public float3 Position { get; }
        public float3 BeginPos { get; }
        public float3 LastPos { get; }
    }

    public class Mouse : IPointer
    {
        public bool IsStarted { get; private set; }
        public bool IsUsed { get; private set; }
        public float Scroll { get; private set; }
        public int Index { get; private set; }
        public float3 Position { get; private set; }
        public float3 BeginPos { get; private set; }
        public float3 LastPos { get; private set; }

        public bool CanUse(int buttonIndex)
        {
            return IsStarted && !IsUsed && Index == buttonIndex;
        }

        public void Use(int buttonIndex)
        {
            if (Index == buttonIndex)
            {
                IsUsed = true;
            }
        }

        public void Update()
        {
            if (IsStarted)
            {
                LastPos = Position;
            }
            Scroll = Input.mouseScrollDelta.y;
            Position = Input.mousePosition;

            for (int mouseButtonIndex = 0; mouseButtonIndex < 3; ++mouseButtonIndex)
            {
                if (Input.GetMouseButtonDown(mouseButtonIndex)
                    && EventSystem.current.currentSelectedGameObject == null) // skip MouseButtonDown when UI is being interacted with
                {
                    IsStarted = true;
                    IsUsed = false;
                    Index = mouseButtonIndex;
                    BeginPos = LastPos = Position;
                    break;
                }
            }

            if (IsStarted && Input.GetMouseButtonUp(Index))
            {
                IsStarted = false;
            }
        }
    }

    public class Touches : IPointer
    {
        public bool IsStarted { get; private set; }
        public bool IsUsed { get; private set; }
        public int[] Indexes { get; private set; }
        public float3[] Position { get; private set; }
        public float3[] BeginPos { get; private set; }
        public float3[] LastPos { get; private set; }
        public float3 Center => GetCenter(Position);
        public float3 BeginCenter => GetCenter(BeginPos);
        public float3 LastCenter => GetCenter(LastPos);
        public float3 Diff => GetDiff(Position);
        public float3 BeginDiff => GetDiff(BeginPos);
        public float3 LastDiff => GetDiff(LastPos);

        float3 IPointer.Position => Position[0];
        float3 IPointer.BeginPos => BeginPos[0];
        float3 IPointer.LastPos => LastPos[0];

        private int TouchCount { get; set; }

        public Touches(int touchCount)
        {
            TouchCount = touchCount;
            Indexes = new int[touchCount];
            Position = new float3[touchCount];
            BeginPos = new float3[touchCount];
            LastPos = new float3[touchCount];
        }

        public bool CanUse()
        {
            return IsStarted && !IsUsed;
        }

        public void Use()
        {
            IsUsed = true;
        }

        public void Update()
        {
            if (Input.touchCount == TouchCount
                && EventSystem.current.currentSelectedGameObject == null) // skip MouseButtonDown when UI is being interacted with)
            {
                if (IsStarted)
                {
                    for (int i = 0; i < TouchCount; ++i)
                    {
                        LastPos[i] = Position[i];
                        Position[i] = (Vector3)Input.GetTouch(i).position;
                    }
                }
                else
                {
                    IsStarted = true;
                    IsUsed = false;

                    for (int i = 0; i < TouchCount; ++i)
                    {
                        BeginPos[i] = LastPos[i] = Position[i] = (Vector3)Input.GetTouch(i).position;
                    }
                }
            }
            else
            {
                IsStarted = false;
            }
        }

        private static float3 GetCenter(float3[] points)
        {
            float3 center = points[0];

            for (int i = 1, count = points.Length; i < count; ++i)
            {
                center += points[i];
            }

            return center / points.Length;
        }

        private static float3 GetDiff(float3[] points)
        {
            var count = points.Length;
            return points[points.Length - 1] - points[0];
        }
    }

#if ENABLE_INPUT_SYSTEM
    private PointerControls controls;
#endif

    public bool GetKey(KeyCode key)
    {
        return Input.GetKey(key);
    }

    public bool AnyKey(IEnumerable<KeyCode> keys)
    {
        return keys.Any(key => Input.GetKey(key));
    }

    public void Initialize()
    {
#if ENABLE_INPUT_SYSTEM
        controls = new PointerControls();

        controls.pointer.pan.started += _ => StartDragging(0);
        controls.pointer.pan.canceled += _ => StopDragging(0);
        controls.pointer.zoom.started += _ => StartDragging(1);
        controls.pointer.zoom.canceled += _ => StopDragging(1);
        controls.pointer.orbit.started += _ => StartDragging(2);
        controls.pointer.orbit.canceled += _ => StopDragging(2);
        controls.pointer.touch1Contact.started += _ => StartTwoFingersDragging();
        controls.pointer.touch1Contact.canceled += _ => StopTwoFIngersDragging();
        controls.pointer.touch0Contact.canceled += _ => StopTwoFIngersDragging();

        controls.Enable();
#else
        Input.simulateMouseWithTouches = false;
#endif
    }

    public void Update()
    {
        mouse.Update();
        oneTouch.Update();
        twoTouches.Update();

#if ENABLE_INPUT_SYSTEM
        MouseScroll = controls.pointer.scroll.ReadValue<float>();
        MousePositionSS = (Vector3)controls.pointer.pointerPosition.ReadValue<Vector2>();

        if (IsTwoFingersDragging)
        {
            touch0LastPositionSS = touch0PositionSS;
            touch1LastPositionSS = touch1PositionSS;
            touch0PositionSS = (Vector3)controls.pointer.touch0Position.ReadValue<Vector2>();
            touch1PositionSS = (Vector3)controls.pointer.touch1Position.ReadValue<Vector2>();
        }
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private void StartDragging(int draggingIndex)
    {
        // skip MouseButtonDown when UI is being interacted with
        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        IsDragging = true;
        IsDraggingUsed = false;
        DraggingIndex = draggingIndex;
        BeginDragPositionSS = MousePositionSS;
    }

    private void StopDragging(int draggingIndex)
    {
        if (IsDragging && draggingIndex == DraggingIndex)
        {
            IsDragging = false;
        }
    }

    private void StartTwoFingersDragging()
    {
        IsDragging = false;
        IsDraggingUsed = true;
        IsTwoFingersDragging = true;
        touch0BeginPositionSS = touch0LastPositionSS = touch0PositionSS = (Vector3)controls.pointer.touch0Position.ReadValue<Vector2>();
        touch1BeginPositionSS = touch1LastPositionSS = touch1PositionSS = (Vector3)controls.pointer.touch1Position.ReadValue<Vector2>();
    }

    private void StopTwoFIngersDragging()
    {
        IsTwoFingersDragging = false;
    }
#endif
}