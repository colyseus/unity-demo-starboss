// GENERATED AUTOMATICALLY FROM 'Assets/_Project/Input/InputActions_Player.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputActions_Player : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActions_Player()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions_Player"",
    ""maps"": [
        {
            ""name"": ""Spaceship"",
            ""id"": ""ffc58a2c-1fa4-47d5-911c-44629a13873d"",
            ""actions"": [
                {
                    ""name"": ""Steering"",
                    ""type"": ""Value"",
                    ""id"": ""21bc478a-47a6-47cf-beae-dd07d8dd7afb"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Thrust"",
                    ""type"": ""Value"",
                    ""id"": ""b6eaabdf-4ff0-41ca-a841-0199bd328ffa"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Brake"",
                    ""type"": ""Button"",
                    ""id"": ""cc1bf01e-14e3-4f9b-b55b-520b0d7af938"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Button"",
                    ""id"": ""a9a57def-ef4f-4122-ae8f-460518ac6a20"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""3601e876-74ce-47a6-b7c5-47362b018891"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""181fa4f3-fbd1-4a95-a903-fa9aa380c437"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""9cf1b866-677e-434a-9265-f5adabc2f1d1"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""61c548c2-7848-4d81-b8d0-93a8b94231e3"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""2fc76659-fd0b-4de7-b90c-6b8c8ae5354e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""627f19f1-dc9e-48e6-9ebb-23bdf76cc120"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepads"",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dce0b737-948a-4e17-9130-f6f0ae64fd2a"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Thrust"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3363869c-0775-42cc-8a5b-68a5aa75c3df"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepads"",
                    ""action"": ""Thrust"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3484e01e-9c4b-4180-86a2-50326cdf82b5"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Brake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8b010e7-b06b-4f46-ac43-fbfa966c2373"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepads"",
                    ""action"": ""Brake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65e16429-1990-4cbc-baac-f8733616381f"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7863ebd3-bf44-4348-9c30-03d467b1eceb"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepads"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepads"",
            ""bindingGroup"": ""Gamepads"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Spaceship
        m_Spaceship = asset.FindActionMap("Spaceship", throwIfNotFound: true);
        m_Spaceship_Steering = m_Spaceship.FindAction("Steering", throwIfNotFound: true);
        m_Spaceship_Thrust = m_Spaceship.FindAction("Thrust", throwIfNotFound: true);
        m_Spaceship_Brake = m_Spaceship.FindAction("Brake", throwIfNotFound: true);
        m_Spaceship_Shoot = m_Spaceship.FindAction("Shoot", throwIfNotFound: true);
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

    // Spaceship
    private readonly InputActionMap m_Spaceship;
    private ISpaceshipActions m_SpaceshipActionsCallbackInterface;
    private readonly InputAction m_Spaceship_Steering;
    private readonly InputAction m_Spaceship_Thrust;
    private readonly InputAction m_Spaceship_Brake;
    private readonly InputAction m_Spaceship_Shoot;
    public struct SpaceshipActions
    {
        private @InputActions_Player m_Wrapper;
        public SpaceshipActions(@InputActions_Player wrapper) { m_Wrapper = wrapper; }
        public InputAction @Steering => m_Wrapper.m_Spaceship_Steering;
        public InputAction @Thrust => m_Wrapper.m_Spaceship_Thrust;
        public InputAction @Brake => m_Wrapper.m_Spaceship_Brake;
        public InputAction @Shoot => m_Wrapper.m_Spaceship_Shoot;
        public InputActionMap Get() { return m_Wrapper.m_Spaceship; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SpaceshipActions set) { return set.Get(); }
        public void SetCallbacks(ISpaceshipActions instance)
        {
            if (m_Wrapper.m_SpaceshipActionsCallbackInterface != null)
            {
                @Steering.started -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnSteering;
                @Steering.performed -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnSteering;
                @Steering.canceled -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnSteering;
                @Thrust.started -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnThrust;
                @Thrust.performed -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnThrust;
                @Thrust.canceled -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnThrust;
                @Brake.started -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnBrake;
                @Brake.performed -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnBrake;
                @Brake.canceled -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnBrake;
                @Shoot.started -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_SpaceshipActionsCallbackInterface.OnShoot;
            }
            m_Wrapper.m_SpaceshipActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Steering.started += instance.OnSteering;
                @Steering.performed += instance.OnSteering;
                @Steering.canceled += instance.OnSteering;
                @Thrust.started += instance.OnThrust;
                @Thrust.performed += instance.OnThrust;
                @Thrust.canceled += instance.OnThrust;
                @Brake.started += instance.OnBrake;
                @Brake.performed += instance.OnBrake;
                @Brake.canceled += instance.OnBrake;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
            }
        }
    }
    public SpaceshipActions @Spaceship => new SpaceshipActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    private int m_GamepadsSchemeIndex = -1;
    public InputControlScheme GamepadsScheme
    {
        get
        {
            if (m_GamepadsSchemeIndex == -1) m_GamepadsSchemeIndex = asset.FindControlSchemeIndex("Gamepads");
            return asset.controlSchemes[m_GamepadsSchemeIndex];
        }
    }
    public interface ISpaceshipActions
    {
        void OnSteering(InputAction.CallbackContext context);
        void OnThrust(InputAction.CallbackContext context);
        void OnBrake(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
    }
}
