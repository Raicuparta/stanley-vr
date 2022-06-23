#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Networking.PlayerConnection;

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Features.RuntimeDebugger.Editor")]
namespace UnityEditor.XR.OpenXR.Features.RuntimeDebugger
{
    internal class DebuggerState
    {
        public enum Command
        {
            kStartFunctionCall,
            kStartStruct,

            kFloat,
            kString,
            kInt32,
            kInt64,
            kUInt32,
            kUInt64,

            kEndStruct,
            kEndFunctionCall,

            kCacheNotLargeEnough,
        };

        internal static List<FunctionCall> _functionCalls = new List<FunctionCall>();

        private static Action _doneCallback;
        internal static UInt32 _lastPayloadSize;
        internal static UInt32 _frameCount;

        internal static void SetDoneCallback(Action done)
        {
            _doneCallback = done;
        }

        private static StringBuilder _sb = new StringBuilder();
        internal static string ReadString(BinaryReader r)
        {
            _sb.Clear();
            byte b;
            while ((b = r.ReadByte()) != (byte)0)
            {
                _sb.Append((Char)b);
            }
            return _sb.ToString();
        }

        internal static void OnMessageEvent(MessageEventArgs args)
        {
            if (args == null || args.data == null)
                return;
            _lastPayloadSize = (UInt32)args.data.Length;
            _frameCount = 0;
            try
            {
                using (MemoryStream ms = new MemoryStream(args.data))
                {
                    using (BinaryReader r = new BinaryReader(ms, Encoding.UTF8))
                    {
                        while (r.BaseStream.Position != r.BaseStream.Length)
                        {
                            var command = (Command)r.ReadUInt32();
                            switch (command)
                            {
                                case Command.kStartFunctionCall:
                                    var thread = ReadString(r);
                                    var funcName = ReadString(r);
                                    var funcCall = new FunctionCall(thread, funcName);
                                    _functionCalls.Add(funcCall);
                                    funcCall.Parse(r);

                                    if (funcName == "xrBeginFrame")
                                    {
                                        ++_frameCount;
                                    }
                                    break;
                                case Command.kCacheNotLargeEnough:
                                    funcCall = new FunctionCall(r.ReadUInt32().ToString(), ReadString(r));
                                    _functionCalls.Add(funcCall);
                                    var result = ReadString(r);
                                    funcCall.displayName += " = " + result + " (cache not large enough)";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            _doneCallback?.Invoke();
        }

        internal class DebugEvent : TreeViewItem
        {
            private static int idCounter = 1;
            private List<DebugEvent> childrenEvents = new List<DebugEvent>();

            protected DebugEvent(string displayName)
            : base(idCounter++, 0, displayName)
            {
            }

            public virtual string GetValue()
            {
                return "";
            }

            public void Parse(BinaryReader r)
            {
                DebugEvent parsedChild = null;
                bool endEvent = false;

                do
                {
                    if (parsedChild != null)
                    {
                        AddChildEvent(parsedChild);
                        parsedChild.Parse(r);
                        parsedChild = null;
                    }

                    var command = (Command) r.ReadUInt32();
                    switch (command)
                    {
                        case Command.kStartStruct:
                            parsedChild = new StructDebugEvent(ReadString(r), ReadString(r));
                            break;
                        case Command.kFloat:
                            AddChildEvent(new FloatDebugEvent(ReadString(r), r.ReadSingle()));
                            break;
                        case Command.kString:
                            AddChildEvent(new StringDebugEvent(ReadString(r), ReadString(r)));
                            break;
                        case Command.kInt32:
                            AddChildEvent(new Int32DebugEvent(ReadString(r), r.ReadInt32()));
                            break;
                        case Command.kInt64:
                            AddChildEvent(new Int64DebugEvent(ReadString(r), r.ReadInt64()));
                            break;
                        case Command.kUInt32:
                            AddChildEvent(new UInt32DebugEvent(ReadString(r), r.ReadUInt32()));
                            break;
                        case Command.kUInt64:
                            AddChildEvent(new UInt64DebugEvent(ReadString(r), r.ReadUInt64()));
                            break;
                        case Command.kEndStruct:
                            endEvent = true;
                            break;
                        case Command.kEndFunctionCall:
                            var result = ReadString(r);
                            displayName += " = " + result;
                            endEvent = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } while (!endEvent && r.BaseStream.Position != r.BaseStream.Length);
            }

//            public IEnumerable<DebugEvent> GetChildren()
//            {
//                return childrenEvents;
//            }

            private void AddChildEvent(DebugEvent evt)
            {
                childrenEvents.Add(evt);
                AddChild(evt);
            }
        }

        internal class FunctionCall : DebugEvent
        {
            public string threadId { get; }
            public string returnVal { get; set; }

            public FunctionCall(string threadId, string displayName)
            : base(displayName)
            {
                this.threadId = threadId;
            }
        }

        internal class StructDebugEvent : DebugEvent
        {
            public StructDebugEvent(string fieldname, string structname)
                : base($"{fieldname} = {structname}")
            {

            }
        }

        internal class FloatDebugEvent : DebugEvent
        {
            public float value { get; }
            public FloatDebugEvent(string displayName, float val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }
        }

        internal class StringDebugEvent : DebugEvent
        {
            public string value { get; }
            public StringDebugEvent(string displayName, string val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return value;
            }
        }

        internal class Int32DebugEvent : DebugEvent
        {
            public Int32 value { get; }
            public Int32DebugEvent(string displayName, Int32 val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }
        }

        internal class Int64DebugEvent : DebugEvent
        {
            public Int64 value { get; }
            public Int64DebugEvent(string displayName, Int64 val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }
        }

        internal class UInt32DebugEvent : DebugEvent
        {
            public UInt32 value { get; }
            public UInt32DebugEvent(string displayName, UInt32 val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }
        }

        internal class UInt64DebugEvent : DebugEvent
        {
            public UInt64 value { get; }
            public UInt64DebugEvent(string displayName, UInt64 val)
                : base(displayName + " = " + val)
            {
                this.value = val;
            }

            public override string GetValue()
            {
                return $"{value}";
            }
        }
    }
}
#endif