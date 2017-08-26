// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from src\spec\vk.xml in the Vulkan-Docs repository for tag v1.0.51-core
// Original source is Copyright © 2015-2017 The Khronos Group Inc.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TerraFX.Interop
{
    public static unsafe partial class Vulkan
    {
        #region Constants
        public const uint VK_KHR_display = 1;

        public const uint VK_KHR_DISPLAY_SPEC_VERSION = 21;

        public const string VK_KHR_DISPLAY_EXTENSION_NAME = "VK_KHR_display";
        #endregion

        #region External Methods
        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkGetPhysicalDeviceDisplayPropertiesKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkGetPhysicalDeviceDisplayPropertiesKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            uint* pPropertyCount,
            VkDisplayPropertiesKHR* pProperties
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkGetPhysicalDeviceDisplayPlanePropertiesKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkGetPhysicalDeviceDisplayPlanePropertiesKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            uint* pPropertyCount,
            VkDisplayPlanePropertiesKHR* pProperties
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkGetDisplayPlaneSupportedDisplaysKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkGetDisplayPlaneSupportedDisplaysKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            uint planeIndex,
            uint* pDisplayCount,
            [ComAliasName("VkDisplayKHR")] IntPtr* pDisplays
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkGetDisplayModePropertiesKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkGetDisplayModePropertiesKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            [ComAliasName("VkDisplayKHR")] IntPtr display,
            uint* pPropertyCount,
            VkDisplayModePropertiesKHR* pProperties
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkCreateDisplayModeKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkCreateDisplayModeKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            [ComAliasName("VkDisplayKHR")] IntPtr display,
            VkDisplayModeCreateInfoKHR* pCreateInfo,
            VkAllocationCallbacks* pAllocator,
            [ComAliasName("VkDisplayModeKHR")] IntPtr* pMode
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkGetDisplayPlaneCapabilitiesKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkGetDisplayPlaneCapabilitiesKHR(
            [ComAliasName("VkPhysicalDevice")] IntPtr physicalDevice,
            [ComAliasName("VkDisplayModeKHR")] IntPtr mode,
            uint planeIndex,
            VkDisplayPlaneCapabilitiesKHR* pCapabilities
        );

        [DllImport("Vulkan-1", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, EntryPoint = "vkCreateDisplayPlaneSurfaceKHR", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        public static extern VkResult vkCreateDisplayPlaneSurfaceKHR(
            [ComAliasName("VkInstance")] IntPtr instance,
            VkDisplaySurfaceCreateInfoKHR* pCreateInfo,
            VkAllocationCallbacks* pAllocator,
            [ComAliasName("VkSurfaceKHR")] IntPtr* pSurface
        );
        #endregion
    }
}
