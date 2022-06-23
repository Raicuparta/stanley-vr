using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEngine.XR.OpenXR
{
    internal class DiagnosticReport
    {
        private const string LibraryName = "UnityOpenXR";
        public static readonly ulong k_NullSection = 0;

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_StartReport")]
        public static extern void StartReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_GetSection", CharSet = CharSet.Ansi)]
        public static extern ulong GetSection(string sectionName);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddSectionEntry", CharSet = CharSet.Ansi)]
        public static extern void AddSectionEntry(ulong sectionHandle, string sectionEntry, string sectionBody);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddSectionBreak", CharSet = CharSet.Ansi)]
        public static extern void AddSectionBreak(ulong sectionHandle);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddEventEntry", CharSet = CharSet.Ansi)]
        public static extern void AddEventEntry(string eventName, string eventData);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_DumpReport")]
        private static extern void Internal_DumpReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_DumpReportWithReason")]
        private static extern void Internal_DumpReport(string reason);

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_GenerateReport")]
        static extern IntPtr Internal_GenerateReport();

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_ReleaseReport")]
        static extern void Internal_ReleaseReport(IntPtr report);


        public enum CustomerSupportEntryType
        {
            Runtime,
            Feature
        }

        [DllImport(LibraryName, EntryPoint = "DiagnosticReport_AddCustomerSupportEntry")]
        static extern void Internal_AddCustomerSupportEntry(CustomerSupportEntryType type, string company, string value);

        static Dictionary<string, string> s_RuntimeMap = new Dictionary<string, string>(){
            { "Windows Mixed Reality Runtime", "Microsoft" },
            { "Microsoft Holographic AppRemoting Runtime", "Microsoft" },
            { "Oculus", "Oculus"},
            { "SteamVR/OpenXR", "Valve"},
            { "Unity Mock Runtime", "Unity" },
        };

        public static void AddCustomerSupportRuntimeInfo(string runtimeName)
        {
            string company;
            if (!s_RuntimeMap.TryGetValue(runtimeName, out company))
                company = "UNKNOWN COMPANY";
            Internal_AddCustomerSupportEntry(CustomerSupportEntryType.Runtime, company, "");
        }

        public static void AddCustomerSupportFeatureInfo(string company, string value)
        {
            Internal_AddCustomerSupportEntry(CustomerSupportEntryType.Feature, company, value);
        }

        internal static string GenerateReport()
        {
            string ret = "";

            IntPtr buffer = Internal_GenerateReport();
            if (buffer != IntPtr.Zero)
            {
                ret = Marshal.PtrToStringAnsi(buffer);
                Internal_ReleaseReport(buffer);
                buffer = IntPtr.Zero;
            }
            return ret;
        }

        public static void DumpReport(string reason)
        {
            AddCustomerSupportRuntimeInfo(OpenXRRuntime.name);
            var features = (OpenXRFeature[])OpenXRSettings.Instance.features.Clone();
            foreach (var feature in features)
            {
                if (null == feature || !feature.enabled)
                    continue;

                if (String.IsNullOrEmpty(feature.company) || String.IsNullOrEmpty(feature.name))
                    continue;
                AddCustomerSupportFeatureInfo(feature.company, feature.name);
            }
            Internal_DumpReport(reason);
        }
    }
}