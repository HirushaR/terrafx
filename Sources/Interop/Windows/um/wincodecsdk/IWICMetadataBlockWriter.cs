// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\wincodecsdk.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TerraFX.Interop
{
    [Guid("08FB9676-B444-41E8-8DBE-6A53A542BFF1")]
    public /* blittable */ unsafe struct IWICMetadataBlockWriter
    {
        #region Fields
        public readonly Vtbl* lpVtbl;
        #endregion

        #region Delegates
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int InitializeFromBlockReader(
            [In] IWICMetadataBlockWriter* This,
            [In] IWICMetadataBlockReader* pIMDBlockReader = null
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int GetWriterByIndex(
            [In] IWICMetadataBlockWriter* This,
            [In, ComAliasName("UINT")] uint nIndex,
            [Out] IWICMetadataWriter** ppIMetadataWriter = null
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int AddWriter(
            [In] IWICMetadataBlockWriter* This,
            [In] IWICMetadataWriter* pIMetadataWriter = null
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int SetWriterByIndex(
            [In] IWICMetadataBlockWriter* This,
            [In, ComAliasName("UINT")] uint nIndex,
            [In] IWICMetadataWriter* pIMetadataWriter = null
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int RemoveWriterByIndex(
            [In] IWICMetadataBlockWriter* This,
            [In, ComAliasName("UINT")] uint nIndex
        );
        #endregion

        #region Structs
        public /* blittable */ struct Vtbl
        {
            #region Fields
            public IWICMetadataBlockReader.Vtbl BaseVtbl;

            public IntPtr InitializeFromBlockReader;

            public IntPtr GetWriterByIndex;

            public IntPtr AddWriter;

            public IntPtr SetWriterByIndex;

            public IntPtr RemoveWriterByIndex;
            #endregion
        }
        #endregion
    }
}
