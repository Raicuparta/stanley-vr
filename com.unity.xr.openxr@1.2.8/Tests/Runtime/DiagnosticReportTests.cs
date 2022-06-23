using NUnit.Framework;

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class DiagnosticReportTests
    {
        const string k_SectionOneTitle = "Section One";
        const string k_SectionTwoTitle = "Section Two";

        [SetUp]
        public void SetUp()
        {
            DiagnosticReport.StartReport();
        }

        [Test]
        public void GettingSectionReturnsValidHandle()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            Assert.AreNotEqual(DiagnosticReport.k_NullSection, sectionOneHandle);

        }

        [Test]
        public void SameSectionTitleGivesSameSectionHandle()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionOneHandleTwo = DiagnosticReport.GetSection(k_SectionOneTitle);
            Assert.AreEqual(sectionOneHandle, sectionOneHandleTwo);
        }

        [Test]
        public void DifferentSectionTitlesGiveDifferentSectionHandles()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            Assert.AreNotEqual(sectionOneHandle, sectionTwoHandle);
        }

        [Test]
        public void CheckSimpleReportGenerationIsCorrect()
        {
            const string k_ExpectedOutput = "==== Section One ====\n\n==== Last 20 Events ====\n";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var report = DiagnosticReport.GenerateReport();
            Assert.IsFalse(String.IsNullOrEmpty(report));
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGenerateReportWithEntries()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Entry Header: Entry Body
Entry Header 2: Entry Body 2
Entry Header 3: Entry Body 3

==== Last 20 Events ====
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header", "Entry Body");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 2", "Entry Body 2");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 3", "Entry Body 3");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGenerateReportWithMultipleSectionsAndEntries()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Entry Header: Entry Body
Entry Header 2: Entry Body 2
Entry Header 3: Entry Body 3

==== Section Two ====
Entry Header 4: Entry Body 4
Entry Header 5: Entry Body 5
Entry Header 6: Entry Body 6

==== Last 20 Events ====
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header", "Entry Body");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 2", "Entry Body 2");
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Entry Header 3", "Entry Body 3");

            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 4", "Entry Body 4");
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 5", "Entry Body 5");
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Entry Header 6", "Entry Body 6");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);
        }

        [Test]
        public void CheckGeneratedEventsAreReported()
        {
            const string k_ExpectedOutput = @"==== Last 20 Events ====
Event One: Event Body One
";

            DiagnosticReport.AddEventEntry("Event One", "Event Body One");

            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void CheckGeneratedEventsOverTwentyAreReported()
        {
            const string k_ExpectedOutput = @"==== Last 20 Events ====
Event 11: Event Body 11
Event 12: Event Body 12
Event 13: Event Body 13
Event 14: Event Body 14
Event 15: Event Body 15
Event 16: Event Body 16
Event 17: Event Body 17
Event 18: Event Body 18
Event 19: Event Body 19
Event 20: Event Body 20
Event 21: Event Body 21
Event 22: Event Body 22
Event 23: Event Body 23
Event 24: Event Body 24
Event 25: Event Body 25
Event 26: Event Body 26
Event 27: Event Body 27
Event 28: Event Body 28
Event 29: Event Body 29
Event 30: Event Body 30
";

            for (int i = 0; i <= 30; i++)
            {
                DiagnosticReport.AddEventEntry($"Event {i}", $"Event Body {i}");
            }


            var report = DiagnosticReport.GenerateReport();
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void CheckFullReport()
        {
            const string k_ExpectedOutput = @"==== Section One ====
Section One Entry One: Simple

==== Section Two ====
Section Two Entry One: Simple

Section Two Entry Two: (2)
    FOO=BAR
    BAZ=100

==== Last 20 Events ====
Event 11: Event Body 11
Event 12: Event Body 12
Event 13: Event Body 13
Event 14: Event Body 14
Event 15: Event Body 15
Event 16: Event Body 16
Event 17: Event Body 17
Event 18: Event Body 18
Event 19: Event Body 19
Event 20: Event Body 20
Event 21: Event Body 21
Event 22: Event Body 22
Event 23: Event Body 23
Event 24: Event Body 24
Event 25: Event Body 25
Event 26: Event Body 26
Event 27: Event Body 27
Event 28: Event Body 28
Event 29: Event Body 29
Event 30: Event Body 30
";

            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            DiagnosticReport.AddSectionEntry(sectionOneHandle, "Section One Entry One", "Simple");

            for (int i = 0; i <= 30; i++)
            {
                DiagnosticReport.AddEventEntry($"Event {i}", $"Event Body {i}");
            }

            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Section Two Entry One", "Simple");
            DiagnosticReport.AddSectionBreak(sectionTwoHandle);
            DiagnosticReport.AddSectionEntry(sectionTwoHandle, "Section Two Entry Two", @"(2)
    FOO=BAR
    BAZ=100
");

            var report = DiagnosticReport.GenerateReport();
            Debug.Log(report);
            Assert.AreEqual(k_ExpectedOutput, report);

        }

        [Test]
        public void SectionReportsStayInCreatedOrder()
        {
            var sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            var reportOne = DiagnosticReport.GenerateReport();

            DiagnosticReport.StartReport();
            sectionTwoHandle = DiagnosticReport.GetSection(k_SectionTwoTitle);
            sectionOneHandle = DiagnosticReport.GetSection(k_SectionOneTitle);
            var reportTwo = DiagnosticReport.GenerateReport();

            Assert.AreNotEqual(reportOne, reportTwo);

        }

        static readonly (string, string, string)[] k_CompanyValues = {
                ("Windows Mixed Reality Runtime", "Microsoft", "is"),
                ("Oculus", "Oculus", "is"),
                ("SteamVR/OpenXR", "Valve", "is not"),
                ("", "UNKNOWN COMPANY", "is not")
        };

        [Test]
        public void AddRuntimeToCustomerSupportAddsCorrectCompanyInfoToReport(
            [ValueSource("k_CompanyValues")](string runtime, string company, string supported) info)
        {
            string matchString = $"OpenXR Runtime:\n    {info.company}, which {info.supported} a Unity supported partner";
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportRuntimeInfo(info.runtime);
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf(matchString) > 0);
        }


        [Test]
        public void AddDuplicateFeauterOnlyReportsOneIntance()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Microsoft Hand Interaction Profile");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Microsoft Hand Interaction Profile");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf("    Microsoft Hand Interaction Profile: Microsoft, which is a Unity supported partner") > 0);
        }

        [Test]
        public void AddFeatureToCustomerSupportAddsCorrectCompanyInfoToReport()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Microsoft Hand Interaction Profile");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Eye Tracking");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Unity", "First Person Observer");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Valve", "Knuckles");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf("    Microsoft Hand Interaction Profile, Eye Tracking: Microsoft, which is a Unity supported partner") > 0);
            Assert.IsTrue(report.IndexOf("    First Person Observer: Unity") > 0);
            Assert.IsTrue(report.IndexOf("    Knuckles: Valve, which is not a Unity supported partner") > 0);
        }

        const string k_ExpectedCustomerSupportSupportedText = "Unity Support:\n    Unity supports the runtime and Unity OpenXR Features above. When requesting assistance, please copy the OpenXR section from ==== Start Unity OpenXR Diagnostic Report ==== to ==== End Unity OpenXR Diagnostic Report ==== to the bug or forum post.";
        const string k_ExpectedCustomerSupportNotsupportedText = "Unity Support:\n    Unity doesn't support some aspects of the runtime and Unity OpenXR Features above. Please attempt to reproduce the issue with only Unity supported aspects before submitting an issue to Unity.";

        [Test]
        public void AddSupportedPartnersReportsSupported()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportRuntimeInfo("Windows Mixed Reality Runtime");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Microsoft Hand Interaction Profile");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Eye Tracking");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Unity", "First Person Observer");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf(k_ExpectedCustomerSupportSupportedText) > 0);
        }

        [Test]
        public void AddUnsupportedPartnersReportsUnsupported()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportRuntimeInfo("Windows Mixed Reality Runtime");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Valve", "Knuckles");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf(k_ExpectedCustomerSupportNotsupportedText) > 0);
        }

        [Test]
        public void AddMixedPartnersReportsUnsupported()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportRuntimeInfo("Windows Mixed Reality Runtime");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Microsoft Hand Interaction Profile");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Microsoft", "Eye Tracking");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Unity", "First Person Observer");
            DiagnosticReport.AddCustomerSupportFeatureInfo("Valve", "Knuckles");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf(k_ExpectedCustomerSupportNotsupportedText) > 0);
        }

        [Test]
        public void AddUnsupportedRuntimeReportsUnsupported()
        {
            DiagnosticReport.StartReport();
            DiagnosticReport.AddCustomerSupportRuntimeInfo("SteamVR/OpenXR");
            var report = DiagnosticReport.GenerateReport();

            Assert.IsTrue(report.IndexOf(k_ExpectedCustomerSupportNotsupportedText) > 0);
        }
    }
}