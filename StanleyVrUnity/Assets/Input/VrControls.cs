// GENERATED AUTOMATICALLY FROM 'Assets/Input/VrControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @VrControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @VrControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""VrControls"",
    ""maps"": [
        {
            ""name"": ""ActionMap"",
            ""id"": ""202e0b20-e0a6-4cf7-9db5-9a42e20dbbec"",
            ""actions"": [
                {
                    ""name"": ""RightHandPose"",
                    ""type"": ""Value"",
                    ""id"": ""bb7cae79-288e-4de4-a0fa-08d2670849b8"",
                    ""expectedControlType"": ""Pose"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""LeftHandPose"",
                    ""type"": ""Button"",
                    ""id"": ""bdee6a18-4f75-49ab-8c1d-7fbd6057d0ef"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d620a98a-61f8-48d1-a370-db86319c5121"",
                    ""path"": ""<XRController>{RightHand}/devicePose"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightHandPose"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""42579ed7-e8c1-41a0-ba2d-b9eb6747126b"",
                    ""path"": ""<XRController>{LeftHand}/devicePose"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftHandPose"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // ActionMap
        m_ActionMap = asset.FindActionMap("ActionMap", throwIfNotFound: true);
        m_ActionMap_RightHandPose = m_ActionMap.FindAction("RightHandPose", throwIfNotFound: true);
        m_ActionMap_LeftHandPose = m_ActionMap.FindAction("LeftHandPose", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // ActionMap
    private readonly InputActionMap m_ActionMap;
    private IActionMapActions m_ActionMapActionsCallbackInterface;
    private readonly InputAction m_ActionMap_RightHandPose;
    private readonly InputAction m_ActionMap_LeftHandPose;
    public struct ActionMapActions
    {
        private @VrControls m_Wrapper;
        public ActionMapActions(@VrControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @RightHandPose => m_Wrapper.m_ActionMap_RightHandPose;
        public InputAction @LeftHandPose => m_Wrapper.m_ActionMap_LeftHandPose;
        public InputActionMap Get() { return m_Wrapper.m_ActionMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ActionMapActions set) { return set.Get(); }
        public void SetCallbacks(IActionMapActions instance)
        {
            if (m_Wrapper.m_ActionMapActionsCallbackInterface != null)
            {
                @RightHandPose.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRightHandPose;
                @RightHandPose.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRightHandPose;
                @RightHandPose.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnRightHandPose;
                @LeftHandPose.started -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnLeftHandPose;
                @LeftHandPose.performed -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnLeftHandPose;
                @LeftHandPose.canceled -= m_Wrapper.m_ActionMapActionsCallbackInterface.OnLeftHandPose;
            }
            m_Wrapper.m_ActionMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @RightHandPose.started += instance.OnRightHandPose;
                @RightHandPose.performed += instance.OnRightHandPose;
                @RightHandPose.canceled += instance.OnRightHandPose;
                @LeftHandPose.started += instance.OnLeftHandPose;
                @LeftHandPose.performed += instance.OnLeftHandPose;
                @LeftHandPose.canceled += instance.OnLeftHandPose;
            }
        }
    }
    public ActionMapActions @ActionMap => new ActionMapActions(this);
    public interface IActionMapActions
    {
        void OnRightHandPose(InputAction.CallbackContext context);
        void OnLeftHandPose(InputAction.CallbackContext context);
    }
}
