using System;
using Devlooped;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CloudStorageAccount.CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic, LanguageNames.FSharp)]
class SponsorLinker : SponsorLink
{
    public SponsorLinker() : base(SponsorLinkSettings.Create(
        "devlooped", "Devlooped.CloudStorageAccount",
        diagnosticsIdPrefix: "DCSA",
        version: new Version(ThisAssembly.Info.Version).ToString(3)
#if DEBUG
        , quietDays: 0
#endif
        ))
    { }
}