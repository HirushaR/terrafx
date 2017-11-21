// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\d3d12.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Runtime.InteropServices;

namespace TerraFX.Interop
{
    public /* blittable */ unsafe struct D3D12_STREAM_OUTPUT_DESC
    {
        #region Fields
        [ComAliasName("D3D12_SO_DECLARATION_ENTRY[]")]
        public D3D12_SO_DECLARATION_ENTRY* pSODeclaration;

        [ComAliasName("UINT")]
        public uint NumEntries;

        [ComAliasName("UINT[]")]
        public uint* pBufferStrides;

        [ComAliasName("UINT")]
        public uint NumStrides;

        [ComAliasName("UINT")]
        public uint RasterizedStream;
        #endregion
    }
}