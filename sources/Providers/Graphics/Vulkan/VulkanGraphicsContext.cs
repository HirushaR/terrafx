// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;
using TerraFX.Interop;
using TerraFX.Numerics;
using TerraFX.Utilities;
using static TerraFX.Graphics.Providers.Vulkan.HelperUtilities;
using static TerraFX.Interop.VkAccessFlagBits;
using static TerraFX.Interop.VkCommandPoolCreateFlagBits;
using static TerraFX.Interop.VkComponentSwizzle;
using static TerraFX.Interop.VkDescriptorType;
using static TerraFX.Interop.VkImageAspectFlagBits;
using static TerraFX.Interop.VkImageLayout;
using static TerraFX.Interop.VkImageViewType;
using static TerraFX.Interop.VkIndexType;
using static TerraFX.Interop.VkPipelineBindPoint;
using static TerraFX.Interop.VkPipelineStageFlagBits;
using static TerraFX.Interop.VkStructureType;
using static TerraFX.Interop.VkSubpassContents;
using static TerraFX.Interop.Vulkan;
using static TerraFX.Utilities.AssertionUtilities;
using static TerraFX.Utilities.ExceptionUtilities;
using static TerraFX.Utilities.State;

namespace TerraFX.Graphics.Providers.Vulkan
{
    /// <inheritdoc />
    public sealed unsafe class VulkanGraphicsContext : GraphicsContext
    {
        private readonly VulkanGraphicsFence _fence;
        private readonly VulkanGraphicsFence _waitForExecuteCompletionFence;

        private ValueLazy<VkCommandBuffer> _vulkanCommandBuffer;
        private ValueLazy<VkCommandPool> _vulkanCommandPool;
        private ValueLazy<VkFramebuffer> _vulkanFramebuffer;
        private ValueLazy<VkImageView> _vulkanSwapChainImageView;

        private State _state;

        internal VulkanGraphicsContext(VulkanGraphicsDevice device, int index)
            : base(device, index)
        {
            _fence = new VulkanGraphicsFence(device);
            _waitForExecuteCompletionFence = new VulkanGraphicsFence(device);

            _vulkanCommandBuffer = new ValueLazy<VkCommandBuffer>(CreateVulkanCommandBuffer);
            _vulkanCommandPool = new ValueLazy<VkCommandPool>(CreateVulkanCommandPool);
            _vulkanFramebuffer = new ValueLazy<VkFramebuffer>(CreateVulkanFramebuffer);
            _vulkanSwapChainImageView = new ValueLazy<VkImageView>(CreateVulkanSwapChainImageView);

            _ = _state.Transition(to: Initialized);
        }

        /// <summary>Finalizes an instance of the <see cref="VulkanGraphicsContext" /> class.</summary>
        ~VulkanGraphicsContext() => Dispose(isDisposing: false);

        /// <inheritdoc cref="GraphicsContext.Device" />
        public new VulkanGraphicsDevice Device => (VulkanGraphicsDevice)base.Device;

        /// <inheritdoc />
        public override VulkanGraphicsFence Fence => _fence;

        /// <summary>Gets the <see cref="VkCommandBuffer" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkCommandBuffer VulkanCommandBuffer => _vulkanCommandBuffer.Value;

        /// <summary>Gets the <see cref="VkCommandPool" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkCommandPool VulkanCommandPool => _vulkanCommandPool.Value;

        /// <summary>Gets the <see cref="VkFramebuffer"/> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkFramebuffer VulkanFramebuffer => _vulkanFramebuffer.Value;

        /// <summary>Gets the <see cref="VkImageView" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkImageView VulkanSwapChainImageView => _vulkanSwapChainImageView.Value;

        /// <summary>Gets a fence that is used to wait for the context to finish execution.</summary>
        public VulkanGraphicsFence WaitForExecuteCompletionFence => _waitForExecuteCompletionFence;

        /// <inheritdoc />
        public override void BeginDrawing(ColorRgba backgroundColor)
        {
            var clearValue = new VkClearValue();

            clearValue.color.float32[0] = backgroundColor.Red;
            clearValue.color.float32[1] = backgroundColor.Green;
            clearValue.color.float32[2] = backgroundColor.Blue;
            clearValue.color.float32[3] = backgroundColor.Alpha;

            var device = Device;
            var surface = device.Surface;

            var surfaceWidth = surface.Width;
            var surfaceHeight = surface.Height;

            var renderPassBeginInfo = new VkRenderPassBeginInfo {
                sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO,
                renderPass = device.VulkanRenderPass,
                framebuffer = VulkanFramebuffer,
                renderArea = new VkRect2D {
                    extent = new VkExtent2D {
                        width = (uint)surface.Width,
                        height = (uint)surface.Height,
                    },
                },
                clearValueCount = 1,
                pClearValues = &clearValue,
            };

            var commandBuffer = VulkanCommandBuffer;
            vkCmdBeginRenderPass(commandBuffer, &renderPassBeginInfo, VK_SUBPASS_CONTENTS_INLINE);

            var viewport = new VkViewport {
                x = 0,
                y = surface.Height,
                width = surface.Width,
                height = -surface.Height,
                minDepth = 0.0f,
                maxDepth = 1.0f,
            };
            vkCmdSetViewport(commandBuffer, firstViewport: 0, viewportCount: 1, &viewport);

            var scissorRect = new VkRect2D {
                extent = new VkExtent2D {
                    width = (uint)surface.Width,
                    height = (uint)surface.Height,
                },
            };
            vkCmdSetScissor(commandBuffer, firstScissor: 0, scissorCount: 1, &scissorRect);
        }

        /// <inheritdoc />
        public override void BeginFrame()
        {
            var fence = Fence;

            fence.Wait();
            fence.Reset();

            var commandBufferBeginInfo = new VkCommandBufferBeginInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
            };

            ThrowExternalExceptionIfNotSuccess(nameof(vkBeginCommandBuffer), vkBeginCommandBuffer(VulkanCommandBuffer, &commandBufferBeginInfo));
        }

        /// <inheritdoc />
        public override void Copy(GraphicsBuffer destination, GraphicsBuffer source)
            => Copy((VulkanGraphicsBuffer)destination, (VulkanGraphicsBuffer)source);

        /// <inheritdoc />
        public override void Copy(GraphicsTexture destination, GraphicsBuffer source)
            => Copy((VulkanGraphicsTexture)destination, (VulkanGraphicsBuffer)source);

        /// <inheritdoc cref="Copy(GraphicsBuffer, GraphicsBuffer)" />
        public void Copy(VulkanGraphicsBuffer destination, VulkanGraphicsBuffer source)
        {
            ThrowIfNull(destination, nameof(destination));
            ThrowIfNull(source, nameof(source));

            var vulkanBufferCopy = new VkBufferCopy {
                srcOffset = 0,
                dstOffset = 0,
                size = Math.Min(destination.Size, source.Size),
            };
            vkCmdCopyBuffer(VulkanCommandBuffer, source.VulkanBuffer, destination.VulkanBuffer, 1, &vulkanBufferCopy);
        }

        /// <inheritdoc cref="Copy(GraphicsTexture, GraphicsBuffer)" />
        public void Copy(VulkanGraphicsTexture destination, VulkanGraphicsBuffer source)
        {
            ThrowIfNull(destination, nameof(destination));
            ThrowIfNull(source, nameof(source));

            var vulkanCommandBuffer = VulkanCommandBuffer;
            var vulkanImage = destination.VulkanImage;

            BeginCopy();

            var vulkanBufferImageCopy = new VkBufferImageCopy {
                imageSubresource = new VkImageSubresourceLayers {
                    aspectMask = (uint)VK_IMAGE_ASPECT_COLOR_BIT,
                    layerCount = 1,
                },
                imageExtent = new VkExtent3D {
                    width = (uint)destination.Width,
                    height = destination.Height,
                    depth = destination.Depth,
                },
            };

            vkCmdCopyBufferToImage(vulkanCommandBuffer, source.VulkanBuffer, vulkanImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &vulkanBufferImageCopy);

            EndCopy();

            void BeginCopy()
            {
                var vulkanImageMemoryBarrier = new VkImageMemoryBarrier {
                    sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
                    dstAccessMask = (uint)VK_ACCESS_TRANSFER_WRITE_BIT,
                    oldLayout = VK_IMAGE_LAYOUT_UNDEFINED,
                    newLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    image = vulkanImage,
                    subresourceRange = new VkImageSubresourceRange {
                        aspectMask = (uint)VK_IMAGE_ASPECT_COLOR_BIT,
                        levelCount = 1,
                        layerCount = 1,
                    },
                };

                vkCmdPipelineBarrier(vulkanCommandBuffer, (uint)VK_PIPELINE_STAGE_HOST_BIT, (uint)VK_PIPELINE_STAGE_TRANSFER_BIT, dependencyFlags: 0, memoryBarrierCount: 0, pMemoryBarriers: null, bufferMemoryBarrierCount: 0, pBufferMemoryBarriers: null, imageMemoryBarrierCount: 1, &vulkanImageMemoryBarrier);
            }

            void EndCopy()
            {
                var vulkanImageMemoryBarrier = new VkImageMemoryBarrier {
                    sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
                    srcAccessMask = (uint)VK_ACCESS_TRANSFER_WRITE_BIT,
                    dstAccessMask = (uint)VK_ACCESS_SHADER_READ_BIT,
                    oldLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL,
                    image = vulkanImage,
                    subresourceRange = new VkImageSubresourceRange {
                        aspectMask = (uint)VK_IMAGE_ASPECT_COLOR_BIT,
                        levelCount = 1,
                        layerCount = 1,
                    },
                };

                vkCmdPipelineBarrier(vulkanCommandBuffer, (uint)VK_PIPELINE_STAGE_TRANSFER_BIT, (uint)VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, dependencyFlags: 0, memoryBarrierCount: 0, pMemoryBarriers: null, bufferMemoryBarrierCount: 0, pBufferMemoryBarriers: null, imageMemoryBarrierCount: 1, &vulkanImageMemoryBarrier);
            }
        }

        /// <inheritdoc />
        public override void Draw(GraphicsPrimitive primitive)
            => Draw((VulkanGraphicsPrimitive)primitive);

        /// <inheritdoc cref="Draw(GraphicsPrimitive)" />
        public void Draw(VulkanGraphicsPrimitive primitive)
        {
            ThrowIfNull(primitive, nameof(primitive));

            var vulkanCommandBuffer = VulkanCommandBuffer;
            var pipeline = primitive.Pipeline;
            var pipelineSignature = pipeline.Signature;
            var vulkanPipeline = pipeline.VulkanPipeline;
            var vertexBuffer = (VulkanGraphicsBuffer)(primitive.VertexBufferView.Buffer);
            var vulkanVertexBuffer = vertexBuffer.VulkanBuffer;
            var vulkanVertexBufferOffset = 0ul;

            vkCmdBindPipeline(vulkanCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, vulkanPipeline);
            vkCmdBindVertexBuffers(vulkanCommandBuffer, firstBinding: 0, bindingCount: 1, (ulong*)&vulkanVertexBuffer, &vulkanVertexBufferOffset);

            var vulkanDescriptorSet = pipelineSignature.VulkanDescriptorSet;

            if (vulkanDescriptorSet != VK_NULL_HANDLE)
            {
                var inputResources = primitive.InputResources;
                var inputResourcesLength = inputResources.Length;

                for (var index = 0; index < inputResourcesLength; index++)
                {
                    var inputResource = inputResources[index];

                    VkWriteDescriptorSet writeDescriptorSet;

                    if (inputResource is VulkanGraphicsBuffer vulkanGraphicsBuffer)
                    {
                        var descriptorBufferInfo = new VkDescriptorBufferInfo {
                            buffer = vulkanGraphicsBuffer.VulkanBuffer,
                            offset = 0,
                            range = vulkanGraphicsBuffer.Size,
                        };

                        writeDescriptorSet = new VkWriteDescriptorSet {
                            sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
                            dstSet = vulkanDescriptorSet,
                            dstBinding = unchecked((uint)index),
                            descriptorCount = 1,
                            descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
                            pBufferInfo = &descriptorBufferInfo,
                        };
                    }
                    else if (inputResource is VulkanGraphicsTexture vulkanGraphicsTexture)
                    {
                        var descriptorImageInfo = new VkDescriptorImageInfo {
                            sampler = vulkanGraphicsTexture.VulkanSampler,
                            imageView = vulkanGraphicsTexture.VulkanImageView,
                            imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL,
                        };

                        writeDescriptorSet = new VkWriteDescriptorSet {
                            sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
                            dstSet = vulkanDescriptorSet,
                            dstBinding = unchecked((uint)index),
                            descriptorCount = 1,
                            descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
                            pImageInfo = &descriptorImageInfo,
                        };
                    }

                    vkUpdateDescriptorSets(Device.VulkanDevice, 1, &writeDescriptorSet, 0, pDescriptorCopies: null);
                }

                vkCmdBindDescriptorSets(vulkanCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, pipelineSignature.VulkanPipelineLayout, firstSet: 0, 1, (ulong*)&vulkanDescriptorSet, dynamicOffsetCount: 0, pDynamicOffsets: null);
            }

            var indexBuffer = (VulkanGraphicsBuffer)(primitive.IndexBufferView.Buffer);

            if (indexBuffer != null)
            {
                var indexBufferStride = primitive.IndexBufferView.Stride;
                var indexType = VK_INDEX_TYPE_UINT16;

                if (indexBufferStride != 2)
                {
                    Assert(indexBufferStride == 4, "Index Buffer has an unsupported stride.");
                    indexType = VK_INDEX_TYPE_UINT32;
                }
                vkCmdBindIndexBuffer(vulkanCommandBuffer, indexBuffer.VulkanBuffer, offset: 0, indexType);

                vkCmdDrawIndexed(vulkanCommandBuffer, indexCount: (uint)(indexBuffer.Size / indexBufferStride), instanceCount: 1, firstIndex: 0, vertexOffset: 0, firstInstance: 0);
            }
            else
            {
                vkCmdDraw(vulkanCommandBuffer, vertexCount: (uint)(vertexBuffer.Size / primitive.VertexBufferView.Stride), instanceCount: 1, firstVertex: 0, firstInstance: 0);
            }
        }

        /// <inheritdoc />
        public override void EndDrawing() => vkCmdEndRenderPass(VulkanCommandBuffer);

        /// <inheritdoc />
        public override void EndFrame()
        {
            var commandBuffer = VulkanCommandBuffer;
            ThrowExternalExceptionIfNotSuccess(nameof(vkEndCommandBuffer), vkEndCommandBuffer(commandBuffer));

            var submitInfo = new VkSubmitInfo {
                sType = VK_STRUCTURE_TYPE_SUBMIT_INFO,
                commandBufferCount = 1,
                pCommandBuffers = (IntPtr*)&commandBuffer,
            };

            var executeGraphicsFence = WaitForExecuteCompletionFence;
            ThrowExternalExceptionIfNotSuccess(nameof(vkQueueSubmit), vkQueueSubmit(Device.VulkanCommandQueue, submitCount: 1, &submitInfo, executeGraphicsFence.VulkanFence));

            executeGraphicsFence.Wait();
            executeGraphicsFence.Reset();
        }

        /// <inheritdoc />
        protected override void Dispose(bool isDisposing)
        {
            var priorState = _state.BeginDispose();

            if (priorState < Disposing)
            {
                _vulkanCommandBuffer.Dispose(DisposeVulkanCommandBuffer);
                _vulkanCommandPool.Dispose(DisposeVulkanCommandPool);
                _vulkanFramebuffer.Dispose(DisposeVulkanFramebuffer);
                _vulkanSwapChainImageView.Dispose(DisposeVulkanSwapChainImageView);

                _waitForExecuteCompletionFence?.Dispose();
                _fence?.Dispose();
            }

            _state.EndDispose();
        }

        internal void OnGraphicsSurfaceSizeChanged(object? sender, PropertyChangedEventArgs<Vector2> eventArgs)
        {
            if (_vulkanFramebuffer.IsCreated)
            {
                var vulkanFramebuffer = _vulkanFramebuffer.Value;

                if (vulkanFramebuffer != VK_NULL_HANDLE)
                {
                    vkDestroyFramebuffer(Device.VulkanDevice, vulkanFramebuffer, pAllocator: null);
                }

                _vulkanFramebuffer.Reset(CreateVulkanFramebuffer);
            }

            if (_vulkanSwapChainImageView.IsCreated)
            {
                var vulkanSwapChainImageView = _vulkanSwapChainImageView.Value;

                if (vulkanSwapChainImageView != VK_NULL_HANDLE)
                {
                    vkDestroyImageView(Device.VulkanDevice, vulkanSwapChainImageView, pAllocator: null);
                }

                _vulkanSwapChainImageView.Reset(CreateVulkanSwapChainImageView);
            }
        }

        private VkCommandBuffer CreateVulkanCommandBuffer()
        {
            VkCommandBuffer vulkanCommandBuffer;

            var commandBufferAllocateInfo = new VkCommandBufferAllocateInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
                commandPool = VulkanCommandPool,
                commandBufferCount = 1,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkAllocateCommandBuffers), vkAllocateCommandBuffers(Device.VulkanDevice, &commandBufferAllocateInfo, (IntPtr*)&vulkanCommandBuffer));

            return vulkanCommandBuffer;
        }

        private VkCommandPool CreateVulkanCommandPool()
        {
            VkCommandPool vulkanCommandPool;

            var commandPoolCreateInfo = new VkCommandPoolCreateInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
                flags = (uint)VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
                queueFamilyIndex = Device.VulkanCommandQueueFamilyIndex,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateCommandPool), vkCreateCommandPool(Device.VulkanDevice, &commandPoolCreateInfo, pAllocator: null, (ulong*)&vulkanCommandPool));

            return vulkanCommandPool;
        }

        private VkFramebuffer CreateVulkanFramebuffer()
        {
            VkFramebuffer vulkanFramebuffer;

            var device = Device;
            var surface = device.Surface;
            var swapChainImageView = VulkanSwapChainImageView;

            var frameBufferCreateInfo = new VkFramebufferCreateInfo {
                sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO,
                renderPass = device.VulkanRenderPass,
                attachmentCount = 1,
                pAttachments = (ulong*)&swapChainImageView,
                width = (uint)surface.Width,
                height = (uint)surface.Height,
                layers = 1,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateFramebuffer), vkCreateFramebuffer(device.VulkanDevice, &frameBufferCreateInfo, pAllocator: null, (ulong*)&vulkanFramebuffer));

            return vulkanFramebuffer;
        }

        private VkImageView CreateVulkanSwapChainImageView()
        {
            VkImageView swapChainImageView;

            var device = Device;

            var swapChainImageViewCreateInfo = new VkImageViewCreateInfo {
                sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                image = device.VulkanSwapchainImages[Index],
                viewType = VK_IMAGE_VIEW_TYPE_2D,
                format = device.VulkanSwapchainFormat,
                components = new VkComponentMapping {
                    r = VK_COMPONENT_SWIZZLE_R,
                    g = VK_COMPONENT_SWIZZLE_G,
                    b = VK_COMPONENT_SWIZZLE_B,
                    a = VK_COMPONENT_SWIZZLE_A,
                },
                subresourceRange = new VkImageSubresourceRange {
                    aspectMask = (uint)VK_IMAGE_ASPECT_COLOR_BIT,
                    levelCount = 1,
                    layerCount = 1,
                },
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateImageView), vkCreateImageView(device.VulkanDevice, &swapChainImageViewCreateInfo, pAllocator: null, (ulong*)&swapChainImageView));

            return swapChainImageView;
        }

        private void DisposeVulkanCommandBuffer(VkCommandBuffer vulkanCommandBuffer)
        {
            _state.AssertDisposing();

            if (vulkanCommandBuffer != null)
            {
                vkFreeCommandBuffers(Device.VulkanDevice, VulkanCommandPool, 1, (IntPtr*)&vulkanCommandBuffer);
            }
        }

        private void DisposeVulkanCommandPool(VkCommandPool vulkanCommandPool)
        {
            _state.AssertDisposing();

            if (vulkanCommandPool != VK_NULL_HANDLE)
            {
                vkDestroyCommandPool(Device.VulkanDevice, vulkanCommandPool, pAllocator: null);
            }
        }

        private void DisposeVulkanFramebuffer(VkFramebuffer vulkanFramebuffer)
        {
            _state.AssertDisposing();

            if (vulkanFramebuffer != VK_NULL_HANDLE)
            {
                vkDestroyFramebuffer(Device.VulkanDevice, vulkanFramebuffer, pAllocator: null);
            }
        }

        private void DisposeVulkanSwapChainImageView(VkImageView vulkanSwapchainImageView)
        {
            _state.AssertDisposing();

            if (vulkanSwapchainImageView != VK_NULL_HANDLE)
            {
                vkDestroyImageView(Device.VulkanDevice, vulkanSwapchainImageView, pAllocator: null);
            }
        }
    }
}
