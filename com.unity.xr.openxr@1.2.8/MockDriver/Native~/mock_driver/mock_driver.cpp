#include "IUnityInterface.h"
#include "XR/IUnityXRTrace.h"
#include <cstring>
#include <queue>
#include <string>
#include <vector>

#define XR_NO_PROTOTYPES
#include <openxr/openxr.h>

#include "openxr_mock_driver.h"

#include "XR/IUnityXRTrace.h"

#include <cassert>
#define CHECK_XRCMD(x)             \
    {                              \
        auto ret = x;              \
        assert(ret == XR_SUCCESS); \
    }

struct DriverContext
{
    PFN_xrTransitionMockToStateUNITY xrTransitionMockToState;
    PFN_xrSetReturnCodeForFunctionUNITY xrSetReturnCodeForFunction;
    PFN_xrRequestExitSessionUNITY xrRequestExitSession;
    PFN_xrSetBlendModeUNITY xrSetBlendMode;
    PFN_xrSetReferenceSpaceBoundsRectUNITY xrSetReferenceSpaceBoundsRect;
    PFN_xrCauseInstanceLossUNITY xrCauseInstanceLoss;
    PFN_xrSetSpacePoseUNITY xrSetSpacePoseUNITY;
    PFN_xrSetViewPoseUNITY xrSetViewPoseUNITY;
    PFN_xrGetEndFrameStatsUNITY xrGetEndFrameStatsUNITY;
    PFN_xrActivateSecondaryViewUNITY xrActivateSecondaryViewUNITY;
    PFN_xrRegisterScriptEventCallback xrRegisterScriptEventCallbackUNITY;
} s_DriverContext{};

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_initialize(PFN_xrGetInstanceProcAddr xrGetInstanceProcAddr, XrInstance instance, XrSession session, XrSpace sceneSpace)
{
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrTransitionMockToStateUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrTransitionMockToState));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetReturnCodeForFunctionUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrSetReturnCodeForFunction));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrRequestExitSessionUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrRequestExitSession));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetBlendModeUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrSetBlendMode));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetReferenceSpaceBoundsRectUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrSetReferenceSpaceBoundsRect));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrCauseInstanceLossUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrCauseInstanceLoss));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetSpacePoseUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrSetSpacePoseUNITY));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrSetViewPoseUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrSetViewPoseUNITY));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrGetEndFrameStatsUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrGetEndFrameStatsUNITY));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrActivateSecondaryViewUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrActivateSecondaryViewUNITY));
    CHECK_XRCMD(xrGetInstanceProcAddr(instance, "xrRegisterScriptEventCallbackUNITY", (PFN_xrVoidFunction*)&s_DriverContext.xrRegisterScriptEventCallbackUNITY));
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
script_shutdown(XrInstance instance)
{
    s_DriverContext = {};
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_TransitionMockToState(XrSession session, XrSessionState requestedState, bool forceTransition)
{
    if (s_DriverContext.xrTransitionMockToState)
        return s_DriverContext.xrTransitionMockToState(session, requestedState, forceTransition);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_SetReturnCodeForFunction(const char* functionName, XrResult result)
{
    if (s_DriverContext.xrSetReturnCodeForFunction)
        return s_DriverContext.xrSetReturnCodeForFunction(functionName, result);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_RequestExitSession(XrSession session)
{
    if (s_DriverContext.xrRequestExitSession)
        return s_DriverContext.xrRequestExitSession(session);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_SetBlendModeOpaque(bool opaque)
{
    if (s_DriverContext.xrSetBlendMode)
    {
        return s_DriverContext.xrSetBlendMode(opaque ? XR_ENVIRONMENT_BLEND_MODE_OPAQUE : XR_ENVIRONMENT_BLEND_MODE_ADDITIVE);
    }
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_SetReferenceSpaceBoundsRect(XrSession session, XrReferenceSpaceType referenceSpaceType, XrExtent2Df bounds)
{
    if (s_DriverContext.xrSetReferenceSpaceBoundsRect)
        return s_DriverContext.xrSetReferenceSpaceBoundsRect(session, referenceSpaceType, bounds);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_CauseInstanceLoss(XrInstance instance)
{
    if (s_DriverContext.xrCauseInstanceLoss)
        return s_DriverContext.xrCauseInstanceLoss(instance);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_SetSpacePose(XrQuaternionf orientation, XrVector3f position, XrSpaceLocationFlags locationFlags)
{
    if (s_DriverContext.xrSetSpacePoseUNITY)
        return s_DriverContext.xrSetSpacePoseUNITY(XrPosef{orientation, position}, locationFlags);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_SetViewPose(int viewIndex, XrQuaternionf orientation, XrVector3f position, XrFovf fov, XrViewStateFlags viewState)
{
    if (s_DriverContext.xrSetViewPoseUNITY)
        return s_DriverContext.xrSetViewPoseUNITY(viewIndex, XrPosef{orientation, position}, fov, viewState);

    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_GetEndFrameStats(int* primaryLayerCount, int* secondaryLayerCount)
{
    if (s_DriverContext.xrGetEndFrameStatsUNITY)
        return s_DriverContext.xrGetEndFrameStatsUNITY(primaryLayerCount, secondaryLayerCount);

    *primaryLayerCount = 0;
    *secondaryLayerCount = 0;
    return XR_ERROR_EXTENSION_NOT_PRESENT;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_ActivateSecondaryView(XrViewConfigurationType viewConfigurationType, bool activate)
{
    if (s_DriverContext.xrActivateSecondaryViewUNITY)
        return s_DriverContext.xrActivateSecondaryViewUNITY(viewConfigurationType, activate);

    return XR_ERROR_EXTENSION_NOT_PRESENT;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR MockDriver_RegisterScriptEventCallback(PFN_ScriptEventCallback callback)
{
    if (s_DriverContext.xrRegisterScriptEventCallbackUNITY)
        return s_DriverContext.xrRegisterScriptEventCallbackUNITY(callback);

    return XR_ERROR_EXTENSION_NOT_PRESENT;
}

#undef DEBUG_LOG_EVERY_FUNC_CALL
