#ifndef OPENXR_MOCK_DRIVER
#define OPENXR_MOCK_DRIVER

#define XR_UNITY_mock_driver_SPEC_VERSION 001
#define XR_UNITY_MOCK_DRIVER_EXTENSION_NAME "XR_UNITY_mock_driver"

// Provides a means of requesting that the current state of the device be moved to passed in state.
// If forceTransition is false, this will only succeed if the new state is a correct transition from
// the current state. If forceTransition is true, the device will immediately be set to the requested
// state. Forcing the transition could have serious consequences (up to and including crashing the
// device).
//
//
typedef XrResult(XRAPI_PTR* PFN_xrTransitionMockToStateUNITY)(XrSession session, XrSessionState requestedState, bool forceTransition);

// Force a specific return code for named function. If the mock driver finds a set return code for
// that function it will remove it and return it immediately on function exectution.
//
typedef XrResult(XRAPI_PTR* PFN_xrSetReturnCodeForFunctionUNITY)(const char* functionName, XrResult returnValue);

// If we are currently in a valid running state, this will cause us to
// ask the runtime to transition to STOPPING.
//
typedef XrResult(XRAPI_PTR* PFN_xrRequestExitSessionUNITY)(XrSession session);

// All future view configs will return XR_ENVIRONMENT_BLEND_MODE_OPAQUE if true,
// XR_ENVIRONMENT_BLEND_MODE_ADDITIVE if false.
//
typedef XrResult(XRAPI_PTR* PFN_xrSetBlendModeUNITY)(XrEnvironmentBlendMode blendMode);
//
typedef XrResult(XRAPI_PTR* PFN_xrSetReferenceSpaceBoundsRectUNITY)(XrSession session, XrReferenceSpaceType referenceSpace, XrExtent2Df bounds);

// Cause the mock device to report an instance loss event.
//
typedef XrResult(XRAPI_PTR* PFN_xrCauseInstanceLossUNITY)(XrInstance instance);

// Set the result of the xrLocateSpace call
//
typedef XrResult(XRAPI_PTR* PFN_xrSetSpacePoseUNITY)(XrPosef pose, XrSpaceLocationFlags locationFlags);

// Set the result of the xrLocateSpace call
//
typedef XrResult(XRAPI_PTR* PFN_xrSetViewPoseUNITY)(int viewIndex, XrPosef pose, XrFovf fov, XrViewStateFlags viewStateFlags);

// Retrieve the end frame statistics from the last frame
//
typedef XrResult(XRAPI_PTR* PFN_xrGetEndFrameStatsUNITY)(int* primaryLayerCount, int* secondaryLayerCount);

// Activate a secondary view configuration
//
typedef XrResult(XRAPI_PTR* PFN_xrActivateSecondaryViewUNITY)(XrViewConfigurationType viewConfigurationType, bool activate);

// Register a callback that gets called for script events sent by mock
//
typedef enum
{
    XR_MOCK_SCRIPT_EVENT_UNKNOWN,
    XR_MOCK_SCRIPT_EVENT_END_FRAME,
    XR_MOCK_SCRIPT_EVENT_HAPTIC_IMPULSE,
    XR_MOCK_SCRIPT_EVENT_HAPTIC_STOP
} XrMockScriptEvent;

typedef void (*PFN_ScriptEventCallback)(XrMockScriptEvent event, uint64_t param);

typedef XrResult(XRAPI_PTR* PFN_xrRegisterScriptEventCallback)(PFN_ScriptEventCallback callback);

#endif //OPENXR_MOCK_DRIVER