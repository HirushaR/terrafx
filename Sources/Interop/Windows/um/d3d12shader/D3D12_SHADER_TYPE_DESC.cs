// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\d3d12shader.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop
{
    public /* blittable */ struct D3D12_SHADER_TYPE_DESC
    {
        #region Fields
        public D3D_SHADER_VARIABLE_CLASS Class;

        public D3D_SHADER_VARIABLE_TYPE Type;

        public UINT Rows;

        public UINT Columns;

        public UINT Elements;

        public UINT Members;

        public UINT Offset;

        public LPCSTR Name;
        #endregion
    }
}