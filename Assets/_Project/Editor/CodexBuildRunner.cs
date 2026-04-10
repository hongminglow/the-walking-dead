// ============================================================
// File:        CodexBuildRunner.cs
// Namespace:   TWD.EditorTools
// Description: Reusable editor build entry points for local
//              Codex-driven build verification.
// Author:      The Walking Dead Team
// Created:     2026-04-10
// ============================================================

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TWD.EditorTools
{
    public static class CodexBuildRunner
    {
        public static void BuildWindowsCandidate()
        {
            string outputPath = System.Environment.GetEnvironmentVariable("CODEX_BUILD_OUTPUT");
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new InvalidOperationException("Missing CODEX_BUILD_OUTPUT environment variable.");

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException("Invalid build output path."));

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[CodexBuildRunner] Building Windows candidate to: {outputPath}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Debug.Log($"[CodexBuildRunner] Build result: {summary.result} | errors={summary.totalErrors} warnings={summary.totalWarnings} size={summary.totalSize}");

            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows build failed with result {summary.result}.");
        }
    }
}
#endif
