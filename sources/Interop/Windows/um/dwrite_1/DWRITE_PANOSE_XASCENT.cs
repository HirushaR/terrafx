// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\dwrite_1.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop
{
    /// <summary>Relative size of the lowercase letters. Present for families: 3-script</summary>
    public enum DWRITE_PANOSE_XASCENT
    {
        DWRITE_PANOSE_XASCENT_ANY = 0,

        DWRITE_PANOSE_XASCENT_NO_FIT = 1,

        DWRITE_PANOSE_XASCENT_VERY_LOW = 2,

        DWRITE_PANOSE_XASCENT_LOW = 3,

        DWRITE_PANOSE_XASCENT_MEDIUM = 4,

        DWRITE_PANOSE_XASCENT_HIGH = 5,

        DWRITE_PANOSE_XASCENT_VERY_HIGH = 6
    }
}