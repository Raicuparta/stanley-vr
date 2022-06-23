// Defines Mock Runtime specific events at the end of the structure type list

const XrStructureType XR_TYPE_EVENT_SCRIPT_EVENT_MOCK = (XrStructureType)(XR_STRUCTURE_TYPE_MAX_ENUM - 1);

typedef struct XrEventScriptEventMOCK
{
    XrStructureType type;
    const void* XR_MAY_ALIAS next;
    XrMockScriptEvent event;
    uint64_t param;

} XrEventScriptEventMOCK;
