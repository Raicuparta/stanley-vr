#pragma once

class MockInputState
{
public:
    XrPath interactionProfile;
    XrPath path;
    XrActionType type;
    const char* localizedName;

    bool IsType(XrActionType actionType) const
    {
        return type == actionType;
    }

    bool IsCompatibleType(XrActionType actionType) const;

    void Reset();

    void Set(float value);
    void Set(XrBool32 value);
    void Set(XrVector2f value);
    void Set(XrSpace space, XrPosef pose);

    void CopyValue(const MockInputState& state);

    float GetFloat() const;
    XrBool32 GetBoolean() const;
    XrVector2f GetVector2() const;
    XrSpace GetLocationSpace() const;
    XrPosef GetLocationPose() const;

private:
    union {
        XrBool32 boolValue;
        float floatValue;
        XrVector2f vectorValue;
        struct
        {
            XrSpace space;
            XrPosef pose;
        } locationValue;

    } value;
};
