// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\dxgidebug.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TerraFX.Interop.DXGI
{
    [Guid("C5A05F0C-16F2-4ADF-9F4D-A8C4D58AC550")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    public interface IDXGIDebug1 : IDXGIDebug
    {
        #region IDXGIDebug
        new void ReportLiveObjects(Guid apiid, DXGI_DEBUG_RLO_FLAGS flags);
        #endregion

        #region Methods
        [PreserveSig]
        void EnableLeakTrackingForThread();

        [PreserveSig]
        void DisableLeakTrackingForThread();

        [PreserveSig]
        int IsLeakTrackingEnabledForThread();
        #endregion
    }
}