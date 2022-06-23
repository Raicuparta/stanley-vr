#include "../mock.h"

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrRequestExitSession(XrSession session);

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrTransitionMockToStateUNITY(XrSession session, XrSessionState requestedState, bool forceTransition)
{
    LOG_FUNC();
    CHECK_SESSION(session);

    MOCK_TRACE_DEBUG("[Driver] Transition request to state %d with force %s", requestedState, forceTransition ? "TRUE" : "FALSE");
    if (!forceTransition && !s_runtime->IsStateTransitionValid(requestedState))
    {
        MOCK_TRACE_ERROR("[Driver] Failed to request state. Was transition valid: %s with force %s",
            s_runtime->IsStateTransitionValid(requestedState) ? "TRUE" : "FALSE",
            forceTransition ? "TRUE" : "FALSE");
        return XR_ERROR_VALIDATION_FAILURE;
    }

    MOCK_TRACE_DEBUG("[Driver] Transitioning to requested state %d\n", requestedState);
    s_runtime->ChangeSessionState(requestedState);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetReturnCodeForFunctionUNITY(const char* functionName, XrResult result)
{
    LOG_FUNC();
    s_runtime->SetFunctionResult(functionName, result);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrRequestExitSessionUNITY(XrSession session)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    if (s_runtime->IsSessionState(XR_SESSION_STATE_READY) ||
        s_runtime->IsSessionState(XR_SESSION_STATE_SYNCHRONIZED) ||
        s_runtime->IsSessionState(XR_SESSION_STATE_VISIBLE) ||
        s_runtime->IsSessionState(XR_SESSION_STATE_FOCUSED))
        return xrRequestExitSession(session);

    return XR_ERROR_VALIDATION_FAILURE;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrSetBlendModeUNITY(XrEnvironmentBlendMode blendMode)
{
    CHECK_RUNTIME();
    s_runtime->SetMockBlendMode(blendMode);
    return XR_SUCCESS;
}

extern "C" XrResult UNITY_INTERFACE_EXPORT XRAPI_PTR xrActivateSecondaryViewUNITY(XrViewConfigurationType viewConfigurationType, bool activate)
{
    CHECK_RUNTIME();
    return s_runtime->ActivateSecondaryView(viewConfigurationType, activate);
}

XrResult xrSetReferenceSpaceBoundsRectUNITY(XrSession session, XrReferenceSpaceType referenceSpace, const XrExtent2Df bounds)
{
    LOG_FUNC();
    CHECK_SESSION(session);
    s_runtime->SetExtentsForReferenceSpace(referenceSpace, bounds);
    return XR_SUCCESS;
}

XrResult xrCauseInstanceLossUNITY(XrInstance instance)
{
    CHECK_INSTANCE(instance);
    s_runtime->CauseInstanceLoss();
    return XR_SUCCESS;
}

XrResult xrSetSpacePoseUNITY(XrPosef pose, XrSpaceLocationFlags locationFlags)
{
    CHECK_RUNTIME();
    s_runtime->SetSpacePose(pose, locationFlags);
    return XR_SUCCESS;
}

XrResult xrSetViewPoseUNITY(int viewIndex, XrPosef pose, XrFovf fov, XrViewStateFlags viewStateFlags)
{
    CHECK_RUNTIME();
    s_runtime->SetViewPose(viewIndex, pose, fov, viewStateFlags);
    return XR_SUCCESS;
}

XrResult xrGetEndFrameStatsUNITY(int* primaryLayerCount, int* secondaryLayerCount)
{
    CHECK_RUNTIME();
    return s_runtime->GetEndFrameStats(primaryLayerCount, secondaryLayerCount);
}

XrResult xrRegisterScriptEventCallbackUNITY(PFN_ScriptEventCallback callback)
{
    CHECK_RUNTIME();
    return s_runtime->RegisterScriptEventCallback(callback);
}

XrResult MockDriver_GetInstanceProcAddr(XrInstance instance, const char* name, PFN_xrVoidFunction* function)
{
#define LOOKUP(funcName)                           \
    if (strcmp(#funcName, name) == 0)              \
    {                                              \
        *function = (PFN_xrVoidFunction)&funcName; \
        return XR_SUCCESS;                         \
    }

    LOOKUP(xrTransitionMockToStateUNITY)
    LOOKUP(xrSetReturnCodeForFunctionUNITY)
    LOOKUP(xrRequestExitSessionUNITY)
    LOOKUP(xrSetBlendModeUNITY)
    LOOKUP(xrActivateSecondaryViewUNITY)
    LOOKUP(xrSetReferenceSpaceBoundsRectUNITY)
    LOOKUP(xrCauseInstanceLossUNITY);
    LOOKUP(xrSetSpacePoseUNITY);
    LOOKUP(xrSetViewPoseUNITY);
    LOOKUP(xrGetEndFrameStatsUNITY);
    LOOKUP(xrRegisterScriptEventCallbackUNITY);

#undef LOOKUP

    return XR_ERROR_FUNCTION_UNSUPPORTED;
}

#undef DEBUG_LOG_EVERY_FUNC_CALL
#undef DEBUG_TRACE
